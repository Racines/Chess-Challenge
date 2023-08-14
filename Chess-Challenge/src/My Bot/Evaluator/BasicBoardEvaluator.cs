using ChessChallenge.API;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static BrainBot;

namespace Evaluator
{
    public class BasicBoardEvaluator : BoardEvaluator
    {
        public override int Evaluate(Board board, EvalParameters parameters)
        {
            return board.HeuristicValue();
        }
    }
}
