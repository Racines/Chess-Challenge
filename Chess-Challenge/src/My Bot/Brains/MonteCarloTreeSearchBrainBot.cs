using ChessChallenge.API;
using MonteCarlo;
using System;
using System.Collections.Generic;
using System.Linq;

public class ChessState : IState<bool, Move>
{
    public Board Board { get; private set; }

    public bool CurrentPlayer => Board.IsWhiteToMove;

    public IList<Move> Actions => Board.GetLegalMoves();

    public ChessState(Board board)
    {
        this.Board = Board.CreateBoardFromFEN(board.GetFenString());
    }

    public void ApplyAction(Move action)
    {
        Board.MakeMove(action);
    }

    public IState<bool, Move> Clone()
    {
        return new ChessState(Board);
    }

    public double GetResult(bool forPlayer)
    {
        if (Board.IsDraw())
            return 0.5f;

        if (Board.IsInCheckmate())
            return Board.IsWhiteToMove && forPlayer ? 0 : 1;

        return 0;
    }

    public bool IsTerminal()
    {
        return Board.IsTerminal();
    }
}

public class MonteCarloTreeSearchBrainBot : IChessBot
{
    static Random s_Rng = new();

    public Move Think(Board board, Timer timer)
    {
        var moveLeftUntilGameEnd = Math.Max(10, 40 - board.PlyCount);
        var timeBudget = timer.MillisecondsRemaining / moveLeftUntilGameEnd;

        var state = new ChessState(board);
        var topActions = MonteCarloTreeSearch.GetTopActions(state, int.MaxValue, timeBudget, timer).ToList();
        var bestScore = topActions[0].NumRuns;

        var bestMoves = topActions.Where(x => x.NumRuns >= bestScore).Select(x => x.Action).ToArray();
        return bestMoves[s_Rng.Next(bestMoves.Length)];
    }
}