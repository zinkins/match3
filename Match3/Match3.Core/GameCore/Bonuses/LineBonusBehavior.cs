using System.Collections.Generic;
using Match3.Core.GameCore.Board;
using Match3.Core.GameCore.ValueObjects;

namespace Match3.Core.GameCore.Bonuses;

public sealed class LineBonusBehavior
{
    public Destroyer Activate(
        LineBonus bonus,
        BoardState board,
        IReadOnlyDictionary<GridPosition, BonusToken> bonusesOnBoard)
    {
        var path = BuildPath(bonus, board);
        var destroyed = new List<GridPosition>(path.Count);
        var activatedBonuses = new List<BonusToken>();

        foreach (var position in path)
        {
            board.SetCell(position, null);
            destroyed.Add(position);

            if (bonusesOnBoard.TryGetValue(position, out var triggered) && triggered.Position != bonus.Position)
            {
                activatedBonuses.Add(triggered);
            }
        }

        return new Destroyer(path, destroyed, activatedBonuses);
    }

    private static IReadOnlyList<GridPosition> BuildPath(LineBonus bonus, BoardState board)
    {
        var path = new List<GridPosition>();
        if (bonus.Orientation == LineOrientation.Horizontal)
        {
            for (var column = 0; column < board.Width; column++)
            {
                path.Add(new GridPosition(bonus.Position.Row, column));
            }
        }
        else
        {
            for (var row = 0; row < board.Height; row++)
            {
                path.Add(new GridPosition(row, bonus.Position.Column));
            }
        }

        return path;
    }
}
