using static CSBarcode.QR.Data.CharacterCapacitiesData;

namespace CSBarcode.QR.Providers;

internal static class CharacterCapacitiesDataProvider
{

    internal static int[] GetCharacterCapacities(EncodingMode encodingMode, ErrorCorrectionLevel errorCorrectionLevel)
    {
        IModeCharacterCapacities characterCapacities = encodingMode switch
        {
            EncodingMode.Numeric      => new NumericCharacterCapacities(),
            EncodingMode.Alphanumeric => new AlphanumericCharacterCapacities(),
            EncodingMode.Byte         => new ByteCharacterCapacities(),
            EncodingMode.Kanji        => new KanjiCharacterCapacities(),
            _                         => 
                throw new NotImplementedException($"QR mode \"{nameof(encodingMode)}\" functionality has not been implemented.")
        };

        return errorCorrectionLevel switch
        {
            ErrorCorrectionLevel.Low      => characterCapacities.LowLimits,
            ErrorCorrectionLevel.Medium   => characterCapacities.MediumLimits,
            ErrorCorrectionLevel.Quartile => characterCapacities.QuartileLimits,
            ErrorCorrectionLevel.High     => characterCapacities.HighLimits,
            _                             => 
                throw new NotImplementedException($"QR error correction level \"{nameof(errorCorrectionLevel)}\" functionality has not been implemented.")
        };
    }
    
}