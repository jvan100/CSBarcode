namespace CSBarcode.QR.Extensions;

internal static class StringExtensions
{
    
    internal static IEnumerable<string> ChunksUpTo(this string input, int maxChunkSize)
    {
        for (int i = 0; i < input.Length; i += maxChunkSize)
        {
            yield return input.Substring(i, Math.Min(maxChunkSize, input.Length - i));
        }
    }
    
}