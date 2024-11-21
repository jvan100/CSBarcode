using System.ComponentModel;
using System.Text;
using CSBarcode.QR.Encoders;
using CSBarcode.QR.Extensions;
using CSBarcode.QR.Mask;
using CSBarcode.QR.Models;
using CSBarcode.QR.Providers;

namespace CSBarcode.QR;

public static class QRGenerator
{

    static QRGenerator()
    {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
    }

    public static string GenerateWiFiMessage(string SSID, string password, WiFiEncryption encryption, bool isHiddenNetwork = false)
    {
        string encryptionString = encryption switch
        {
            WiFiEncryption.None => "",
            WiFiEncryption.WEP  => "WEP",
            WiFiEncryption.WPA  => "WPA",
            _                   => 
                throw new InvalidEnumArgumentException("encryption", (int) encryption, typeof(WiFiEncryption))
        };

        if (encryption == WiFiEncryption.None)
        {
            password = "";
        }

        string isHiddenNetworkString = isHiddenNetwork ? "H:true;" : "";

        return $"WIFI:T:{encryptionString};S:{SSID};P:{password};{isHiddenNetworkString};";
    }
    
    public static string GenerateEmailMessage(string email, string subject = "", string body = "")
    {
        return $"mailto:{email}?subject={subject}&body={body}";
    }
    
    public static string GenerateSMSMessage(string phoneNumber = "", string message = "")
    {
        return $"SMSTO:{phoneNumber}:{message}";
    }
    
    public static string GenerateTweetMessage(string username = "", string message = "")
    {
        return $"https://twitter.com/intent/tweet?text={message}&via={username}";
    }
    
    public static string GenerateCalendarEventMessage(string title = "", string description = "", string location = "", DateTime? startDateTime = null, DateTime? endDateTime = null)
    {
        string startDateString = startDateTime?.ToString("yyyyMMddTHHmmssZ") ?? string.Empty;
        string endDateString = endDateTime?.ToString("yyyyMMddTHHmmssZ") ?? string.Empty;
        
        return $"BEGIN:VEVENT\nSUMMARY:{title}\nDESCRIPTION:{description}\nLOCATION:{location}\nDTSTART:{startDateString}\nDTEND:{endDateString}\nEND:VEVENT";
    }

