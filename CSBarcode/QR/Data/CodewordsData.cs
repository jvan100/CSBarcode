namespace CSBarcode.QR.Data;

internal sealed class CodewordsData
{
        
    internal int NoBlocksInGroup1 { get; }
    internal int NoDataCodewordsInGroup1Blocks { get; }
    internal int NoBlocksInGroup2 { get; }
    internal int NoDataCodewordsInGroup2Blocks { get; }
    internal int TotalNoDataCodewords { get; }
    internal int NoECCodewordsPerBlock { get; }

    internal CodewordsData(int noBlocksInGroup1, int noDataCodewordsInGroup1Blocks, int noBlocksInGroup2,
        int noDataCodewordsInGroup2Blocks, int totalNoDataCodewords, int noECCodewordsPerBlock)
    {
        NoBlocksInGroup1 = noBlocksInGroup1;
        NoDataCodewordsInGroup1Blocks = noDataCodewordsInGroup1Blocks;
        NoBlocksInGroup2 = noBlocksInGroup2;
        NoDataCodewordsInGroup2Blocks = noDataCodewordsInGroup2Blocks;
        TotalNoDataCodewords = totalNoDataCodewords;
        NoECCodewordsPerBlock = noECCodewordsPerBlock;
    }
        
}