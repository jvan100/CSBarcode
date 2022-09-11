using CSBarcode.QR.Data;

namespace CSBarcode.QR.Providers;

internal static class FormatInfoDataProvider
{

    internal static byte[] GetFormatInfo(ErrorCorrectionLevel errorCorrectionLevel, int maskNo)
    {
        byte[][] levelFormatInfo = errorCorrectionLevel switch
        {
            ErrorCorrectionLevel.Low      => FormatInfoData.LowFormatInfo,
            ErrorCorrectionLevel.Medium   => FormatInfoData.MediumFormatInfo,
            ErrorCorrectionLevel.Quartile => FormatInfoData.QuartileFormatInfo,
            ErrorCorrectionLevel.High     => FormatInfoData.HighFormatInfo,
            _                             =>
                throw new NotImplementedException($"QR error correction level \"{nameof(errorCorrectionLevel)}\" functionality has not been implemented.")
        };

        return levelFormatInfo[maskNo];
    }
    
}