    public static QRCode Generate(string message, ErrorCorrectionLevel errorCorrectionLevel, bool displayDebugInfo = false)
    {
        StringBuilder rawDataBuilder = new();
        
        EncodingMode mode = Utils.GetBestFitEncodingMode(message);
        int version = Utils.GetSmallestVersion(message, mode, errorCorrectionLevel);

        rawDataBuilder.Append(Utils.GetModeIndicator(mode));
        
        if (displayDebugInfo)
        {
            Console.WriteLine("Debug information\n-----------------");
            Console.WriteLine($"\nQR version: {version}");
            Console.WriteLine($"Error correction level: {errorCorrectionLevel.ToString()}");
            Console.WriteLine($"Encoding mode: {mode.ToString()}");
            Console.WriteLine($"\nPlain text message: {message}");
        }
        
        int characterCountIndicatorLength = Utils.GetCharacterCountIndicatorLength(mode, version);
        rawDataBuilder.Append(message.Length.ToLeftPaddedBinaryString(characterCountIndicatorLength));

        IModeEncoder modeEncoder = Utils.GetModeEncoder(mode);
        rawDataBuilder.Append(modeEncoder.Encode(message));

        CodewordsData codewordsData = ErrorCorrectionDataProvider.GetCodewordsData(errorCorrectionLevel, version);
        int noRequiredBits = codewordsData.TotalNoDataCodewords * 8;
        int noTerminatorBits = Math.Min(noRequiredBits - rawDataBuilder.Length, 4);
        rawDataBuilder.Append('0', noTerminatorBits);
        
        if (displayDebugInfo)
        {
            Console.WriteLine($"\nInput string:\n{Utils.ToByteString(rawDataBuilder.ToString())}");
        }

        int noRemainingBitsToAdd = 8 - (rawDataBuilder.Length % 8);
        
        if (noRemainingBitsToAdd != 8)
        {
            rawDataBuilder.Append('0', noRemainingBitsToAdd);
        }

        int noPadBytesToAdd = (noRequiredBits - rawDataBuilder.Length) / 8;

        for (int i = 0; i < noPadBytesToAdd; i++)
        {
            rawDataBuilder.Append(Utils.GetPadByte(i));
        }
        
        if (displayDebugInfo)
        {
            Console.WriteLine($"\nInput string with padding:\n{Utils.ToByteString(rawDataBuilder.ToString())}");
        }

        CodewordsGroup[] dataCodewordsGroups = GenerateDataCodewordsGroups(rawDataBuilder.ToString(), codewordsData);
        
        if (displayDebugInfo)
        {
            Console.Write($"\nData codewords ({codewordsData.NoDataCodewordsInGroup1Blocks} * {codewordsData.NoBlocksInGroup1} + {codewordsData.NoDataCodewordsInGroup2Blocks} * {codewordsData.NoBlocksInGroup2} = {codewordsData.TotalNoDataCodewords} total):\n[");
            
            foreach (CodewordsGroup group in dataCodewordsGroups)
            {
                if (group.Blocks.Length > 0)
                {
                    Console.Write("[");
                
                    foreach (CodewordsBlock block in group.Blocks)
                    {
                        Console.Write(string.Join(", ", block.Codewords));
                    }

                    Console.Write("]");
                }
            }

            Console.WriteLine("]");
        }
        
        CodewordsGroup[] errorCodewordsGroups = GenerateErrorCodewordsGroups(dataCodewordsGroups, codewordsData.NoECCodewordsPerBlock);
        
        if (displayDebugInfo)
        {
            int totalBlocks = codewordsData.NoBlocksInGroup1 + codewordsData.NoBlocksInGroup2;
            
            Console.Write($"\nError correction codewords ({codewordsData.NoECCodewordsPerBlock} * {totalBlocks} = {(codewordsData.NoECCodewordsPerBlock * totalBlocks)} total):\n[");
            
            foreach (CodewordsGroup group in errorCodewordsGroups)
            {
                if (group.Blocks.Length > 0)
                {
                    Console.Write("[");
                
                    foreach (CodewordsBlock block in group.Blocks)
                    {
                        Console.Write(string.Join(", ", block.Codewords));
                    }

                    Console.Write("]");
                }
            }

            Console.WriteLine("]");
        }

        rawDataBuilder.Clear();

        IEnumerable<CodewordsBlock> allDataCodewordsBlocks = dataCodewordsGroups.SelectMany(group => group.Blocks);
        
        int maxNoDataCodewordsPerBlock = Math.Max(codewordsData.NoDataCodewordsInGroup1Blocks, codewordsData.NoDataCodewordsInGroup2Blocks);
        
        for (int i = 0; i < maxNoDataCodewordsPerBlock; i++)
        {
            foreach (CodewordsBlock dataCodewordsBlock in allDataCodewordsBlocks)
            {
                try
                {
                    rawDataBuilder.Append(dataCodewordsBlock.Codewords[i]);
                }
                catch (IndexOutOfRangeException _) {}
            }
        }

        IEnumerable<CodewordsBlock> allErrorCodewordsBlocks = errorCodewordsGroups.SelectMany(group => group.Blocks);

        for (int i = 0; i < codewordsData.NoECCodewordsPerBlock; i++)
        {
            foreach (CodewordsBlock errorCodewordsBlock in allErrorCodewordsBlocks)
            {
                rawDataBuilder.Append(errorCodewordsBlock.Codewords[i]);
            }
        }

        rawDataBuilder.Append('0', RemainderBitsDataProvider.GetRemainderBit(version));

        if (displayDebugInfo)
        {
            Console.WriteLine($"\nFinal message:\n{Utils.ToByteString(rawDataBuilder.ToString())}");
        }

        byte[,] matrix = CreateMatrix(errorCorrectionLevel, version, rawDataBuilder.ToString(), displayDebugInfo);
        
        if (displayDebugInfo)
        {
            Console.WriteLine("\nFinal matrix:");
            PrintMatrix(matrix);
        }

        return new QRCode(mode, version, message, rawDataBuilder.ToString(), matrix);
    }

    private static CodewordsGroup[] GenerateDataCodewordsGroups(string rawData, CodewordsData codewordsData)
    {
        CodewordsGroup[] dataCodewordsGroups = new CodewordsGroup[2];

        int rawDataIndex = 0;

        CodewordsBlock[] dataCodewordsBlocks = GenerateDataCodewordsBlocks(rawData, ref rawDataIndex, codewordsData.NoBlocksInGroup1, codewordsData.NoDataCodewordsInGroup1Blocks);
        dataCodewordsGroups[0] = new CodewordsGroup { Blocks = dataCodewordsBlocks };
        
        dataCodewordsBlocks = GenerateDataCodewordsBlocks(rawData, ref rawDataIndex, codewordsData.NoBlocksInGroup2, codewordsData.NoDataCodewordsInGroup2Blocks);
        dataCodewordsGroups[1] = new CodewordsGroup { Blocks = dataCodewordsBlocks };

        return dataCodewordsGroups;
    }

