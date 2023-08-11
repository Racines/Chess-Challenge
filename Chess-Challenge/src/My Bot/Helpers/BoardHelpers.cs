using ChessChallenge.API;
using System;
using System.Collections.Generic;
using System.Linq;

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
        var pawnAtCenterCount = BitboardHelper.GetNumberOfSetBits(pawnAtCenterBitboard);

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
        var pawnShieldCount = BitboardHelper.GetNumberOfSetBits(pawnShieldBitboard);

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
    /// Bishoponlarge parameter returns the number of bishop placed on one of the two large diagonals
    /// of the board. Bishops are stronger on the large diagonals because they have higher mobility and
    /// they are reaching the two central squares simultaneously controlling the center.
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

        return BitboardHelper.GetNumberOfSetBits(bishopsOnLargeBitboard);
    }

    #endregion


    #region Bishoppair

    /// <summary>
    /// Bishoppair returns 1 if Player A has two or more bishops. Bishop pairs are generally considered
    /// an advantage as to bishops can together cover all possible squares regardless of the color of the
    /// square.Bishop pairs are especially strong in open positions where there are no central pawns and
    /// the bishops can move freely to create threats.
    /// </summary>
    /// <param name="board"></param>
    /// <returns></returns>
    public static int Bishoppair(this Board board)
    {
        return Bishoppair(board, true) - Bishoppair(board, false);
    }

    public static int Bishoppair(this Board board, bool isWhite)
    {
        var bishops = board.GetPieceList(PieceType.Bishop, isWhite);
        var hasBishopPair = bishops.Count >= 2;

        return hasBishopPair ? 1 : 0;
    }

    #endregion


    #region Knightsupport

    /// <summary>
    /// Knightsupport returns 1 per knight on a given square that is supported by ones own pawn.
    /// Supported knights are strong because they can only be backfired by pawns and since they are
    /// supported they can stay in an important position for a long number of moves making it harder for
    /// the opponent to play around it.
    /// </summary>
    /// <param name="board"></param>
    /// <returns></returns>
    public static int Knightsupport(this Board board)
    {
        return Knightsupport(board, true) - Knightsupport(board, false);
    }

    public static int Knightsupport(this Board board, bool isWhite)
    {
        ulong knightsSquareBitboard = 0;
        ulong pawnsAttackBitboard = 0;

        var knights = board.GetPieceList(PieceType.Knight, isWhite);
        var pawns = board.GetPieceList(PieceType.Pawn, isWhite);

        foreach (var pawn in pawns)
        {
            pawnsAttackBitboard |= BitboardHelper.GetPawnAttacks(pawn.Square, isWhite);
        }

        //BitboardHelper.VisualizeBitboard(pawnsAttackBitboard);

        foreach (var knight in knights) 
        {
            BitboardHelper.SetSquare(ref knightsSquareBitboard, knight.Square);
        }
        //BitboardHelper.VisualizeBitboard(knightsSquareBitboard);

        var knightSupportedBitboard = knightsSquareBitboard & pawnsAttackBitboard;
        //BitboardHelper.VisualizeBitboard(knightSupportedBitboard);

        return BitboardHelper.GetNumberOfSetBits(knightSupportedBitboard);
    }

    #endregion


    #region Knightperipheries

    /// <summary>
    /// Knightperiphery0 returns 1 if a given knight is on the squares a1 to a8,a8 to h8,a1 to h1 or h1 to h8.
    /// This is the outest periphery and most of the times knights on these squares are weaker.
    /// Knightperiphery1 returns 1 if a given knight is on the squares b2 to b7,b7 to g7,b2 to g2 or g2 to g7.
    /// Knightperiphery2 returns 1 if a given knight is on the squares c3 to c6,c6 to f6,c3 to f3 or f3 to f6.
    /// Knightperiphery3 returns 1 if a given knight is on the squares e4, e5,d4 or d5.
    /// </summary>
    /// <param name="board"></param>
    /// <param name="peripheryIndex"></param>
    /// <returns></returns>
    public static int Knightperiphery(this Board board, int peripheryIndex)
    {
        return Knightperiphery(board, true, peripheryIndex) - Knightperiphery(board, false, peripheryIndex);
    }

    public static int Knightperiphery(this Board board, bool isWhite, int peripheryIndex)
    {
        int knightPeripheryCount = 0;

        var knights = board.GetPieceList(PieceType.Knight, isWhite);
        foreach (var knight in knights)
        {
            Square sq = knight.Square;

            if (sq.Rank == peripheryIndex &&
                sq.File >= peripheryIndex &&
                sq.File < 7 - peripheryIndex 
                ||
                sq.File == peripheryIndex &&
                sq.Rank >= peripheryIndex &&
                sq.Rank < 7 - peripheryIndex
                ||
                sq.File == 7 - peripheryIndex &&
                sq.Rank >= peripheryIndex &&
                sq.Rank < 7 - peripheryIndex
                ||
                sq.File == 7 - peripheryIndex &&
                sq.Rank >= peripheryIndex &&
                sq.Rank < 7 - peripheryIndex)
            {
                ++knightPeripheryCount;
            }
        }

        //Console.WriteLine($"knightPeripheryCount: {knightPeripheryCount}");

        return knightPeripheryCount;
    }

    #endregion


    #region Isopawn

    /// <summary>
    /// Isopawn count the number of pawn that has no neighboring pawns of the same color. Isolated pawns
    /// are generally considered as a weakness since they cannot be protected by pawns so they should be
    /// protected by other more valuable pieces.
    /// </summary>
    /// <param name="board"></param>
    /// <returns></returns>
    public static int Isopawn(this Board board)
    {
        return Isopawn(board, true) - Isopawn(board, false);
    }

    public static int Isopawn(this Board board, bool isWhite) 
    {
        int isoPawnCount = 0;

        uint flattenPawnSquareBitboard = 0;

        var pawns = board.GetPieceList(PieceType.Pawn, isWhite);
        foreach (var pawn in pawns)
        {
            flattenPawnSquareBitboard |= 1u << pawn.Square.File;
        }
        //Console.WriteLine($"flattenPawnSquareBitboard: {Convert.ToString(flattenPawnSquareBitboard, 2)}");

        uint isoPattern = 0b101;
        for (int i = 0; i <= 7; i++)
        {
            // ignore if not pawn
            if ((flattenPawnSquareBitboard & (1 << i)) == 0)
                continue;

            uint shiftedIsoPattern = isoPattern << (i - 1);
            if (i == 0)
                shiftedIsoPattern = isoPattern >> 1;

            var isIso = (shiftedIsoPattern & flattenPawnSquareBitboard) == 0;
            if (isIso)
                ++isoPawnCount;
        }

        //Console.WriteLine($"isoPawnCount: {isoPawnCount}");

        return 0;
    }

    #endregion


    #region Doublepawn

    /// <summary>
    /// Doublepawn returns 1 per pawn that is doubled pawn. Doubled pawns are considered a disadvantage as
    /// they blocked each other and they are vulnerable to attacks
    /// </summary>
    /// <param name="board"></param>
    /// <returns></returns>
    public static int Doublepawn(this Board board)
    {
        return Doublepawn(board, true) - Doublepawn(board, false);
    }

    public static int Doublepawn(this Board board, bool isWhite) 
    {
        var pawnCounterPerFile = new int[8];

        var pawns = board.GetPieceList(PieceType.Pawn, isWhite);
        foreach (var pawn in pawns)
        {
            ++pawnCounterPerFile[pawn.Square.File];
        }

        var doublePawn = pawnCounterPerFile.Count(x => x > 1);
        //Console.WriteLine($"doublePawn: {doublePawn}");

        return doublePawn;
    }

    #endregion


    #region Passpawn

    /// <summary>
    /// Passpawn returns 1 per pawn there are no opposing pawns of the enemy on the neighboring
    /// columns and on the given pawn’s column ahead of the pawn.If a pawn is passed it is big threat for
    /// the opponent because the are no pawns on the way to prevent it from promoting.
    /// </summary>
    /// <param name="board"></param>
    /// <returns></returns>
    public static int Passpawn(this Board board)
    {
        return Passpawn(board, true) - Passpawn(board, false);
    }

    public static int Passpawn(this Board board, bool isWhite)
    {
        List<Piece> passPawns = GetPassPawns(board, isWhite);

        int passPawnCount = passPawns.Count;
        //Console.WriteLine($"passPawnCount: {passPawnCount}");

        return passPawnCount;
    }

    private static List<Piece> GetPassPawns(Board board, bool isWhite)
    {
        var passPawns = new List<Piece>();

        var opponentPawnsFile = new List<int>[8];
        for (int i = 0; i < opponentPawnsFile.Length; i++)
        {
            opponentPawnsFile[i] = new List<int>();
        }

        var opponentPawns = board.GetPieceList(PieceType.Pawn, !isWhite);
        foreach (var pawn in opponentPawns)
        {
            opponentPawnsFile[pawn.Square.File].Add(pawn.Square.Rank);
        }

        Func<int, int, bool> compare = (x, y) => x < y;
        if (isWhite)
            compare = (x, y) => x > y;

        var pawns = board.GetPieceList(PieceType.Pawn, isWhite);
        foreach (var pawn in pawns)
        {
            // opponent pawns in neighbour files
            IEnumerable<int> pawnsFileToCheck = opponentPawnsFile[pawn.Square.File];
            if (pawn.Square.File > 0)
                pawnsFileToCheck = pawnsFileToCheck.Union(opponentPawnsFile[pawn.Square.File - 1]);
            if (pawn.Square.File < 7)
                pawnsFileToCheck = pawnsFileToCheck.Union(opponentPawnsFile[pawn.Square.File + 1]);

            var isPass = !pawnsFileToCheck.Any(x => compare(x, pawn.Square.Rank));
            if (isPass)
                passPawns.Add(pawn);
        }

        return passPawns;
    }

    #endregion


    #region Rookbhdpasspawn

    /// <summary>
    /// Rookbhdpasspawn returns 1 per rook of the same color that is behind the passed pawn. If there is a rook
    /// behind a passed pawn it is easier to push to pawn forward as it is always protected by the rook and
    /// rook never gets in the way.
    /// </summary>
    /// <param name="board"></param>
    /// <returns></returns>
    public static int Rookbhdpasspawn(this Board board)
    {
        return Rookbhdpasspawn(board, true) - Rookbhdpasspawn(board, false);
    }

    public static int Rookbhdpasspawn(this Board board, bool isWhite)
    {
        int rookbhdpasspawn = 0;

        var rooks = board.GetPieceList(PieceType.Rook, isWhite);
        if (rooks.Count > 0)
        {
            var passPawns = GetPassPawns(board, isWhite);
            if (passPawns.Count > 0)
            {
                foreach (var pawn in passPawns)
                {
                    var isRookbhdpasspawn = rooks.Any(x => x.Square.File == pawn.Square.File);
                    if (isRookbhdpasspawn)
                        ++rookbhdpasspawn;
                }
            }
        }

        //Console.WriteLine($"rookbhdpasspawn: {rookbhdpasspawn}");

        return rookbhdpasspawn;
    }

    #endregion


    #region Rankpassedpawn

    /// <summary>
    /// Rankpassedpawn is the sum of ranks of the passed pawns. A passed pawn on rank 7 which means the pawn
    /// is one move away from promoting is a lot more dangerous compared to a passed pawn on its
    /// initial square. Passed pawns with higher ranks have higher priority thus they are an advantage.
    /// </summary>
    /// <param name="board"></param>
    /// <returns></returns>
    public static int Rankpassedpawn(this Board board)
    {
        return Rankpassedpawn(board, true) - Rankpassedpawn(board, false);
    }

    public static int Rankpassedpawn(this Board board, bool isWhite) 
    {
        int rankPassedpawns = 0;

        var passPawns = GetPassPawns(board, isWhite);
        foreach (var pawn in passPawns)
        {
            if (isWhite)
                rankPassedpawns += pawn.Square.Rank;
            else
                rankPassedpawns += 8 - pawn.Square.Rank;
        }
        //Console.WriteLine($"rankPassedpawns: {rankPassedpawns}");

        return rankPassedpawns;
    }

    #endregion


    #region Blockedpawn

    /// <summary>
    /// Blockedpawn returns 1 per central pawn on column e or d on its initial square that is blocked by its own
    /// piece which severely decreases the mobility of the pieces.
    /// </summary>
    /// <param name="board"></param>
    /// <returns></returns>
    public static int Blockedpawn(this Board board)
    {
        return Blockedpawn(board, true) - Blockedpawn(board, false);
    }

    public static int Blockedpawn(this Board board, bool isWhite) 
    {
        int blockedPawn = 0;

        var initialRank = isWhite ? 1 : 6;
        int pawnMoveOffset = isWhite ? 1 : -1;

        var pawns = board.GetPieceList(PieceType.Pawn, isWhite);
        var centralPawnsAtInitialPos = pawns
            .Where(x => x.Square.File is 3 or 4)
            .Where(x => x.Square.Rank == initialRank);

        var pawnMoveBitboard = 0ul;

        foreach (var pawn in centralPawnsAtInitialPos)
        {
            Square square = new Square(pawn.Square.File, pawn.Square.Rank + pawnMoveOffset);
            BitboardHelper.SetSquare(ref pawnMoveBitboard, square);
        }

        var playerPieceBitboard = isWhite ? board.WhitePiecesBitboard : board.BlackPiecesBitboard;
        var blockedPawnBitboard = playerPieceBitboard & pawnMoveBitboard;
        //BitboardHelper.VisualizeBitboard(blockedPawnBitboard);

        blockedPawn = BitboardHelper.GetNumberOfSetBits(blockedPawnBitboard);
        //Console.WriteLine($"blockedPawn: {blockedPawn}");

        return blockedPawn;
    }

    #endregion


    #region Blockedpassedpawn

    /// <summary>
    /// Blockedpassedpawn returns 1 per passed pawn of Player A that is blocked by a piece of Player B which
    /// prevents it from moving closer to promotion.This is an advantage for the blocking side.
    /// </summary>
    /// <param name="board"></param>
    /// <returns></returns>
    public static int Blockedpassedpawn(this Board board)
    {
        return Blockedpassedpawn(board, true) - Blockedpassedpawn(board, false);
    }

    public static int Blockedpassedpawn(this Board board, bool isWhite)
    {
        int blockedPassedPawns = 0;

        int pawnMoveOffset = isWhite ? 1 : -1;

        var pawns = GetPassPawns(board, isWhite);

        var pawnMoveBitboard = 0ul;

        foreach (var pawn in pawns)
        {
            Square square = new Square(pawn.Square.File, pawn.Square.Rank + pawnMoveOffset);
            BitboardHelper.SetSquare(ref pawnMoveBitboard, square);
        }

        var opponentPieceBitboard = !isWhite ? board.WhitePiecesBitboard : board.BlackPiecesBitboard;
        var blockedPawnBitboard = opponentPieceBitboard & pawnMoveBitboard;
        //BitboardHelper.VisualizeBitboard(blockedPawnBitboard);

        blockedPassedPawns = BitboardHelper.GetNumberOfSetBits(blockedPawnBitboard);
        //Console.WriteLine($"blockedPassedPawns: {blockedPassedPawns}");

        return blockedPassedPawns;
    }

    #endregion


    #region Rookopenfile

    public enum EFileType
    {
        Open,
        SemiOpen,
        Closed,
    }

    /// <summary>
    /// Rookopenfile returns 1 per given rook that is on a file with no pawns from either side. Rooks are stronger
    /// on open columns because they can move freely.
    /// </summary>
    /// <param name="board"></param>
    /// <returns></returns>
    public static int Rookopenfile(this Board board)
    {
        return Rookfile(board, true, EFileType.Open) - Rookfile(board, false, EFileType.Open);
    }

    /// <summary>
    /// Rooksemiopenfile returns 1 per given rook that is on a file with no pawns from its own side. Rooks are
    /// strong on semi-open files as well.
    /// </summary>
    /// <param name="board"></param>
    /// <returns></returns>
    public static int Rooksemiopenfile(this Board board)
    {
        return Rookfile(board, true, EFileType.SemiOpen) - Rookfile(board, false, EFileType.SemiOpen);
    }

    /// <summary>
    /// Rookclosedfile returns 1 per rook that is on a file with pawns from both sides. Rooks on closed
    /// files are considered a disadvantage as they have lower file mobility and no access to the important
    /// squares of the game especially in the middlegame and endgame.
    /// </summary>
    /// <param name="board"></param>
    /// <returns></returns>
    public static int Rookclosedfile(this Board board)
    {
        return Rookfile(board, true, EFileType.Closed) - Rookfile(board, false, EFileType.Closed);
    }

    public static int Rookfile(this Board board, bool isWhite, EFileType fileType)
    {
        int rookFileType = 0;

        var rooks = board.GetPieceList(PieceType.Rook, isWhite);
        if (rooks.Count > 0)
        {
            var playerPawns = board.GetPieceList(PieceType.Pawn, isWhite);
            var opponentPawns = board.GetPieceList(PieceType.Pawn, !isWhite);
            IEnumerable<Piece> pawns = playerPawns;
            if (fileType != EFileType.SemiOpen)
                pawns = playerPawns.Union(opponentPawns);

            var hasPlayerPawnOnFile = new bool[8];
            var hasOpponentPawnOnFile = new bool[8];
            foreach (var pawn in pawns)
            {
                if (isWhite == pawn.IsWhite)
                    hasPlayerPawnOnFile[pawn.Square.File] = true;
                else
                    hasOpponentPawnOnFile[pawn.Square.File] = true;
            }

            foreach (var rook in rooks)
            {
                bool isFileType = !hasPlayerPawnOnFile[rook.Square.File];
                switch (fileType)
                {
                    case EFileType.Open:
                    default:
                        isFileType = !hasPlayerPawnOnFile[rook.Square.File] &&
                                     !hasOpponentPawnOnFile[rook.Square.File];
                        break;
                    case EFileType.SemiOpen:
                        isFileType = !hasPlayerPawnOnFile[rook.Square.File];
                        break;
                    case EFileType.Closed:
                        isFileType = hasPlayerPawnOnFile[rook.Square.File] &&
                                     hasOpponentPawnOnFile[rook.Square.File];
                        break;
                }

                if (isFileType)
                    ++rookFileType;
            }
        }

        //Console.WriteLine($"rookFileType: {rookFileType}");

        return rookFileType;
    }

    #endregion


    #region Rookonseventh

    /// <summary>
    /// Rookonseventh returns 1 per rook that is on seventh rank from the Players perspective. For white
    /// that would be the rank 7 for black rank 2. Rooks on seventh rank are dangerous and a classical
    /// theme in chess for creating major threats at once.
    /// </summary>
    /// <param name="board"></param>
    /// <returns></returns>
    public static int Rookonseventh(this Board board)
    {
        return Rookonseventh(board, true) - Rookonseventh(board, false);
    }

    public static int Rookonseventh(this Board board, bool isWhite)
    {
        var rooks = board.GetPieceList(PieceType.Rook, isWhite);
        var relativeSeventh = isWhite ? 6 : 1;

        int rookOnSeventh = rooks.Count(x => x.Square.Rank == relativeSeventh);
        //Console.WriteLine($"rookOnSeventh: {rookOnSeventh}");

        return rookOnSeventh;
    }

    #endregion


    #region PieceMob

    /// <summary>
    /// Bishopmob is the number of squares that bishops can go to.
    /// </summary>
    /// <param name="board"></param>
    /// <returns></returns>
    public static int Bishopmob(this Board board)
    {
        return PieceMob(board, true, PieceType.Bishop) - PieceMob(board, false, PieceType.Bishop);
    }

    /// <summary>
    /// Knightmob is the number of squares that knights can go to.
    /// </summary>
    /// <param name="board"></param>
    /// <returns></returns>
    public static int Knightmob(this Board board)
    {
        return PieceMob(board, true, PieceType.Knight) - PieceMob(board, false, PieceType.Knight);
    }

    /// <summary>
    /// Rookmob returns the number of squares rooks can move to.
    /// </summary>
    /// <param name="board"></param>
    /// <returns></returns>
    public static int Rookmob(this Board board)
    {
        return PieceMob(board, true, PieceType.Rook) - PieceMob(board, false, PieceType.Rook);
    }

    /// <summary>
    /// Queenmob returns the number of squares queens can move to.
    /// </summary>
    /// <param name="board"></param>
    /// <returns></returns>
    public static int Queenmob(this Board board)
    {
        return PieceMob(board, true, PieceType.Queen) - PieceMob(board, false, PieceType.Queen);
    }

    /// <summary>
    /// Kingmob returns the number of squares the king can move to.
    /// </summary>
    /// <param name="board"></param>
    /// <returns></returns>
    public static int Kingmob(this Board board)
    {
        return PieceMob(board, true, PieceType.King) - PieceMob(board, false, PieceType.King);
    }

    public static int PieceMob(this Board board, bool isWhite, PieceType pieceType)
    {
        var pieces = board.GetPieceList(pieceType, isWhite);
        var piecesMovesBitboard = 0ul;

        foreach (var piece in pieces)
        {
            piecesMovesBitboard |= BitboardHelper.GetPieceAttacks(pieceType, piece.Square, board, isWhite);
        }

        // remove defenders moves
        var playerPiecesBitboard = isWhite ? board.WhitePiecesBitboard : board.BlackPiecesBitboard;
        piecesMovesBitboard &= ~playerPiecesBitboard;

        //BitboardHelper.VisualizeBitboard(piecesMovesBitboard);

        int pieceMob = BitboardHelper.GetNumberOfSetBits(piecesMovesBitboard);

        //Console.WriteLine($"pieceMob({pieceType}): {pieceMob}");

        return pieceMob;
    }

    #endregion


    #region Rookcon

    /// <summary>
    /// Rookcon returns 1 if there are no pieces between to rooks of the same color and they are on the
    /// same file or on the same rank.Connected rooks defend each other to create threats in the opposition
    /// area because they cannot be captured by queen or king.
    /// </summary>
    /// <param name="board"></param>
    /// <returns></returns>
    public static int Rookcon(this Board board)
    {
        return Rookcon(board, true) - Rookcon(board, false);
    }

    public static int Rookcon(this Board board, bool isWhite)
    {
        var rooks = board.GetPieceList(PieceType.Rook, isWhite);
        var rooksMovesBitboard = 0ul;

        foreach (var rook in rooks)
        {
            rooksMovesBitboard |= BitboardHelper.GetPieceAttacks(PieceType.Rook, rook.Square, board, isWhite);
        }

        ulong rooksSquareBitboard = board.GetPieceBitboard(PieceType.Rook, isWhite);
        ulong rooksConBitboard = rooksMovesBitboard & rooksSquareBitboard;
        var isRookCon = rooksConBitboard != 0;

        BitboardHelper.VisualizeBitboard(rooksConBitboard);

        int rookCon = isRookCon ? 1 : 0;
        Console.WriteLine($"rookCon: {rookCon}");

        return rookCon;
    }

    #endregion
}