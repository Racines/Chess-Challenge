using ChessChallenge.API;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

public record class ScoredMove(Move Move, int Score)
{
    public override string ToString()
    {
        return $"({Move}, {Score})";
    }
}

public class ScoredMoves : List<ScoredMove>
{
    static Random s_Rng = new();

    /// <summary>
    /// Get random move among best moves
    /// </summary>
    /// <returns></returns>
    public Move GetBestMove(bool isMaximizing)
    {
        var score = 0;
        if (isMaximizing)
            score = this.Max(x => x.Score);
        else
            score = this.Min(x => x.Score);

        var bestMoves = this.Where(x => x.Score == score).ToArray();

        // Select random move among best moves
        return bestMoves[s_Rng.Next(bestMoves.Length)].Move;
    }

    public void SortByScoreDesc()
    {
        this.Sort((a, b) => b.Score.CompareTo(a.Score));
        this.Sort((a, b) =>
        {
            var diff = b.Score.CompareTo(a.Score);
            if (diff != 0)
                return diff;

            return a.Move.ToString().CompareTo(b.Move.ToString());
        });
    }

    public override string ToString()
    {
        StringBuilder sb = new StringBuilder();
        foreach (var scoredMove in this)
        {
            sb.AppendLine(scoredMove.ToString());
        }

        return sb.ToString();
    }
}
