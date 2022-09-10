namespace CSBarcode.QR.Extensions;

internal static class IntExtensions
{

    internal static string ToLeftPaddedBinaryString(this int x, int totalLength)
    {
        return Convert.ToString(x, 2).PadLeft(totalLength, '0');
    }

}