    private static CodewordsBlock[] GenerateDataCodewordsBlocks(string rawData, ref int rawDataIndex, int noBlocksInGroup, int noDataCodewordsInBlocks)
    {
        CodewordsBlock[] dataCodewordsBlocks = new CodewordsBlock[noBlocksInGroup];

        for (int i = 0; i < noBlocksInGroup; i++)
        {
            string[] dataCodewords = new string[noDataCodewordsInBlocks];

            for (int j = 0; j < noDataCodewordsInBlocks; j++)
            {
                dataCodewords[j] = rawData.Substring(rawDataIndex, 8);
                rawDataIndex += 8;
            }
            
            dataCodewordsBlocks[i] = new CodewordsBlock { Codewords = dataCodewords };
        }

        return dataCodewordsBlocks;
    }

    private static int[] CreateGeneratorPolynomial(int noECCodewords)
    {
        int[] generatorPolynomial = { 0, 0 };

        for (int i = 1; i < noECCodewords; i++)
        {
            int[] toMultiplyBy = { 0, i };
            int[] nextGeneratorPolynomial = new int[i + 2];

            int j;

            for (j = 0; j < generatorPolynomial.Length; j++)
            {
                for (int k = 0; k < 2; k++)
                {
                    int x = (generatorPolynomial[j] + toMultiplyBy[k]) % 255;
                    nextGeneratorPolynomial[j + k] ^= Utils.Log(x);
                }

                nextGeneratorPolynomial[j] = Utils.Antilog(nextGeneratorPolynomial[j]) % 255;
            }

            nextGeneratorPolynomial[j] = Utils.Antilog(nextGeneratorPolynomial[j]) % 255;
            generatorPolynomial = nextGeneratorPolynomial;
        }
        
        return generatorPolynomial;
    }

    private static CodewordsGroup[] GenerateErrorCodewordsGroups(CodewordsGroup[] dataCodewordsGroups, int noECCodewordsPerBlock)
    {
        int[] generatorPolynomial = CreateGeneratorPolynomial(noECCodewordsPerBlock);
        
        CodewordsGroup[] errorCodewordsGroups = new CodewordsGroup[2];

        for (int i = 0; i < 2; i++)
        {
            CodewordsBlock[] errorCodewordsBlocks = GenerateErrorCodewordsBlocks(dataCodewordsGroups[i], generatorPolynomial, noECCodewordsPerBlock);
            errorCodewordsGroups[i] = new CodewordsGroup { Blocks = errorCodewordsBlocks };
        }
        
        return errorCodewordsGroups;
    }

    private static CodewordsBlock[] GenerateErrorCodewordsBlocks(CodewordsGroup dataCodewordsGroup, int[] generatorPolynomial, int noECCodewordsPerBlock)
    {
        CodewordsBlock[] dataCodewordsBlocks = dataCodewordsGroup.Blocks;
        CodewordsBlock[] errorCodewordsBlocks = new CodewordsBlock[dataCodewordsBlocks.Length];

        for (int i = 0; i < dataCodewordsBlocks.Length; i++)
        {
            string[] dataCodewords = dataCodewordsBlocks[i].Codewords;
            int[] messagePolynomial = dataCodewords.Select(codeword => Convert.ToInt32(codeword, 2)).ToArray();

            string[] errorCodewords = GenerateErrorCodewords(messagePolynomial, generatorPolynomial, noECCodewordsPerBlock);
            errorCodewordsBlocks[i] = new CodewordsBlock { Codewords = errorCodewords };
        }

        return errorCodewordsBlocks;
    }   

    private static string[] GenerateErrorCodewords(int[] messagePolynomial, int[] generatorPolynomial, int noECCodewords)
    {
        int messagePolynomialLength = messagePolynomial.Length;
        int[] newMessagePolynomial = new int[messagePolynomialLength + noECCodewords];
        Array.Copy(messagePolynomial, newMessagePolynomial, messagePolynomialLength);

        for (int i = 0; i < messagePolynomialLength; i++)
        {
            int leadTerm = Utils.Antilog(newMessagePolynomial[i]);

            for (int j = 0; j < generatorPolynomial.Length; j++)
            {
                newMessagePolynomial[i + j] ^= Utils.Log((generatorPolynomial[j] + leadTerm) % 255);
            }
        }

        string[] errorCodewords = newMessagePolynomial.Skip(messagePolynomialLength)
            .Select(term => term.ToLeftPaddedBinaryString(8))
            .ToArray();

        return errorCodewords;
    }

