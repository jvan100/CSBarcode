using System.Text;
using System.Text.RegularExpressions;
using CSBarcode.QR.Data;
using CSBarcode.QR.Encoders;
using CSBarcode.QR.Exceptions;
using CSBarcode.QR.Providers;

namespace CSBarcode.QR;

internal static class Utils
{

    private static readonly Regex _numericRegex = new(@"^\d+$");
    private static readonly Regex _alphanumericRegex = new(@"^[\dA-Z$%*+-.\/: ]+$");

    internal static EncodingMode GetBestFitMode(string message)
    {
        return message switch
        {
            not null when IsValidNumericString(message)      => EncodingMode.Numeric,
            not null when IsValidAlphanumericString(message) => EncodingMode.Alphanumeric,
            not null when IsValidISOString(message)          => EncodingMode.Byte,
            not null when IsValidShiftJISString(message)     => EncodingMode.Kanji,
            _                                                => 
                throw new NotImplementedException($"Input message \"{message}\" does not use a QR recognised encoding.")
        };
    }

    private static bool IsValidNumericString(string input)
    {
        return _numericRegex.IsMatch(input);
    }

    private static bool IsValidAlphanumericString(string input)
    {
        return _alphanumericRegex.IsMatch(input);
    }

    private static bool IsValidISOString(string input)
    {
        return CheckStringUsesEncoding(input, 28591);
    }

    private static bool IsValidShiftJISString(string input)
    {
        return CheckStringUsesEncoding(input, 932);
    }

    private static bool CheckStringUsesEncoding(string input, int encodingIdentifier)
    {
        Encoding encoding = Encoding.GetEncoding(encodingIdentifier);
        byte[] bytes = encoding.GetBytes(input);
        string encodedString = encoding.GetString(bytes);
        return input == encodedString;
    }

    internal static string GetModeIndicator(EncodingMode mode)
    {
        return mode switch
        {
            EncodingMode.Numeric      => EncodingModeData.NUMERIC_INDICATOR,
            EncodingMode.Alphanumeric => EncodingModeData.ALPHANUMERIC_INDICATOR,
            EncodingMode.Byte         => EncodingModeData.BYTE_INDICATOR,
            EncodingMode.Kanji        => EncodingModeData.KANJI_INDICATOR,
            _                 => 
                throw new NotImplementedException($"QR encoding mode \"{nameof(mode)}\" functionality has not been implemented.")
        };
    }
    
    internal static int GetSmallestVersion(string message, EncodingMode mode, ErrorCorrectionLevel errorCorrectionLevel)
    {
        int[] characterCapacities = CharacterCapacitiesDataProvider.GetCharacterCapacities(mode, errorCorrectionLevel);

        int messageLength = message.Length;

        for (int i = 0; i < 40; i++)
        {
            if (messageLength <= characterCapacities[i])
            {
                return i + 1;
            }
        }

        throw new MessageTooLongException($"Message \"{message}\" is too long to be used in a QR code.");
    }

    internal static int GetCharacterCountIndicatorLength(EncodingMode mode, int version)
    {
        int[] characterCountIndicatorLengths = mode switch
        {
            EncodingMode.Numeric      => CharacterCountIndicatorData.numericModeLengths,
            EncodingMode.Alphanumeric => CharacterCountIndicatorData.alphanumericModeLengths,
            EncodingMode.Byte         => CharacterCountIndicatorData.byteModeLengths,
            EncodingMode.Kanji        => CharacterCountIndicatorData.kanjiModeLengths,
            _                 => 
                throw new NotImplementedException($"QR encoding mode \"{nameof(mode)}\" functionality has not been implemented.")
        };

        return version switch
        {
            <= 9  => characterCountIndicatorLengths[0],
            <= 26 => characterCountIndicatorLengths[1],
            _     => characterCountIndicatorLengths[2]
        };
    }

    internal static IModeEncoder GetModeEncoder(EncodingMode mode)
    {
        return mode switch
        {
            EncodingMode.Numeric      => new NumericEncoder(),
            EncodingMode.Alphanumeric => new AlphanumericEncoder(),
            EncodingMode.Byte         => new ByteModeEncoder(),
            EncodingMode.Kanji        => new KanjiModeEncoder(),
            _                         => 
                throw new NotImplementedException($"QR encoding mode \"{nameof(mode)}\" functionality has not been implemented.")
        };
    }

    internal static int ToAlphanumericValue(char c)
    {
        return AlphanumericData.values[c];
    }

    internal static byte[] EncodeString(string input, int encodingIdentifier)
    {
        return Encoding.GetEncoding(encodingIdentifier).GetBytes(input);
    }

    internal static string GetPadByte(int n)
    {
        return (n % 2 == 0) ? PadBytesData.padBytes[0] : PadBytesData.padBytes[1];
    }

    internal static int Log(int x)
    {
        return LogAntilogData.log[x];
    }

    internal static int Antilog(int x)
    {
        return LogAntilogData.antilog[x];
    }

    internal static int GetRemainderBit(int version)
    {
        return RemainderBitsData.remainderBits[version - 1];
    }
    
}