using Match3.Core.GameCore.Pieces;
using Match3.Core.GameCore.ValueObjects;

namespace Match3.Core.GameCore.Board;

public sealed class GravityResolver
{
    public void Apply(BoardState board)
    {
        for (var column = 0; column < board.Width; column++)
        {
            var targetRow = board.Height - 1;

            for (var row = board.Height - 1; row >= 0; row--)
            {
                var piece = board.GetCell(new GridPosition(row, column));
                if (piece is null)
                {
                    continue;
                }

                if (targetRow != row)
                {
                    board.SetCell(new GridPosition(targetRow, column), piece);
                    board.SetCell(new GridPosition(row, column), null);
                }

                targetRow--;
            }

            while (targetRow >= 0)
            {
                board.SetCell(new GridPosition(targetRow, column), null);
                targetRow--;
            }
        }
    }
}