    private static byte[,] CreateMatrix(ErrorCorrectionLevel errorCorrectionLevel, int version, string rawData, bool displayDebugInfo)
    {
        int noOfModules = version * 4 + 17;
        
        byte[,] matrix = new byte[noOfModules, noOfModules];
        
        if (displayDebugInfo)
        {
            Console.WriteLine($"\nCreating matrix ({noOfModules} x {noOfModules})");
        }

        for (int i = 0; i < noOfModules; i++)
        {
            for (int j = 0; j < noOfModules; j++)
            {
                matrix[i, j] = ModuleColours.EMPTY;
            }
        }
        
        AddFinderPatternsToMatrix(matrix);
        AddSeparatorsToMatrix(matrix);
        AddAlignmentPatternsToMatrix(matrix, version);
        AddReservedAreasToMatrix(matrix, version);
        AddTimingPatternsToMatrix(matrix);
        AddRawDataToMatrix(matrix, rawData);
        
        if (displayDebugInfo)
        {
            Console.WriteLine("\nMatrix with raw data:");
            PrintMatrix(matrix);
        }

        int maskNo = MatrixMasker.Mask(ref matrix);
        
        if (displayDebugInfo)
        {
            Console.WriteLine($"\nMatrix with mask {maskNo} applied:");
            PrintMatrix(matrix);
        }
        
        AddFormatInfoToMatrix(matrix, errorCorrectionLevel, maskNo);

        if (version > 6)
        {
            AddVersionInfoToMatrix(matrix, version);
        }
        
        return matrix;
    }

    private static void AddFinderPatternsToMatrix(byte[,] matrix)
    {
        int shift = matrix.GetLength(0) - 7;

        // Add finder rings
        for (int i = 0; i < 3; i++)
        {
            byte colour = (i % 2 == 0) ? ModuleColours.PATTERN_FOREGROUND : ModuleColours.PATTERN_BACKGROUND;

            int boundary = 6 - i;

            for (int j = i; j < boundary; j++)
            {
                // Top left
                matrix[j, i] = colour;
                matrix[i, j + 1] = colour;
                matrix[j + 1, boundary] = colour;
                matrix[boundary, j] = colour;
                
                // Top right
                matrix[j, shift + i] = colour;
                matrix[i, shift + j + 1] = colour;
                matrix[j + 1, shift + boundary] = colour;
                matrix[boundary, shift + j] = colour;
                
                // Bottom left
                matrix[shift + j, i] = colour;
                matrix[shift + i, j + 1] = colour;
                matrix[shift + j + 1, boundary] = colour;
                matrix[shift + boundary, j] = colour;
            }
        }
        
        // Add finder centres
        matrix[3, 3] = ModuleColours.PATTERN_FOREGROUND;
        matrix[3, shift + 3] = ModuleColours.PATTERN_FOREGROUND;
        matrix[shift + 3, 3] = ModuleColours.PATTERN_FOREGROUND;
    }

    private static void AddSeparatorsToMatrix(byte[,] matrix)
    {
        const byte PATTERN_BACKGROUND = ModuleColours.PATTERN_BACKGROUND;
        
        int shift = matrix.GetLength(0) - 8;

        for (int i = 0; i < 8; i++)
        {
            // Top left
            matrix[i, 7] = PATTERN_BACKGROUND;
            matrix[7, i] = PATTERN_BACKGROUND;
            
            // Top right
            matrix[i, shift] = PATTERN_BACKGROUND;
            matrix[7, shift + i] = PATTERN_BACKGROUND;
            
            // Bottom left
            matrix[shift + i, 7] = PATTERN_BACKGROUND;
            matrix[shift, i] = PATTERN_BACKGROUND;
        }
    }

    private static void AddAlignmentPatternsToMatrix(byte[,] matrix, int version)
    {
        if (version > 1)
        {
            foreach ((int row, int col) in AlignmentLocationsDataProvider.GetAlignmentLocations(version))
            {
                int rowOffset = row - 2;
                int colOffset = col - 2;
                
                // Add alignment rings
                for (int i = 0; i < 2; i++)
                {
                    byte colour = (i % 2 == 0) ? ModuleColours.PATTERN_FOREGROUND : ModuleColours.PATTERN_BACKGROUND;
                    
                    int boundary = 4 - i;

                    for (int j = i; j < boundary; j++)
                    {
                        matrix[rowOffset + j, colOffset + i] = colour;
                        matrix[rowOffset + i, colOffset + j + 1] = colour;
                        matrix[rowOffset + j + 1, colOffset + boundary] = colour;
                        matrix[rowOffset + boundary, colOffset + j] = colour;
                    }
                }
                
                // Add centre
                matrix[row, col] = ModuleColours.PATTERN_FOREGROUND;
            }
        }
    }

