using ChessChallenge.API;

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
            board.MakeMove(move);

            // Always play checkmate in one
            if (board.IsInCheckmate())
            {
                return move;
            }

            // else evaluate the board position
            var score = Evaluate(board, timer, move, isBotWhite);
            scoredMoves.Add(new (move, score));

            board.UndoMove(move);
        }

        // return best move
        return scoredMoves.GetBestMove();
    }

    public abstract int Evaluate(Board node, Timer timer, Move move, bool isWhite);

    protected virtual int HeuristicValue(Board node, bool isWhite)
    {
        return node.HeuristicValue(isWhite);
    }

    protected virtual Move[] OrderMoves(Board board, Move[] allMoves)
    {
        return allMoves;
    }
}
