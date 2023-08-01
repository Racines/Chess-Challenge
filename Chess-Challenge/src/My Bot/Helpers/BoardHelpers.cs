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

    public static ulong TeamAttackedBitboard(this Board board, bool isWhite)
    {
        ulong opponentAttackedBitboard = 0;
        foreach (var piecesList in board.GetAllPieceLists())
        {
            // ignore opponent pieces
            if (piecesList.IsWhitePieceList != isWhite)
                continue;

            opponentAttackedBitboard |= TeamAttackedBitboard(board, piecesList);
        }

        return opponentAttackedBitboard;
    }

    private static ulong TeamAttackedBitboard(this Board board, PieceList piecesList)
    {
        ulong opponentAttackedBitboard = 0;

        foreach (var piece in piecesList)
        {
            var pieceAttackBitboard = BitboardHelper.GetPieceAttacks(piece.PieceType, piece.Square, board, piece.IsWhite);
            opponentAttackedBitboard |= pieceAttackBitboard;
        }

        return opponentAttackedBitboard;
    }

    static BoardHelpers()
    {
        InitCenterpawnBitboard();
        InitBishoponlargeBitboard();
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


    #region Kingpawnshield

    /// <summary>
    /// Kingpawnshield is the number of pawns of Player A adjacent to Player A’s king. Pawn shield is
    /// an important parameter for evaluating the king safety
    /// </summary>
    /// <param name="board"></param>
    /// <returns></returns>
    public static int Kingpawnshield(this Board board)
    {
        return Kingpawnshield(board, true) - Kingpawnshield(board, false);
    }

    public static int Kingpawnshield(this Board board, bool isWhite)
    {
        var kingAttacksBitboard = BitboardHelper.GetKingAttacks(board.GetKingSquare(isWhite));
        var pawnBitboard = board.GetPieceBitboard(PieceType.Pawn, isWhite);
        var pawnShieldBitboard = kingAttacksBitboard & pawnBitboard;
        var pawnShieldCount = CustomBitboardHelper.CountBits(pawnShieldBitboard);

        //BitboardHelper.VisualizeBitboard(pawnShieldBitboard);

        return pawnShieldCount;
    }

    #endregion


    #region Kingattacked

    /// <summary>
    /// Kingdefended is the material value of the pieces of the Player A that are acting on Player A’s king’s
    /// adjacent squares.
    /// </summary>
    /// <param name="board"></param>
    /// <returns></returns>
    public static int Kingattacked(this Board board)
    {
        return Kingattacked(board, true) - Kingdefended(board, false);
    }

    public static int Kingattacked(this Board board, bool isWhite)
    {
        int defendedValue = 0;

        var attackedBitboard = 0ul;
        var kingAttacksBitboard = BitboardHelper.GetKingAttacks(board.GetKingSquare(isWhite));
        foreach (var pieceList in board.GetAllPieceLists())
        {
            // ignore friendly pieces
            if (pieceList.IsWhitePieceList == isWhite)
                continue;

            // sum pieces value that are attacking the king
            foreach (var piece in pieceList)
            {
                var pieceAttackBitboard = BitboardHelper.GetPieceAttacks(piece.PieceType, piece.Square, board, piece.IsWhite);
                ulong pieaceAttackKingBitboard = pieceAttackBitboard & kingAttacksBitboard;
                attackedBitboard |= pieaceAttackKingBitboard;

                var isDefended = pieaceAttackKingBitboard != 0;
                if (isDefended)
                    defendedValue += piece.Value();
            }
        }

        //BitboardHelper.VisualizeBitboard(attackedBitboard);

        return 0;
    }

    #endregion


    #region Kingdefended

    /// <summary>
    /// Kingdefended is the material value of the pieces of the Player A that are acting on Player A’s king’s
    /// adjacent squares.
    /// </summary>
    /// <param name="board"></param>
    /// <returns></returns>
    public static int Kingdefended(this Board board)
    {
        return Kingdefended(board, true) - Kingdefended(board, false);
    }

    public static int Kingdefended(this Board board, bool isWhite)
    {
        int defendedValue = 0;

        var defendedBitboard = 0ul;
        var kingAttacksBitboard = BitboardHelper.GetKingAttacks(board.GetKingSquare(isWhite));
        foreach (var pieceList in board.GetAllPieceLists())
        {
            // ignore enemy pieces
            if (pieceList.IsWhitePieceList != isWhite)
                continue;

            if (pieceList.TypeOfPieceInList == PieceType.King)
                continue;

            // sum pieces value that are defending the king
            foreach (var piece in pieceList)
            {
                var pieceAttackBitboard = BitboardHelper.GetPieceAttacks(piece.PieceType, piece.Square, board, piece.IsWhite);
                ulong pieceDefendKingBitboard = pieceAttackBitboard & kingAttacksBitboard;
                defendedBitboard |= pieceDefendKingBitboard;

                var isDefended = pieceDefendKingBitboard != 0;
                if (isDefended)
                    defendedValue += piece.Value();
            }
        }

        BitboardHelper.VisualizeBitboard(defendedBitboard);

        return 0;
    }

    #endregion


    #region Bishopmob

    /// <summary>
    /// Bishopmob is the number of squares that a bishop can go to. This type of parameters are calculated
    /// seperately for each bishop
    /// </summary>
    /// <param name="board"></param>
    /// <returns></returns>
    public static int Bishopmob(this Board board)
    {
        return Bishopmob(board, true) - Bishopmob(board, false);
    }

    public static int Bishopmob(this Board board, bool isWhite)
    {
        PieceList piecesList = board.GetPieceList(PieceType.Bishop, isWhite);
        var bishopAttackBitboard = TeamAttackedBitboard(board, piecesList);
        var opponentAttackedBitboard = TeamAttackedBitboard(board, !isWhite);
        var bishopAttacks = bishopAttackBitboard & ~opponentAttackedBitboard;

        //BitboardHelper.VisualizeBitboard(bishopAttacks);

        return CustomBitboardHelper.CountBits(bishopAttacks);
    }

    #endregion


    #region Bishoponlarge

    private static ulong s_BishoponlargeBitboard;

    /// <summary>
    /// Generate large diagonals bitboard
    /// </summary>
    private static void InitBishoponlargeBitboard()
    {
        for (int file = 0; file < 8; file++)
        {
            for (int rank = 0; rank < 8; rank++)
            {
                if (file == rank || file == 7 - rank)
                    BitboardHelper.SetSquare(ref s_BishoponlargeBitboard, new Square(file, rank));
            }
        }

        //BitboardHelper.VisualizeBitboard(s_BishoponlargeBitboard);
    }

    /// <summary>
    /// Bishoponlarge parameter returns 1 if the bishop on the given square is one of the two large diagonals
    /// of the board.Bishops are stronger on the large diagonals because they have higher mobility and
    /// they are reaching the two central squares simultaneously controlling the center
    /// </summary>
    /// <param name="board"></param>
    /// <param name="isWhite"></param>
    /// <returns></returns>
    public static int Bishoponlarge(this Board board)
    {
        return Bishoponlarge(board, true) - Bishoponlarge(board, false);
    }

    public static int Bishoponlarge(this Board board, bool isWhite)
    {
        var bishops = board.GetPieceList(PieceType.Bishop, isWhite);
        ulong bishopsSquareBitboard = 0;
        foreach (var bishop in bishops)
        {
            BitboardHelper.SetSquare(ref bishopsSquareBitboard, bishop.Square);
        }

        ulong bishopsOnLargeBitboard = bishopsSquareBitboard & s_BishoponlargeBitboard;
        //BitboardHelper.VisualizeBitboard(bishopsOnLargeBitboard);

        var isOnLarge = bishopsOnLargeBitboard != 0;
        return isOnLarge ? 1 : 0;
    }

    #endregion
}