    private static void AddReservedAreasToMatrix(byte[,] matrix, int version)
    {
        const byte RESERVED = ModuleColours.RESERVED;
        
        int shift = matrix.GetLength(0) - 9;
        
        for (int i = 0; i < 9; i++) {
            // Top left
            matrix[i, 8] = RESERVED;
            matrix[8, i] = RESERVED;

            // Top right
            if (i > 0) {
                matrix[8, shift + i] = RESERVED;

                // Bottom left
                if (i > 1)
                    matrix[shift + i, 8] = RESERVED;
            }
        }
        
        if (version > 6) {
            for (int i = shift; i > shift - 3; i--) {
                for (int j = 0; j < 6; j++) {
                    // Top right
                    matrix[j, i] = RESERVED;

                    // Bottom left
                    matrix[i, j] = RESERVED;
                }
            }
        }

        matrix[shift + 1, 8] = ModuleColours.PATTERN_FOREGROUND;
    }

    private static void AddTimingPatternsToMatrix(byte[,] matrix)
    {
        int shift = matrix.GetLength(0) - 9;
        
        for (int i = 8; i <= shift; i++)
        {
            byte colour = (i % 2 == 0) ? ModuleColours.PATTERN_FOREGROUND : ModuleColours.PATTERN_BACKGROUND;
            
            // Top
            matrix[6, i] = colour;

            // Left
            matrix[i, 6] = colour;
        }
    }

    private static void AddRawDataToMatrix(byte[,] matrix, string rawData)
    {
        int matrixLength = matrix.GetLength(0);
        
        int dataIndex = 0;
        int rowDirection = -1;
        int colDirection = -1;

        for (int col = matrixLength - 1; col >= 0; col -= 2)
        {
            if (col == 6)
            {
                col = 7;
                continue;
            }

            int row = (rowDirection == 1) ? 0 : matrixLength - 1;

            while (row >= 0 && row < matrixLength)
            {
                if (matrix[row, col] == ModuleColours.EMPTY)
                {
                    char bit = rawData[dataIndex++];
                    matrix[row, col] = bit == '1' ? ModuleColours.FOREGROUND : ModuleColours.BACKGROUND;
                }

                col += colDirection;

                if (colDirection == 1)
                {
                    row += rowDirection;
                }

                colDirection = -colDirection;
            }

            rowDirection = -rowDirection;
        }
    }

    private static void AddFormatInfoToMatrix(byte[,] matrix, ErrorCorrectionLevel errorCorrectionLevel, int maskNo)
    {
        int matrixLength = matrix.GetLength(0);
        
        byte[] formatInfo = FormatInfoDataProvider.GetFormatInfo(errorCorrectionLevel, maskNo);

        for (int i = 0; i < 7; i++)
        {
            int j = i != 6 ? i : 7;

            matrix[8, j] = formatInfo[i];
            matrix[matrixLength - i - 1, 8] = formatInfo[i];
        }

        for (int i = 7; i < 15; i++)
        {
            int j = i < 9 ? i : i + 1;

            matrix[15 - j, 8] = formatInfo[i];
            matrix[8, matrixLength + i - 15] = formatInfo[i];
        }
    }

    private static void AddVersionInfoToMatrix(byte[,] matrix, int version)
    {
        byte[] versionInfo = VersionInfoDataProvider.GetVersionInfo(version);

        int shift = matrix.GetLength(0) - 11;

        for (int i = 0; i < 18; i++)
        {
            int row = (17 - i) / 3;
            int col = (17 - i) % 3;

            matrix[row, col + shift] = versionInfo[i];
            matrix[col + shift, row] = versionInfo[i];
        }
    }

    private static void PrintMatrix<T>(T[,] matrix)
    {
        Console.WriteLine();
        
        for (int i = 0; i < matrix.GetLength(0); i++)
        {
            for (int j = 0; j < matrix.GetLength(1); j++)
            {
                Console.Write(matrix[i,j] + " ");
            }
            
            Console.WriteLine();
        }
    }

}