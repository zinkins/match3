using Match3.Core.GameCore.ValueObjects;

namespace Match3.Core.GameCore.Board;

public sealed class MoveValidator
{
    public bool IsValid(Move move)
    {
        if (move.From == move.To)
        {
            return false;
        }

        return move.From.IsAdjacentTo(move.To);
    }
}
