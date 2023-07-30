using ChessChallenge.API;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chess_Challenge.src.My_Bot.Evaluator
{
    public abstract class BoardEvaluator
    {
        public abstract int Evaluate(Board board);
    }
}
