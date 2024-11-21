using CSBarcode.QR.Models;
using static CSBarcode.QR.Data.ErrorCorrectionData;

namespace CSBarcode.QR.Providers;

internal static class ErrorCorrectionDataProvider
{

    internal static CodewordsData GetCodewordsData(ErrorCorrectionLevel errorCorrectionLevel, int version)
    {
        ILevelCodeWordsData levelCodeWordsData = errorCorrectionLevel switch
        {
            ErrorCorrectionLevel.Low      => new LowCodewordsData(),
            ErrorCorrectionLevel.Medium   => new MediumCodewordsData(),
            ErrorCorrectionLevel.Quartile => new QuartileCodewordsData(),
            ErrorCorrectionLevel.High     => new HighCodewordsData(),
            _                             => 
                throw new NotImplementedException($"QR error correction level \"{nameof(errorCorrectionLevel)}\" functionality has not been implemented.")
        };

        return levelCodeWordsData.VersionCodewordsData[version - 1];
    }
    
}