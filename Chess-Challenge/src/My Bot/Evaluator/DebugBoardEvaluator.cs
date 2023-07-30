using ChessChallenge.API;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chess_Challenge.src.My_Bot.Evaluator
{
    public class DebugBoardEvaluator : BoardEvaluator
    {
        public override int Evaluate(Board board)
        {
            return board.Centerpawncount(true);
        }
    }
}
