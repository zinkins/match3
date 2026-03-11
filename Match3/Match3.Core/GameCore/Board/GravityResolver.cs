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
                var content = board.GetContent(new GridPosition(row, column));
                if (content is null)
                {
                    continue;
                }

                if (targetRow != row)
                {
                    board.SetContent(new GridPosition(targetRow, column), content);
                    board.SetContent(new GridPosition(row, column), null);
                }

                targetRow--;
            }

            while (targetRow >= 0)
            {
                board.SetContent(new GridPosition(targetRow, column), null);
                targetRow--;
            }
        }
    }
}
