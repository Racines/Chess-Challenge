﻿using ChessChallenge.API;

public abstract class BrainBot : IChessBot
{
    public Move Think(Board board, Timer timer)
    {
        Move[] allMoves = board.GetLegalMoves();
        var scoredMoves = new ScoredMoves();

        bool isBotWhite = board.IsWhiteToMove;

        // optionnal order
        var orderedMoves = OrderMoves(board, allMoves);

        foreach (Move move in orderedMoves)
        {
            // get the captured piece
            Piece capturedPiece = board.GetPiece(move.TargetSquare);

            board.MakeMove(move);

            // Always play checkmate in one
            if (board.IsInCheckmate())
            {
                return move;
            }

            // else evaluate the board position
            var score = Evaluate(board, capturedPiece, isBotWhite);
            scoredMoves.Add(new (move, score));

            board.UndoMove(move);
        }

        // return best move
        return scoredMoves.GetBestMove();
    }

    public abstract int Evaluate(Board node, Piece capturedPiece, bool isWhite);

    protected virtual Move[] OrderMoves(Board board, Move[] allMoves)
    {
        return allMoves;
    }
}
