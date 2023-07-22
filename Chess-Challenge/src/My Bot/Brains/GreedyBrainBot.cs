using ChessChallenge.API;
using System;

public class GreedyBrainBot : BrainBot
{
    public override int Evaluate(Board node, Piece capturedPiece, bool isWhite)
    {
        return capturedPiece.Value();
    }
}