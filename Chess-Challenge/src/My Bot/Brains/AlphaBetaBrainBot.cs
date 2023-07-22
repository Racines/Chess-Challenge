using ChessChallenge.API;
using System;

public class AlphaBeta1BrainBot : AlphaBetaBrainBot
{
    public AlphaBeta1BrainBot()
        :base(1)
    {
    }
}

public class AlphaBeta3BrainBot : AlphaBetaBrainBot
{
    public AlphaBeta3BrainBot()
        : base(3)
    {
    }
}

public class AlphaBetaBrainBot : BrainBot
{
    private int m_MaxDepth;

    public AlphaBetaBrainBot(int maxDepth)
    {
        m_MaxDepth = maxDepth;
    }

    public override int Evaluate(Board node, Piece capturedPiece, bool isWhite)
    {
        return AlphaBeta(node, m_MaxDepth, int.MinValue, int.MaxValue, false);
    }

    public int AlphaBeta(Board node, int depth, int alpha, int beta, bool maximizingPlayer)
    {
        // if max depth is reach or if node is terminal => return heuristic value of the node
        if (depth == 0 || node.IsTerminal())
            return node.HeuristicValue(maximizingPlayer && node.IsWhiteToMove);

        // minimize or maximize the value depending on given maximizingPlayer
        int value = int.MinValue;
        Func<int, int, int> func = Math.Max;
        if (!maximizingPlayer)
        {
            value = int.MaxValue;
            func = Math.Min;
        }

        // For each legal move
        Move[] moves = node.GetLegalMoves();
        foreach (var move in moves)
        {
            // compute the AlphaBeta on the current move
            node.MakeMove(move);
            value = func(value, AlphaBeta(node, depth - 1, alpha, beta, !maximizingPlayer));
            node.UndoMove(move);

            if (maximizingPlayer)
            {
                // check beta pruning
                if (value >= beta)
                    return value;

                // update alpha value
                alpha = func(alpha, value);
            }
            else
            { 
                // check alpha pruning
                if (alpha >= value)
                return value;

                // update beta value
                beta = func(beta, value);
            }
        }

        return value;
    }
}