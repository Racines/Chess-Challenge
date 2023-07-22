using ChessChallenge.API;
using System;
using System.Linq;

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


public class AlphaBeta4BrainBot : AlphaBetaBrainBot
{
    public AlphaBeta4BrainBot()
        : base(4)
    {
    }
}

public class AlphaBeta5BrainBot : AlphaBetaBrainBot
{
    public AlphaBeta5BrainBot()
        : base(5)
    {
    }
}

public class AlphaBeta3OrderedBrainBot : AlphaBetaOrderedBrainBot
{
    public AlphaBeta3OrderedBrainBot()
        : base(3)
    {
    }
}

public class AlphaBeta4OrderedBrainBot : AlphaBetaOrderedBrainBot
{
    public AlphaBeta4OrderedBrainBot()
        : base(4)
    {
    }
}

public class AlphaBeta5OrderedBrainBot : AlphaBetaOrderedBrainBot
{
    public AlphaBeta5OrderedBrainBot()
        : base(5)
    {
    }
}

public class AlphaBetaOrderedBrainBot : AlphaBetaBrainBot
{
    const int squareControlledByOpponentPawnPenalty = 350;
    const int c_CapturedPieceValueMultiplier = 10;

    public AlphaBetaOrderedBrainBot(int maxDepth)
        :base(maxDepth)
    {
    }

    protected override Move[] OrderMoves(Board board, Move[] allMoves)
    {
        return allMoves.OrderByDescending(move =>
        {
            int score = 0;
            var movePieceType = board.GetPiece(move.TargetSquare).PieceType;
            var capturePieceType = board.GetPiece(move.TargetSquare).PieceType;

            if (capturePieceType != PieceType.None)
            {
                // Order moves to try capturing the most valuable opponent piece with least valuable of own pieces first
                // The capturedPieceValueMultiplier is used to make even 'bad' captures like QxP rank above non-captures
                score += c_CapturedPieceValueMultiplier * movePieceType.Value() - movePieceType.Value();
            }

            if (move.IsPromotion)
            {
                score += move.PromotionPieceType.Value();
            }
            else
            {
                // Penalize moving piece to a square attacked by opponent pawn
                if (board.SquareIsAttackedByOpponent(move.TargetSquare))
                    score -= squareControlledByOpponentPawnPenalty;
            }

            return score;
        }).ToArray();
    }
}

public class AlphaBetaBrainBot : BrainBot
{
    private int m_MaxDepth;

    public AlphaBetaBrainBot(int maxDepth)
    {
        m_MaxDepth = maxDepth;
    }

    public override int Evaluate(Board node, Move move, bool isWhite)
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
        // optionnal order
        var orderedMoves = OrderMoves(node, moves);

        foreach (var move in orderedMoves)
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