﻿using ChessChallenge.API;
using Evaluator;
using System;

public class MiniMax1BrainBot : MiniMaxBrainBot
{
    public MiniMax1BrainBot()
        :base(1)
    {
    }
}

public class MiniMax3BrainBot : MiniMaxBrainBot
{
    public MiniMax3BrainBot()
        : base(3)
    {
    }
}

public class MiniMaxBrainBot : DepthBrainBot
{

    public MiniMaxBrainBot(int maxDepth)
        :base(maxDepth)
    {
    }

    public override int Evaluate(Board node, Timer timer, EvaluationParameters parameters)
    {
        return MiniMax(node, m_MaxDepth);
    }

    public int MiniMax(Board node, int depth)
    {
        // if max depth is reach or if node is terminal => return heuristic value of the node
        if (depth == 0 || node.IsTerminal())
        {
            var evalParams = new BoardEvaluator.EvalParameters()
            {
                Depth = depth,
            };
            return m_BoardEvaluator.Evaluate(node, evalParams);
        }

        bool maximizingPlayer = node.IsWhiteToMove;

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
            // compute the MiniMax on the current move
            node.MakeMove(move);
            value = func(value, MiniMax(node, depth - 1));
            node.UndoMove(move);
        }

        return value;
    }
}