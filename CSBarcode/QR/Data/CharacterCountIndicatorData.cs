namespace CSBarcode.QR.Data;

internal static class CharacterCountIndicatorData
{

    internal static readonly int[] numericModeLengths = { 10, 12, 14 };
    internal static readonly int[] alphanumericModeLengths = { 9, 11, 13 };
    internal static readonly int[] byteModeLengths = { 8, 16, 16 };
    internal static readonly int[] kanjiModeLengths = { 8, 10, 12 };

}