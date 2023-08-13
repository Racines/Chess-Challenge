using Evaluator;
using ChessChallenge.API;
using System;

public abstract class BrainBot : IChessBot
{
    public struct EvaluationParameters
    {
        public Move Move;
        public bool IsWhite;
    }

    protected DepthTranspositionTable m_TranspositionTable = new ();
    protected bool m_UseTranspositionTable = true;
    protected BoardEvaluator m_BoardEvaluator = new BasicBoardEvaluator();
    protected BoardEvaluator m_DebugBoardEvaluator = new DebugBoardEvaluator();


    public bool UseTranspositionTable { get => m_UseTranspositionTable; set => m_UseTranspositionTable = value; }

    public Move Think(Board board, Timer timer)
    {
        m_DebugBoardEvaluator?.Evaluate(board);

        var scoredMoves = EvaluateLegalMoves(board, timer);

        // return best move
        return scoredMoves.GetBestMove(board.IsWhiteToMove);
    }

    public ScoredMoves EvaluateLegalMoves(Board board, Timer timer, bool breakAtCheckMate = true)
    {
        Move[] allMoves = board.GetLegalMoves();
        var scoredMoves = new ScoredMoves();

        bool isBotWhite = board.IsWhiteToMove;

        // optionnal order
        var orderedMoves = OrderMoves(board, allMoves);

        foreach (Move move in orderedMoves)
        {
            var scoredMove = EvaluateMove(board, timer, move, isBotWhite, out var isInCheckmate);
            scoredMoves.Add(scoredMove);
            if (isInCheckmate && breakAtCheckMate)
                break;
        }

        m_TranspositionTable.Clear();

        return scoredMoves;
    }

    public ScoredMove EvaluateMove(Board board, Timer timer, Move move)
    {
        return EvaluateMove(board, timer, move, board.IsWhiteToMove, out _);
    }

    public ScoredMove EvaluateMove(Board board, Timer timer, Move move, bool isBotWhite, out bool isInCheckmate)
    {
        board.MakeMove(move);

        int score = int.MinValue;
        isInCheckmate = board.IsInCheckmate();

        // Always play checkmate in one
        if (isInCheckmate)
        {
            score = m_BoardEvaluator.Evaluate(board);
        }
        else
        {
            // else evaluate the board position
            var parameters = new EvaluationParameters() 
            {
                Move = move,
                IsWhite = isBotWhite,
            };

            score = Evaluate(board, timer, parameters);
        }

        board.UndoMove(move);
        return new(move, score);
    }

    public abstract int Evaluate(Board node, Timer timer, EvaluationParameters parameters);

    protected virtual Move[] OrderMoves(Board board, Move[] allMoves)
    {
        return allMoves;
    }
}
