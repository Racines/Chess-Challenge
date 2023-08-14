using ChessChallenge.API;
using System.Linq;

public class BasicMoveOrderer : MoveOrderer
{
    const int c_SquareControlledByOpponentPawnPenalty = 350;
    const int c_CapturedPieceValueMultiplier = 10;

    public override Move[] OrderMoves(Board board, Move[] allMoves)
    {
        return allMoves.OrderByDescending(move =>
        {
            int score = 0;
            var movePieceType = move.MovePieceType;
            var capturePieceType = move.CapturePieceType;

            if (capturePieceType != PieceType.None)
            {
                // Order moves to try capturing the most valuable opponent piece with least valuable of own pieces first
                // The capturedPieceValueMultiplier is used to make even 'bad' captures like QxP rank above non-captures
                score += c_CapturedPieceValueMultiplier * capturePieceType.Value() - movePieceType.Value();
            }

            if (move.IsPromotion)
            {
                score += move.PromotionPieceType.Value();
            }
            else
            {
                // Penalize moving piece to a square attacked by opponent pawn
                if (board.SquareIsAttackedByOpponent(move.TargetSquare))
                    score -= c_SquareControlledByOpponentPawnPenalty;
            }

            return score;
        }).ToArray();
    }
}

public class NoneMoveOrderer : MoveOrderer
{
    public override Move[] OrderMoves(Board board, Move[] allMoves)
    {
        return allMoves;
    }
}

public abstract class MoveOrderer
{
    public abstract Move[] OrderMoves(Board board, Move[] allMoves);
}