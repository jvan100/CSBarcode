using CSBarcode.QR.Models;

namespace CSBarcode.QR.Data;

internal static class ErrorCorrectionData
{

    internal interface ILevelCodeWordsData  
    {
        CodewordsData[] VersionCodewordsData { get; }
    }
    
    internal class LowCodewordsData : ILevelCodeWordsData
    {
        
        public CodewordsData[] VersionCodewordsData { get; } =
        {
            new(1, 19, 0, 0, 19, 7),
            new(1, 34, 0, 0, 34, 10),
            new(1, 55, 0, 0, 55, 15),
            new(1, 80, 0, 0, 80, 20),
            new(1, 108, 0, 0, 108, 26),
            new(2, 68, 0, 0, 136, 18),
            new(2, 78, 0, 0, 156, 20),
            new(2, 97, 0, 0, 194, 24),
            new(2, 116, 0, 0, 232, 30),
            new(2, 68, 2, 69, 274, 18),
            new(4, 81, 0, 0, 324, 20),
            new(2, 92, 2, 93, 370, 24),
            new(4, 107, 0, 0, 428, 26),
            new(3, 115, 1, 116, 461, 30),
            new(5, 87, 1, 88, 523, 22)
        };
        
    }
    
    internal class MediumCodewordsData : ILevelCodeWordsData
    {
        
        public CodewordsData[] VersionCodewordsData { get; } =
        {
            new(1, 16, 0, 0, 16, 10),
            new(1, 28, 0, 0, 28, 16),
            new(1, 44, 0, 0, 44, 26),
            new(2, 32, 0, 0, 64, 18),
            new(2, 43, 0, 0, 86, 24),
            new(4, 27, 0, 0, 108, 16),
            new(4, 31, 0, 0, 124, 18),
            new(2, 38, 2, 39, 154, 22),
            new(3, 36, 2, 37, 182, 22),
            new(4, 43, 1, 44, 216, 26),
            new(1, 50, 4, 51, 254, 30),
            new(6, 36, 2, 37, 290, 22),
            new(8, 37, 1, 38, 334, 22),
            new(4, 40, 5, 41, 365, 24),
            new(5, 41, 5, 42, 415, 24)
        };

    }
    
    internal class QuartileCodewordsData : ILevelCodeWordsData
    {

        public CodewordsData[] VersionCodewordsData { get; } =
        {
            new(1, 13, 0, 0, 13, 13),
            new(1, 22, 0, 0, 22, 22),
            new(2, 17, 0, 0, 34, 18),
            new(2, 24, 0, 0, 48, 26),
            new(2, 15, 2, 16, 62, 18),
            new(4, 19, 0, 0, 76, 24),
            new(2, 14, 4, 15, 88, 18),
            new(4, 18, 2, 19, 110, 22),
            new(4, 16, 4, 17, 132, 20),
            new(6, 19, 2, 20, 154, 24),
            new(4, 22, 4, 23, 180, 28),
            new(4, 20, 6, 21, 206, 26),
            new(8, 20, 4, 21, 244, 24),
            new(11, 16, 5, 17, 261, 20),
            new(5, 24, 7, 25, 295, 30)
        };
        
    }
    
    internal class HighCodewordsData : ILevelCodeWordsData
    {
        
        public CodewordsData[] VersionCodewordsData { get; } =
        {
            new(1, 9, 0, 0, 9, 17),
            new(1, 16, 0, 0, 16, 28),
            new(2, 13, 0, 0, 26, 22),
            new(4, 9, 0, 0, 36, 16),
            new(2, 11, 2, 12, 46, 22),
            new(4, 15, 0, 0, 60, 28),
            new(4, 13, 1, 14, 66, 26),
            new(4, 14, 2, 15, 86, 26),
            new(4, 12, 4, 13, 100, 24),
            new(6, 15, 2, 16, 122, 28),
            new(3, 12, 8, 13, 140, 24),
            new(7, 14, 4, 15, 158, 28),
            new(12, 11, 4, 12, 180, 22),
            new(11, 12, 5, 13, 197, 24),
            new(11, 12, 7, 13, 223, 24)
        };
        
    }
    
}