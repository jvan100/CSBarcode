using CSBarcode.QR.Extensions;

namespace CSBarcode.QR.Encoders;

internal class NumericEncoder : IModeEncoder
{

    public string Encode(string message)
    {
        return string.Join("",
            message.ChunksUpTo(3)
                .Select(chunk =>
                {
                    int x = int.Parse(chunk);

                    return x switch
                    {
                        <= 9 => x.ToLeftPaddedBinaryString(4),
                        <= 99 => x.ToLeftPaddedBinaryString(7),
                        _ => x.ToLeftPaddedBinaryString(10)
                    };
                })
        );
    }

}