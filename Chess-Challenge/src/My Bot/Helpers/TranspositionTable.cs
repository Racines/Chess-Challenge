using System.Collections.Generic;

public enum ETranspositionTableNodeType
{
    Exact,
    LowerBound,
    UpperBound,
}

public struct TranspositionTableData 
{
    public int Value { get; set; }
    public ETranspositionTableNodeType NodeType { get; set; }
}

public class TranspositionTable : Dictionary<ulong, TranspositionTableData>
{
}
