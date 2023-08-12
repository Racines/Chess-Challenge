using ChessChallenge.API;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Evaluator
{
    public abstract class BoardEvaluator
    {
        public abstract int Evaluate(Board board);
    }
}
