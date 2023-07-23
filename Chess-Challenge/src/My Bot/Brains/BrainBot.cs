using ChessChallenge.API;
using Microsoft.CodeAnalysis.CSharp;
using System;

public abstract class BrainBot : IChessBot
{
    public Move Think(Board board, Timer timer)
    {
        var scoredMoves = EvaluateLegalMoves(board, timer);

        // return best move
        return scoredMoves.GetBestMove(board.IsWhiteToMove);
    }

    public ScoredMoves EvaluateLegalMoves(Board board, Timer timer)
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
            if (isInCheckmate)
                break;
        }

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
            score = board.HeuristicValue();
        }
        else
        {
            // else evaluate the board position
            score = Evaluate(board, timer, move, isBotWhite);
        }

        board.UndoMove(move);
        return new(move, score);
    }

    public abstract int Evaluate(Board node, Timer timer, Move move, bool isWhite);

    protected virtual int HeuristicValue(Board node)
    {
        return node.HeuristicValue();
    }

    protected virtual Move[] OrderMoves(Board board, Move[] allMoves)
    {
        return allMoves;
    }
}
