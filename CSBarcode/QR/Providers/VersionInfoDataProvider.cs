using CSBarcode.QR.Data;

namespace CSBarcode.QR.Providers;

internal static class VersionInfoDataProvider
{

    internal static byte[] GetVersionInfo(int version)
    {
        return VersionInfoData.versionInfo[version - 7];
    }
    
}