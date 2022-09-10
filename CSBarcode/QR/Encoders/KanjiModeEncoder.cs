using CSBarcode.QR.Extensions;

namespace CSBarcode.QR.Encoders;

internal class KanjiModeEncoder : IModeEncoder
{
    
    public string Encode(string message)
    {
        return string.Join("",
            message.ChunksUpTo(1)
                .Select(chunk =>
                {
                    byte[] bytes = Utils.EncodeString(chunk, 932);

                    if (bytes[0] <= 0x9f && bytes[1] <= 0xfc)
                    {
                        bytes[0] -= 0x81;
                    }
                    else
                    {
                        bytes[0] -= 0xc1;
                    }

                    bytes[1] -= 0x40;

                    int value = bytes[0] * 0xc0 + bytes[1];
                    return value.ToLeftPaddedBinaryString(13);
                })
        );
    }
    
}