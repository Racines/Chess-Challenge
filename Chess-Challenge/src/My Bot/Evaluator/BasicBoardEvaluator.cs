using ChessChallenge.API;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Evaluator
{
    public class BasicBoardEvaluator : BoardEvaluator
    {
        public override int Evaluate(Board board)
        {
            return board.HeuristicValue();
        }
    }
}
