using CSBarcode.QR.Data;

namespace CSBarcode.QR.Providers;

internal static class AlignmentLocationsDataProvider
{
    
    internal static (int, int)[] GetAlignmentLocations(int version)
    {
        return AlignmentLocationsData.alignmentLocations[version - 2];
    }
    
}