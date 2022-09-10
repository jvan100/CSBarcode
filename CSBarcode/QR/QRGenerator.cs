using System.Text;
using CSBarcode.QR.Data;
using CSBarcode.QR.Encoders;
using CSBarcode.QR.Extensions;
using CSBarcode.QR.Models;
using CSBarcode.QR.Providers;

namespace CSBarcode.QR;

public static class QRGenerator
{

    static QRGenerator()
    {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
    }

    public static QRCode Generate(string message, int width, ErrorCorrectionLevel errorCorrectionLevel)
    {
        StringBuilder rawDataBuilder = new();
        
        EncodingMode mode = Utils.GetBestFitMode(message);
        int version = Utils.GetSmallestVersion(message, mode, errorCorrectionLevel);

        rawDataBuilder.Append(Utils.GetModeIndicator(mode));
        
        int characterCountIndicatorLength = Utils.GetCharacterCountIndicatorLength(mode, version);
        rawDataBuilder.Append(message.Length.ToLeftPaddedBinaryString(characterCountIndicatorLength));

        IModeEncoder modeEncoder = Utils.GetModeEncoder(mode);
        rawDataBuilder.Append(modeEncoder.Encode(message));

        CodewordsData codewordsData = ErrorCorrectionDataProvider.GetCodewordsData(errorCorrectionLevel, version);
        int noRequiredBits = codewordsData.TotalNoDataCodewords * 8;
        int noTerminatorBits = Math.Min(noRequiredBits - rawDataBuilder.Length, 4);
        rawDataBuilder.Append('0', noTerminatorBits);

        int noRemainingBitsToAdd = 8 - (rawDataBuilder.Length % 8);
        rawDataBuilder.Append('0', noRemainingBitsToAdd);

        int noPadBytesToAdd = (noRequiredBits - rawDataBuilder.Length) / 8;

        for (int i = 0; i < noPadBytesToAdd; i++)
        {
            rawDataBuilder.Append(Utils.GetPadByte(i));
        }

        CodewordsGroup[] dataCodewordsGroups = GenerateDataCodewordsGroups(rawDataBuilder.ToString(), codewordsData);
        CodewordsGroup[] errorCodewordsGroups = GenerateErrorCodewordsGroups(dataCodewordsGroups, codewordsData.NoECCodewordsPerBlock);

        rawDataBuilder.Clear();

        CodewordsBlock[] allDataCodewordsBlocks = dataCodewordsGroups.SelectMany(group => group.Blocks).ToArray();
        
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

        CodewordsBlock[] allErrorCodewordsBlocks = errorCodewordsGroups.SelectMany(group => group.Blocks).ToArray();

        for (int i = 0; i < codewordsData.NoECCodewordsPerBlock; i++)
        {
            foreach (CodewordsBlock errorCodewordsBlock in allErrorCodewordsBlocks)
            {
                rawDataBuilder.Append(errorCodewordsBlock.Codewords[i]);
            }
        }

        rawDataBuilder.Append('0', Utils.GetRemainderBit(version));

        return new QRCode
        {
            Mode = mode,
            RawData = rawDataBuilder.ToString(),
            Version = version,
            Width = width
        };
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

}