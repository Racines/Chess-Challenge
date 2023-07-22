using ChessChallenge.API;
using System;

public class GreedyBrainBot : BrainBot
{
    public override int Evaluate(Board node, Piece capturedPiece)
    {
        return capturedPiece.Value();
    }
}