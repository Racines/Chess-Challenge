using ChessChallenge.API;
using System;
using System.Collections.Generic;
using System.Linq;

public record class ScoredMove(Move Move, int Score);
public class ScoredMoves : List<ScoredMove>
{
    static Random s_Rng = new();

    /// <summary>
    /// Get random move among best moves
    /// </summary>
    /// <returns></returns>
    public Move GetBestMove()
    {
        var maxScore = this.Max(x => x.Score);
        var bestMoves = this.Where(x => x.Score >= maxScore).ToArray();

        // Select random move among best moves
        return bestMoves[s_Rng.Next(bestMoves.Length)].Move;
    }
}
