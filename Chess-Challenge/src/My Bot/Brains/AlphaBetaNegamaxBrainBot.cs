using ChessChallenge.API;
using Evaluator;
using System;

public class AlphaBetaNegamax0BrainBot : AlphaBetaNegamaxBrainBot
{
    public AlphaBetaNegamax0BrainBot()
       : base(0)
    { }
}
public class AlphaBetaNegamax1BrainBot : AlphaBetaNegamaxBrainBot
{
    public AlphaBetaNegamax1BrainBot()
       : base(1)
    { }
}
public class AlphaBetaNegamax2BrainBot : AlphaBetaNegamaxBrainBot
{
    public AlphaBetaNegamax2BrainBot()
       : base(2)
    { }
}

public class AlphaBetaNegamax3BrainBot : AlphaBetaNegamaxBrainBot
{
    public AlphaBetaNegamax3BrainBot()
       : base(3)
    { }
}

public class AlphaBetaNegamaxBrainBot : BrainBot
{
    public AlphaBetaNegamaxBrainBot()
    {
    }

    public AlphaBetaNegamaxBrainBot(int maxDepth)
    {
        m_MaxDepth = maxDepth;
    }

    public AlphaBetaNegamaxBrainBot(int minDepth, int maxDepth)
    {
        m_MaxDepth = maxDepth;
        m_MinDepth = minDepth;
    }

    public override int Evaluate(Board node, Timer timer, EvaluationParameters parameters)
    {
        m_TranspositionTable.Clear();
        var colorMult = node.IsWhiteToMove ? 1 : -1;
        return colorMult * Negamax(node, parameters.Depth, -1_000_000, 1_000_000);
    }

    public int Negamax(Board node, int depth, int alpha, int beta)
    {
        int value = int.MinValue;

        // if max depth is reach or if node is terminal => return heuristic value of the node
        if (depth == 0 || node.IsTerminal())
        {
            var evalParameters = new BoardEvaluator.EvalParameters()
            {
                Alpha = alpha,
                Beta = beta,
                Depth = depth,
            };
            value = m_BoardEvaluator.Evaluate(node, evalParameters);
            var colorMult = node.IsWhiteToMove ? 1 : -1;
            return colorMult * value;
        }        

        // For each legal move
        Move[] moves = node.GetLegalMoves();
        // optionnal order
        var orderedMoves = m_MoveOrderer.OrderMoves(node, moves);

        foreach (var move in orderedMoves)
        {
            // compute the AlphaBeta on the current move
            node.MakeMove(move);
            value = Math.Max(value, -Negamax(node, depth - 1, -beta, -alpha));
            node.UndoMove(move);

            // update alpha value
            alpha = Math.Max(alpha, value);
            
            if (alpha >= beta) 
                break; // cut-off

        }

        return value;
    }
}