using CSBarcode.QR.Extensions;

namespace CSBarcode.QR;

public class QRCode
{

    public EncodingMode Mode { get; }
    public int Version { get; }
    public string RawData { get; }
    public int Width { get; private set; }

    public QRCode(EncodingMode mode, int version, string rawData, int width)
    {
        Mode = mode;
        Version = version;
        RawData = rawData;
        Width = width;
    }

    public void Print()
    {
        foreach (string chunk in RawData.ChunksUpTo(8))
        {
            Console.Write($"{chunk} ");
        }

        Console.WriteLine();
    }

    public void ResizeTo(int width)
    {
        Width = width;
    }

}