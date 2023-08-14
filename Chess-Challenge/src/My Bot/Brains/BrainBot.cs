//#define debuglog

using Evaluator;
using ChessChallenge.API;
using System;
using System.Diagnostics;


public abstract class BrainBot : IChessBot
{
    public struct EvaluationParameters
    {
        public Move Move;
        public bool IsWhite;
        public int Depth;
    }

    protected DepthTranspositionTable m_TranspositionTable = new ();
    protected bool m_UseTranspositionTable = true;
    protected BoardEvaluator m_BoardEvaluator = new BasicBoardEvaluator();
    protected BoardEvaluator m_DebugBoardEvaluator = new DebugBoardEvaluator();
    protected int m_MinDepth = 1;
    protected int m_MaxDepth = 100;

    protected MoveOrderer m_MoveOrderer = new BasicMoveOrderer();


    public bool UseTranspositionTable { get => m_UseTranspositionTable; set => m_UseTranspositionTable = value; }

    public Move Think(Board board, Timer timer)
    {
        var evalParams = new BoardEvaluator.EvalParameters();
        m_DebugBoardEvaluator?.Evaluate(board, evalParams);

        var scoredMoves = EvaluateLegalMoves(board, timer);

        // return best move
        return scoredMoves.GetBestMove(board.IsWhiteToMove);
    }

    public ScoredMoves EvaluateLegalMoves(Board board, Timer timer, bool breakAtCheckMate = true)
    {
        Move[] allMoves = board.GetLegalMoves();

        // compute time allowed for this turn
        const int averageTurnInAGame = 80;
        const int minTurnLeft = 20;
        var moveUntilEnd = Math.Max(minTurnLeft, averageTurnInAGame - board.PlyCount);
        var turnTimeAllowed = timer.MillisecondsRemaining / moveUntilEnd;

        // Compute default min depth that should be computable in given time
        const int timeToComputeDepth3 = 100;
        const int baseDepthComputation = 3;
        const int averageLegalMove = 30;
        const int depthComplexity = 5;
        var baseTime = (timeToComputeDepth3 * allMoves.Length) / (double)averageLegalMove;
        var minDepth = (int)(Math.Log(turnTimeAllowed / baseTime, depthComplexity) + baseDepthComputation);

        minDepth = Math.Clamp(minDepth, m_MinDepth, m_MaxDepth);

        var p = new EvaluationParameters()
        {
            IsWhite = board.IsWhiteToMove,
            Depth = minDepth,
        };

        MyBotLogLine($"====== Move {board.PlyCount} - allowed time: {turnTimeAllowed}");
        MyBotLogLine($"legal move: {allMoves.Length}");

        var orderedMoves = m_MoveOrderer.OrderMoves(board, allMoves);
        var scoredMoves = new ScoredMoves();

        do
        {
            foreach (Move move in orderedMoves)
            {
                p.Move = move;
                var scoredMove = EvaluateMove(board, timer, p, out var isInCheckmate);
                scoredMoves.Add(scoredMove);
                if (isInCheckmate && breakAtCheckMate)
                    goto doubleBreak;
            }

            MyBotLogLine($"evaluated depth {p.Depth} in {timer.MillisecondsElapsedThisTurn} / {turnTimeAllowed}");

            if (++p.Depth > m_MaxDepth)
                break;
        }
        while (timer.MillisecondsElapsedThisTurn * 5 < turnTimeAllowed);

        doubleBreak:

        m_TranspositionTable.Clear();

        return scoredMoves;
    }

    public ScoredMove EvaluateMove(Board board, Timer timer, Move move)
    {
        var p = new EvaluationParameters()
        {
            Move = move,
            IsWhite = board.IsWhiteToMove,
        };

        return EvaluateMove(board, timer, p, out _);
    }

    public ScoredMove EvaluateMove(Board board, Timer timer, EvaluationParameters parameters, out bool isInCheckmate)
    {
        board.MakeMove(parameters.Move);

        int score = int.MinValue;
        isInCheckmate = board.IsInCheckmate();

        // Always play checkmate in one
        if (isInCheckmate)
        {
            var evalParams = new BoardEvaluator.EvalParameters();
            score = m_BoardEvaluator.Evaluate(board, evalParams);
        }
        else
        {
            // else evaluate the board position
            score = Evaluate(board, timer, parameters);
        }

        board.UndoMove(parameters.Move);
        return new(parameters.Move, score);
    }

    public abstract int Evaluate(Board node, Timer timer, EvaluationParameters parameters);

    #region Helpers

    [Conditional("debuglog")]
    public static void LogLine(string line)
    {
        Console.WriteLine(line);
    }

    [Conditional("debuglog")]
    public void MyBotLogLine(string line)
    {
        if (this is not MyBot)
            return;

        LogLine(line);
    }

    #endregion
}
