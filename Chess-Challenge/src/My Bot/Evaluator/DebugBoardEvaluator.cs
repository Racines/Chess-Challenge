using ChessChallenge.API;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static BoardHelpers;

namespace Evaluator
{
    public class DebugBoardEvaluator : BoardEvaluator
    {
        public override int Evaluate(Board board)
        {
            Console.WriteLine(board);

            return board.Isopawn(true);
        }
    }
}
