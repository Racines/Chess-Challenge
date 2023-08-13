using ChessChallenge.API;
using System;


public class GreedyAdvancedEvalBrainBot : BrainBot
{
    public override int Evaluate(Board node, Timer timer, Move move, bool isWhite)
    {
        return Evaluators.s_AdvancedEvaluator.Evaluate(node);
    }
}

public class GreedyBrainBot : BrainBot
{
    public override int Evaluate(Board node, Timer timer, Move move, bool isWhite)
    {
        return move.CapturePieceType.Value();
    }
}