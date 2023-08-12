using ChessChallenge.API;
using System;

namespace Evaluator
{
    [Serializable]
    public class BoardEvaluatorWeights
    {
        public int Pawn;
        public int Knight;
        public int Bishop;
        public int Rook;
        public int Queen;

        public int Weakcount;
        public int Enemyknightonweak;
        public int Centerpawncount;
        public int Kingpawnshield;
        public int Kingattacked;
        public int Kingdefended;
        public int Kingcastled;
        public int Bishopmob;
        public int Bishoponlarge;
        public int Bishoppair;
        public int Knightmob;
        public int Knightsupport;
        public int Knightperiphery0;
        public int Knightperiphery1;
        public int Knightperiphery2;
        public int Knightperiphery3;
        public int Isopawn;
        public int Doublepawn;
        public int Passpawn;
        public int Rookbhdpasspawn;
        public int Backwardpawn;
        public int Rankpassedpawn;
        public int Blockedpawn;
        public int Blockedpassedpawn;
        public int Rookopenfile;
        public int Rooksemiopenfile;
        public int Rookclosedfile;
        public int Rookonseventh;
        public int Rookmob;
        public int Rookcon;
        public int Queenmob;
    }

    public class AdvancedBoardEvaluator : BoardEvaluator
    {
        private BoardEvaluatorWeights m_Weights;
        private int[] m_PieceValues;

        public AdvancedBoardEvaluator(BoardEvaluatorWeights w)
        {
            m_Weights = w;
            m_PieceValues = new int [] { 0, w.Pawn, w.Knight, w.Bishop, w.Rook, w.Queen, 10000 };
        }

        public override int Evaluate(Board board)
        {
            var value = board.HeuristicValue(m_PieceValues);

            value += m_Weights.Weakcount * board.Weakcount();
            value += m_Weights.Enemyknightonweak * board.Enemyknightonweak();
            value += m_Weights.Centerpawncount * board.Centerpawncount();
            value += m_Weights.Kingpawnshield * board.Kingpawnshield();
            value += m_Weights.Kingattacked * board.Kingattacked();
            value += m_Weights.Kingdefended * board.Kingdefended();
            value += m_Weights.Kingcastled * board.Kingcastled();
            value += m_Weights.Bishopmob * board.Bishopmob();
            value += m_Weights.Bishoponlarge * board.Bishoponlarge();
            value += m_Weights.Bishoppair * board.Bishoppair();
            value += m_Weights.Knightmob * board.Knightmob();
            value += m_Weights.Knightsupport * board.Knightsupport();
            value += m_Weights.Knightperiphery0 * board.Knightperiphery(0);
            value += m_Weights.Knightperiphery1 * board.Knightperiphery(1);
            value += m_Weights.Knightperiphery2 * board.Knightperiphery(2);
            value += m_Weights.Knightperiphery3 * board.Knightperiphery(3);
            value += m_Weights.Isopawn * board.Isopawn();
            value += m_Weights.Doublepawn * board.Doublepawn();
            value += m_Weights.Passpawn * board.Passpawn();
            value += m_Weights.Rookbhdpasspawn * board.Rookbhdpasspawn();
            value += m_Weights.Backwardpawn * board.Backwardpawn();
            value += m_Weights.Rankpassedpawn * board.Rankpassedpawn();
            value += m_Weights.Blockedpawn * board.Blockedpawn();
            value += m_Weights.Blockedpassedpawn * board.Blockedpassedpawn();
            value += m_Weights.Rookopenfile * board.Rookopenfile();
            value += m_Weights.Rooksemiopenfile * board.Rooksemiopenfile();
            value += m_Weights.Rookclosedfile * board.Rookclosedfile();
            value += m_Weights.Rookonseventh * board.Rookonseventh();
            value += m_Weights.Rookmob * board.Rookmob();
            value += m_Weights.Rookcon * board.Rookcon();
            value += m_Weights.Queenmob * board.Queenmob();

            return value;
        }
    }
}
