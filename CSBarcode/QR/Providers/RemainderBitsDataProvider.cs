using CSBarcode.QR.Data;

namespace CSBarcode.QR.Providers;

internal static class RemainderBitsDataProvider
{
    
    internal static int GetRemainderBit(int version)
    {
        return RemainderBitsData.remainderBits[version - 1];
    }
    
}