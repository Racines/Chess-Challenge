using ChessChallenge.API;

internal static class BotHelpers
{
    // Piece values: null, pawn, knight, bishop, rook, queen, king
    static int[] s_PieceValues = { 0, 100, 300, 300, 500, 900, 10000 };

    /// <summary>
    /// Node is terminal if game is in checkmate state or is a draw
    /// </summary>
    /// <param name="node"></param>
    /// <returns></returns>
    public static bool IsTerminal(this Board node)
    {
        return node.IsInCheckmate() || node.IsDraw();
    }

    // Test if this move gives checkmate
    public static bool MoveIsCheckmate(this Board node, Move move)
    {
        node.MakeMove(move);
        bool isMate = node.IsInCheckmate();
        node.UndoMove(move);
        return isMate;
    }

    public static int HeuristicValue(this Board node)
    {
        // Heuristic value for checkmate 
        if (node.IsInCheckmate())
            return node.IsWhiteToMove ? int.MinValue : int.MaxValue;

        // Heuristic value for draw 
        if (node.IsDraw())
            return 0;

        // Compute heuristic value based on the score of each team
        int heuristicValue = 0;
        var teamsPieces = node.GetAllPieceLists();
        foreach (var teamPieces in teamsPieces)
        {
            // Compute score for the team
            int teamScore = teamPieces.TypeOfPieceInList.Value() * teamPieces.Count;

            // add or substract team score depending of isWhite value
            if (teamPieces.IsWhitePieceList)
                heuristicValue += teamScore;
            else
                heuristicValue -= teamScore;
        }

        return heuristicValue;
    }

    public static int Value(this Piece piece)
    {
        return piece.PieceType.Value();
    }

    public static int Value(this PieceType pieceType)
    {
        return s_PieceValues[(int)pieceType];
    }
}