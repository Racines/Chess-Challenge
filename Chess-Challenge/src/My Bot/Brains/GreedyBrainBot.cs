using ChessChallenge.API;
using System;


public class GreedyAdvancedEvalBrainBot : BrainBot
{
    public override int Evaluate(Board node, Timer timer, EvaluationParameters parameters)
    {
        return Evaluators.s_AdvancedEvaluator.Evaluate(node);
    }
}

public class GreedyBrainBot : BrainBot
{
    public override int Evaluate(Board node, Timer timer, EvaluationParameters parameters)
    {
        return parameters.Move.CapturePieceType.Value();
    }
}