using Evaluator;

public static class Evaluators
{
    public static AdvancedBoardEvaluator s_AdvancedEvaluator = 
        new (BoardEvaluatorWeights.Deserialize("../../../../Weights/boardEvaluatorWeights.json"));
}
