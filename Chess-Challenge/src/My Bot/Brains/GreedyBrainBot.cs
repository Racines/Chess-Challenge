using ChessChallenge.API;
using Evaluator;
using System;


public class GreedyAdvancedEvalBrainBot : BrainBot
{
    public override int Evaluate(Board node, Timer timer, EvaluationParameters parameters)
    {
        var evalParams = new BoardEvaluator.EvalParameters();
        return Evaluators.s_AdvancedEvaluator.Evaluate(node, evalParams);
    }
}

public class GreedyBrainBot : BrainBot
{
    public override int Evaluate(Board node, Timer timer, EvaluationParameters parameters)
    {
        return parameters.Move.CapturePieceType.Value();
    }
}