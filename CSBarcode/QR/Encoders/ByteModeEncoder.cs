using CSBarcode.QR.Extensions;

namespace CSBarcode.QR.Encoders;

internal class ByteModeEncoder : IModeEncoder
{
    
    public string Encode(string message)
    {
        return string.Join("",
            message.ToCharArray()
                .Select(c => ((int)c).ToLeftPaddedBinaryString(8))
        );
    }
    
}