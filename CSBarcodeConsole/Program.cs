using CSBarcode.QR;

namespace CSBarcodeConsole;

public class Program
{
    public static void Main(string[] args)
    {
        //string wifiMessage = QRGenerator.GenerateWiFiMessage("Get_Off_My_LAN", "R4nV5q4Hb9uZ", WiFiEncryption.WPA);
        //Console.WriteLine(wifiMessage);
        QRCode qr = QRGenerator.Generate("HELLO THERE MAN HAJSH SAJDH SGDHGS HDGS D GSH", 400, ErrorCorrectionLevel.Quartile);
        //qr.Print();
    }
}