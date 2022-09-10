using CSBarcode.QR.Extensions;

namespace CSBarcode.QR;

public class QRCode
{

    public EncodingMode Mode { get; init; }
    public string RawData { get; init; }
    public int Version { get; init; }
    public int Width { get; init; }

    public void Print()
    {
        foreach (string chunk in RawData.ChunksUpTo(8))
        {
            Console.Write($"{chunk} ");
        }

        Console.WriteLine();
    }

}