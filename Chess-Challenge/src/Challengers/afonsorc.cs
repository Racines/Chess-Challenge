using ChessChallenge.API;
using System;


public class afonsorc : IChessBot
{
    public struct Entry
    {
        public ulong key;
        public Move move;
        public int depth, evaluation, evaluationType;

        public Entry(ulong key, Move move, int depth, int evaluation, int evaluationType)
        {
            this.key = key;
            this.move = move;
            this.depth = depth;
            this.evaluation = evaluation;
            this.evaluationType = evaluationType;
        }
    }

    Move bestRootMove = Move.NullMove;
    const int ttSize = 8388608;
    public Entry[] transpositionTable = new Entry[ttSize];

    // PeSTO Piece Square Table
    readonly static ulong[] pestoPST = { 657614902731556116, 420894446315227099, 384592972471695068, 312245244820264086, 364876803783607569, 366006824779723922, 366006826859316500, 786039115310605588, 421220596516513823, 366011295806342421, 366006826859316436, 366006896669578452, 162218943720801556, 440575073001255824, 657087419459913430, 402634039558223453, 347425219986941203, 365698755348489557, 311382605788951956, 147850316371514514, 329107007234708689, 402598430990222677, 402611905376114006, 329415149680141460, 257053881053295759, 291134268204721362, 492947507967247313, 367159395376767958, 384021229732455700, 384307098409076181, 402035762391246293, 328847661003244824, 365712019230110867, 366002427738801364, 384307168185238804, 347996828560606484, 329692156834174227, 365439338182165780, 386018218798040211, 456959123538409047, 347157285952386452, 365711880701965780, 365997890021704981, 221896035722130452, 384289231362147538, 384307167128540502, 366006826859320596, 366006826876093716, 366002360093332756, 366006824694793492, 347992428333053139, 457508666683233428, 329723156783776785, 329401687190893908, 366002356855326100, 366288301819245844, 329978030930875600, 420621693221156179, 422042614449657239, 384602117564867863, 419505151144195476, 366274972473194070, 329406075454444949, 275354286769374224, 366855645423297932, 329991151972070674, 311105941360174354, 256772197720318995, 365993560693875923, 258219435335676691, 383730812414424149, 384601907111998612, 401758895947998613, 420612834953622999, 402607438610388375, 329978099633296596, 67159620133902 };
    readonly int[] pieceValue = { 0, 100, 310, 330, 500, 1000, 0 };
    readonly int[] piecePhase = { 0, 0, 1, 1, 2, 4, 0 };


    public Move Think(Board board, Timer timer)
    {

        for (int depth = 3; depth <= 3; depth++)
        {
            int evaluation = AlphaBetaSearch(board, depth, -20000, 20000, timer, 0);
            if (timer.MillisecondsElapsedThisTurn >= timer.MillisecondsRemaining / 30) break;
        }
        return bestRootMove == Move.NullMove ? board.GetLegalMoves()[0] : bestRootMove;
    }


    public int GetValuePesto(int index)
    {
        return (int)(((pestoPST[index / 10] >> (6 * (index % 10))) & 63) - 20) * 8;
    }


    public int Evaluate(Board board)
    {

        int phase = 0;
        int[] midgame = new int[2];
        int[] endgame = new int[2];
        bool isWhite = true;

        for (int side = 0; side < 2; side++)
        {
            for (var piece = PieceType.Pawn; piece <= PieceType.King; piece++)
            {
                ulong bitboard = board.GetPieceBitboard(piece, isWhite);
                int materialEvaluation = pieceValue[(int)piece] * board.GetPieceList(piece, isWhite).Count;
                midgame[side] += materialEvaluation;
                endgame[side] += materialEvaluation;
                phase += board.GetPieceList(piece, isWhite).Count * piecePhase[(int)piece];
                while (bitboard != 0)
                {
                    int index = 128 * ((int)piece - 1) + BitboardHelper.ClearAndGetIndexOfLSB(ref bitboard) ^ (isWhite ? 56 : 0);
                    midgame[side] += GetValuePesto(index);
                    endgame[side] += GetValuePesto(index + 64);
                }
            }
            isWhite = false;
        }

        phase = Math.Min(phase, 24);
        int evaluation = ((midgame[0] - midgame[1]) * phase + (endgame[0] - endgame[1]) * (24 - phase)) / 24;
        return board.IsWhiteToMove ? evaluation + 10 : -10 - evaluation;
    }


    public void OrderMoves(Move[] moves, Entry entry)
    {

        int moveCount = moves.Length;
        int[] score = new int[moveCount];

        for (int i = 0; i < moveCount; i++)
        {
            if (moves[i] == entry.move)
                score[i] = 10000;
            else if (moves[i].IsCapture)
            {
                int capture = (int)moves[i].CapturePieceType;
                int move = (int)moves[i].MovePieceType;
                score[i] = pieceValue[capture] * capture - pieceValue[move] * move;
            }
            else score[i] = -10000;
        }

        for (int i = 0; i < moveCount - 1; i++)
        {
            int biggestValue = i;
            for (int j = i + 1; j < moveCount; j++)
                if (score[j] > score[biggestValue]) biggestValue = j;
            if (biggestValue != i)
            {
                (moves[i], moves[biggestValue]) = (moves[biggestValue], moves[i]);
                (score[i], score[biggestValue]) = (score[biggestValue], score[i]);
            }
        }
    }


    public int AlphaBetaSearch(Board board, int depth, int alpha, int beta, Timer timer, int ply)
    {

        bool isRoot = ply <= 0;

        // repetition occurs more often so we check it first
        // might implement penalty for draw or contempt factor
        if (!isRoot && board.IsRepeatedPosition()) return 0;
        if (!isRoot && (board.FiftyMoveCounter >= 100 || board.IsInsufficientMaterial())) return 0;

        ulong zobristHash = board.ZobristKey;
        Entry entry = transpositionTable[zobristHash % ttSize];

        // check if current board position is on transposition table and return it if valid
        if (!isRoot && entry.key == zobristHash && entry.depth >= depth)
            if (entry.evaluationType == 0 || (entry.evaluationType == 1 && entry.evaluation <= alpha) || (entry.evaluationType == 2 && entry.evaluation >= beta))
                return entry.evaluation;


        bool isQueiescenceSearch = depth <= 0;
        int max = -30000;
        int alphaStart = alpha;

        if (isQueiescenceSearch)
        {
            max = Evaluate(board);
            if (max >= beta) return beta;
            if (max > alpha) alpha = max;
        }


        Move[] moves = board.GetLegalMoves(isQueiescenceSearch);
        OrderMoves(moves, entry);

        Move bestMove = Move.NullMove;
        foreach (Move move in moves)
        {

            if (timer.MillisecondsElapsedThisTurn >= timer.MillisecondsRemaining / 30) return 10000;

            board.MakeMove(move);
            int evaluation = -AlphaBetaSearch(board, depth - 1, -beta, -alpha, timer, ply + 1);
            board.UndoMove(move);

            if (evaluation > max)
            {
                max = evaluation;
                bestMove = move;
                if (isRoot) bestRootMove = move;
                alpha = Math.Max(alpha, evaluation);
                if (alpha >= beta) break;
            }
        }

        //Check for checkmate or stalemate
        if (board.GetLegalMoves().Length == 0) return board.IsInCheck() ? -10000 : 0;

        int evaluationType = 0;
        if (max <= alphaStart) evaluationType = 1;
        else if (max >= beta) evaluationType = 2;

        transpositionTable[zobristHash % ttSize] = new Entry(zobristHash, bestMove, depth, alpha, evaluationType);
        return max;
    }
}