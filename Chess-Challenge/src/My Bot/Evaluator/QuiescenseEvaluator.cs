using ChessChallenge.API;
using System.Xml.Linq;

namespace Evaluator
{
    public class QuiescenseBasicEvaluator : QuiescenseEvaluator<BasicBoardEvaluator>
    { }

    public class QuiescenseEvaluator<T> : BoardEvaluator where T : BoardEvaluator, new()
    {
        private T m_Evaluator = new();

        public override int Evaluate(Board board, EvalParameters parameters)
        {
            if (parameters.Depth != 0)
                return m_Evaluator.Evaluate(board, parameters);

            return Quiescense(board, parameters);
        }

        private int Quiescense(Board board, EvalParameters parameters)
        {
            var standardEval = m_Evaluator.Evaluate(board, parameters);

            var maximizingPlayer = board.IsWhiteToMove;

            if (maximizingPlayer)
            {
                if (standardEval >= parameters.Beta)
                    return parameters.Beta;

                if (parameters.Alpha < standardEval)
                    parameters.Alpha = standardEval;
            }
            else
            {
                if (standardEval <= parameters.Alpha)
                    return parameters.Alpha;

                if (parameters.Beta > standardEval)
                    parameters.Beta = standardEval;
            }

            Move[] captureMoves = board.GetLegalMoves(true);
            foreach (var move in captureMoves)
            {
                board.MakeMove(move);
                int score = Quiescense(board, parameters);
                board.UndoMove(move);

                if (maximizingPlayer)
                {
                    if (score >= parameters.Beta)
                        return parameters.Beta;

                    if (parameters.Alpha < score)
                        parameters.Alpha = score;
                }
                else
                {
                    if (score <= parameters.Alpha)
                        return parameters.Alpha;

                    if (parameters.Beta > score)
                        parameters.Beta = score;
                }
            }

            if (maximizingPlayer)
                return parameters.Alpha;
            else
                return parameters.Beta;
        }
    }
}
