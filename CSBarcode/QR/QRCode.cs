using CSBarcode.QR.Extensions;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.PixelFormats;

namespace CSBarcode.QR;

public class QRCode
{

    public EncodingMode Mode { get; }
    public int Version { get; }
    public string RawData { get; }
    public string Message { get; }
    public byte[,] Matrix { get; }

    public QRCode(EncodingMode mode, int version, string message, string rawData, byte[,] matrix)
    {
        Mode = mode;
        Version = version;
        Message = message;
        RawData = rawData;
        Matrix = matrix;
    }

    public void Print()
    {
        foreach (string chunk in RawData.ChunksUpTo(8))
        {
            Console.Write($"{chunk} ");
        }

        Console.WriteLine();
    }
    
    public void Save(string filePath, int pixelsPerModule)
    {
        int qrWidth = Matrix.GetLength(0);
        int quietZoneModules = 4;
        int imageWidth = (qrWidth + quietZoneModules * 2) * pixelsPerModule;
        
        using Image<L8> image = new(imageWidth, imageWidth);
        
        L8 black = new(0);
        L8 white = new(255);
        
        image.ProcessPixelRows(pixelAccessor =>
        {
            // Add top quiet zone
            for (int pixelRow = 0; pixelRow < quietZoneModules * pixelsPerModule; pixelRow++)
            {
                Span<L8> rowSpan = pixelAccessor.GetRowSpan(pixelRow);

                for (int pixelCol = 0; pixelCol < imageWidth; pixelCol++)
                {
                    rowSpan[pixelCol] = white;
                }
            }
            
            // Add QR code
            for (int qrRow = 0; qrRow < qrWidth; qrRow++)
            {
                for (int pixelRow = (qrRow + quietZoneModules) * pixelsPerModule; pixelRow < (qrRow + quietZoneModules + 1) * pixelsPerModule; pixelRow++)
                {
                    Span<L8> rowSpan = pixelAccessor.GetRowSpan(pixelRow);
                    
                    // Add left quiet zone
                    for (int pixelCol = 0; pixelCol < quietZoneModules * pixelsPerModule; pixelCol++)
                    {
                        rowSpan[pixelCol] = white;
                    }

                    for (int qrCol = 0; qrCol < qrWidth; qrCol++)
                    {
                        L8 colour = Matrix[qrRow, qrCol] == ModuleColours.BACKGROUND ? white : black;
                        
                        for (int pixelCol = (qrCol + quietZoneModules) * pixelsPerModule; pixelCol < (qrCol + quietZoneModules + 1) * pixelsPerModule; pixelCol++)
                        {
                            rowSpan[pixelCol] = colour;
                        }
                    }
                    
                    // Add right quiet zone
                    for (int pixelCol = (qrWidth + quietZoneModules) * pixelsPerModule; pixelCol < imageWidth; pixelCol++)
                    {
                        rowSpan[pixelCol] = white;
                    }
                }
            }
            
            // Add bottom quiet zone
            for (int pixelRow = (qrWidth + quietZoneModules) * pixelsPerModule; pixelRow < imageWidth; pixelRow++)
            {
                Span<L8> rowSpan = pixelAccessor.GetRowSpan(pixelRow);
            
                for (int pixelCol = 0; pixelCol < imageWidth; pixelCol++)
                {
                    rowSpan[pixelCol] = white;
                }
            }
        });
        
        image.SaveAsPng(filePath, new PngEncoder
        {
            BitDepth = PngBitDepth.Bit1,
            ColorType = PngColorType.Grayscale
        });
        
        Console.WriteLine($"\nQR code saved to {filePath}");
    }

}