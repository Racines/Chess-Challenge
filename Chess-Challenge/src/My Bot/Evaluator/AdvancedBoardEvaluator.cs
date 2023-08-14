using ChessChallenge.API;
using System;
using System.Text.Json;

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

        public static BoardEvaluatorWeights Deserialize(string path)
        {
            var options = new JsonSerializerOptions { IncludeFields = true, };
            var json = System.IO.File.ReadAllText(path);
            return JsonSerializer.Deserialize<BoardEvaluatorWeights>(json, options)!;
        }
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

        public BoardEvaluatorWeights Weights => m_Weights;

        public override int Evaluate(Board board)
        {
            // Heuristic value for checkmate 
            if (board.IsInCheckmate())
                return board.IsWhiteToMove ? int.MinValue : int.MaxValue;

            // Heuristic value for draw 
            if (board.IsDraw())
                return 0;

            // Try to evaluate board position
            var value = board.PiecesValue(m_PieceValues);
            value += SmartWeight(board, m_Weights.Weakcount, BoardHelpers.Weakcount);
            value += SmartWeight(board, m_Weights.Centerpawncount, BoardHelpers.Centerpawncount);
            value += SmartWeight(board, m_Weights.Kingpawnshield, BoardHelpers.Kingpawnshield);
            value += SmartWeight(board, m_Weights.Kingattacked, BoardHelpers.Kingattacked);
            value += SmartWeight(board, m_Weights.Kingdefended, BoardHelpers.Kingdefended);
            value += SmartWeight(board, m_Weights.Kingcastled, BoardHelpers.Kingcastled);
            value += SmartWeight(board, m_Weights.Bishopmob, BoardHelpers.Bishopmob);
            value += SmartWeight(board, m_Weights.Bishoponlarge, BoardHelpers.Bishoponlarge);
            value += SmartWeight(board, m_Weights.Bishoppair, BoardHelpers.Bishoppair);
            value += SmartWeight(board, m_Weights.Knightmob, BoardHelpers.Knightmob);
            value += SmartWeight(board, m_Weights.Knightsupport, BoardHelpers.Knightsupport);
            value += SmartWeight(board, m_Weights.Knightperiphery0, 0, BoardHelpers.Knightperiphery);
            value += SmartWeight(board, m_Weights.Knightperiphery1, 1, BoardHelpers.Knightperiphery);
            value += SmartWeight(board, m_Weights.Knightperiphery2, 2, BoardHelpers.Knightperiphery);
            value += SmartWeight(board, m_Weights.Knightperiphery3, 3, BoardHelpers.Knightperiphery);
            value += SmartWeight(board, m_Weights.Isopawn, BoardHelpers.Isopawn);
            value += SmartWeight(board, m_Weights.Doublepawn, BoardHelpers.Doublepawn);
            value += SmartWeight(board, m_Weights.Backwardpawn, BoardHelpers.Backwardpawn);
            value += SmartWeight(board, m_Weights.Blockedpawn, BoardHelpers.Blockedpawn);
            value += SmartWeight(board, m_Weights.Rookonseventh, BoardHelpers.Rookonseventh);
            value += SmartWeight(board, m_Weights.Rookmob, BoardHelpers.Rookmob);
            value += SmartWeight(board, m_Weights.Rookcon, BoardHelpers.Rookcon);
            value += SmartWeight(board, m_Weights.Queenmob, BoardHelpers.Queenmob);

            // Heavy cost, could be optimised by storing passpawn and rook status
            //value += SmartWeight(board, m_Weights.Passpawn, BoardHelpers.Passpawn);
            //value += SmartWeight(board, m_Weights.Rookbhdpasspawn, BoardHelpers.Rookbhdpasspawn);
            //value += SmartWeight(board, m_Weights.Rankpassedpawn, BoardHelpers.Rankpassedpawn);
            //value += SmartWeight(board, m_Weights.Blockedpassedpawn, BoardHelpers.Blockedpassedpawn);
            //value += SmartWeight(board, m_Weights.Rookopenfile, BoardHelpers.Rookopenfile);
            //value += SmartWeight(board, m_Weights.Rooksemiopenfile, BoardHelpers.Rooksemiopenfile);
            //value += SmartWeight(board, m_Weights.Rookclosedfile, BoardHelpers.Rookclosedfile);

            return value;
        }

        private int SmartWeight(Board board, int weight, Func<Board, int> evaluator)
        {
            if (weight == 0)
                return 0;

            return weight * evaluator(board);
        }
        private int SmartWeight(Board board, int weight, int param, Func<Board, int, int> evaluator)
        {
            if (weight == 0)
                return 0;

            return weight * evaluator(board, param);
        }
    }
}
