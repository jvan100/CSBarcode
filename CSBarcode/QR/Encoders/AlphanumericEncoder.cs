using CSBarcode.QR.Extensions;

namespace CSBarcode.QR.Encoders;

internal class AlphanumericEncoder : IModeEncoder
{
    
    public string Encode(string message)
    {
        return string.Join("",
            message.ChunksUpTo(2)
                .Select(chunk =>
                {
                    int value = Utils.ToAlphanumericValue(chunk[0]);
                    int paddedLength = 6;

                    if (chunk.Length == 2)
                    {
                        value = value * 45 + Utils.ToAlphanumericValue(chunk[1]);
                        paddedLength = 11;
                    }

                    return value.ToLeftPaddedBinaryString(paddedLength);
                })
        );
    }
    
}