﻿using ChessChallenge.API;
using System;
using System.Collections.Generic;

public class AlphaBeta1BrainBot : AlphaBetaOrderedBrainBot
{
    public AlphaBeta1BrainBot()
        :base(1)
    {
    }
}

public class AlphaBeta2BrainBot : AlphaBetaOrderedBrainBot
{
    public AlphaBeta2BrainBot()
        : base(2)
    {
    }
}

public class AlphaBeta3BrainBot : AlphaBetaOrderedBrainBot
{
    public AlphaBeta3BrainBot()
        : base(3)
    {
    }
}

public class AlphaBeta4BrainBot : AlphaBetaOrderedBrainBot
{
    public AlphaBeta4BrainBot()
        : base(4)
    {
    }
}

public class AlphaBeta5BrainBot : AlphaBetaOrderedBrainBot
{
    public AlphaBeta5BrainBot()
        : base(5)
    {
    }
}

public class AlphaBeta5NoTranspositionBrainBot : AlphaBeta5BrainBot
{
    public AlphaBeta5NoTranspositionBrainBot()
        : base()
    {
        m_UseTranspositionTable = false;
    }
}

public class AlphaBetaBrainBot : DepthBrainBot
{
    public AlphaBetaBrainBot(int maxDepth)
        :base(maxDepth) 
    {
    }

    public override int Evaluate(Board node, Timer timer, Move move, bool isWhite)
    {
        return AlphaBeta(node, m_MaxDepth, int.MinValue, int.MaxValue);
    }

    public int AlphaBeta(Board node, int depth, int alpha, int beta)
    {
        int value = int.MinValue;
        if (m_UseTranspositionTable && m_TranspositionTable.TryGetValue(node.ZobristKey, out value))
            return value;

        var maximizingPlayer = node.IsWhiteToMove;

        // if max depth is reach or if node is terminal => return heuristic value of the node
        if (depth == 0 || node.IsTerminal())
        {
            value = HeuristicValue(node);
            if (m_UseTranspositionTable)
                m_TranspositionTable.Add(node.ZobristKey, value);
            return value;
        }

        // minimize or maximize the value depending on given maximizingPlayer
        Func<int, int, int> func = Math.Max; 
        value = int.MinValue;
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
            value = func(value, AlphaBeta(node, depth - 1, alpha, beta));
            node.UndoMove(move);

            if (maximizingPlayer)
            {
                // check beta pruning
                if (value >= beta) 
                    break;

                // update alpha value
                alpha = func(alpha, value);
            }
            else
            {
                // check alpha pruning
                if (alpha >= value)
                    break;

                // update beta value
                beta = func(beta, value);
            }
        }

        if (m_UseTranspositionTable)
            m_TranspositionTable.TryAdd(node.ZobristKey, value);
        return value;
    }
}