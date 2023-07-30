using ChessChallenge.API;

public static class BoardHelpers
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
    static BoardHelpers()
    {
        InitCenterpawnBitboard();
    }

    #region Centerpawncount

    private static ulong s_CenterpawnBitboard;

    private static void InitCenterpawnBitboard()
    {
        BitboardHelper.SetSquare(ref s_CenterpawnBitboard, new Square("d4"));
        BitboardHelper.SetSquare(ref s_CenterpawnBitboard, new Square("d5"));
        BitboardHelper.SetSquare(ref s_CenterpawnBitboard, new Square("e4"));
        BitboardHelper.SetSquare(ref s_CenterpawnBitboard, new Square("e5"));

        //BitboardHelper.VisualizeBitboard(s_CenterpawnBitboard);
    }

    /// <summary>
    /// Centerpawncount is the number of pawns of Player A that are in squares e4,e5,d4 or d5. Center
    /// pawns are important for controlling the center and decreasing enemy mobility.
    /// </summary>
    /// <param name="board"></param>
    /// <returns></returns>
    public static int Centerpawncount(this Board board)
    {
        return Centerpawncount(board, true) - Centerpawncount(board, false);
    }

    public static int Centerpawncount(this Board board, bool isWhite)
    {
        var pawnBitboard = board.GetPieceBitboard(PieceType.Pawn, true);
        var pawnAtCenterBitboard = pawnBitboard & s_CenterpawnBitboard;
        var pawnAtCenterCount = CustomBitboardHelper.CountBits(pawnAtCenterBitboard);

        //BitboardHelper.VisualizeBitboard(whitePawnAtCenterBitboard);

        return pawnAtCenterCount;
    }

    #endregion
}