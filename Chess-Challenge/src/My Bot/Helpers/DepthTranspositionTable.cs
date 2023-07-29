using ChessChallenge.API;
using System.Collections.Generic;

public class DepthTranspositionTable : Dictionary<int, TranspositionTable>
{
    public bool TryGetValue(Board board, int depth, int alpha, int beta, out int value)
    {
        value = 0;

        if (!TryGetValue(depth, out var transpositionTable))
            return false;

        if (!transpositionTable.TryGetValue(board.ZobristKey, out var data))
            return false;

        value = data.Value;

        // We have stored the exact evaluation for this position, so return it
        if (data.NodeType == ETranspositionTableNodeType.Exact)
            return true;

        // We have stored the upper bound of the eval for this position. If it's less than alpha then we don't need to
        // search the moves in this position as they won't interest us; otherwise we will have to search to find the exact value
        if (data.NodeType == ETranspositionTableNodeType.UpperBound && value <= alpha)
            return true;


        // We have stored the lower bound of the eval for this position. Only return if it causes a beta cut-off.
        if (data.NodeType == ETranspositionTableNodeType.LowerBound && value >= beta)
            return true;

        value = 0;
        return false;
    }

    public void TryAdd(Board board, int depth, int value, ETranspositionTableNodeType nodeType)
    {
        if (!TryGetValue(depth, out var transpositionTable))
        {
            transpositionTable = new();
            Add(depth, transpositionTable);
        }

        var data = new TranspositionTableData()
        {
            Value = value,
            NodeType = nodeType,
        };

        transpositionTable.TryAdd(board.ZobristKey, data);
    }
}
