using ChessChallenge.API;
using System;

public class GreedyBrainBot : BrainBot
{
    public override int Evaluate(Board node, Timer timer, Move move, bool isWhite)
    {
        return move.CapturePieceType.Value();
    }
}