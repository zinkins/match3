using System.Collections.Generic;
using Match3.Core.GameCore.Board;
using Match3.Core.GameCore.ValueObjects;

namespace Match3.Core.GameCore.Bonuses;

public sealed class BombBonusBehavior
{
    public ExplosionResult Activate(
        BombBonus bonus,
        BoardState board,
        IReadOnlyDictionary<GridPosition, BonusToken> bonusesOnBoard)
    {
        var area = BuildArea(bonus, board);
        var destroyed = new List<GridPosition>(area.Count);
        var activatedBonuses = new List<BonusToken>();

        foreach (var position in area)
        {
            board.SetCell(position, null);
            destroyed.Add(position);

            if (bonusesOnBoard.TryGetValue(position, out var triggered) && triggered.Position != bonus.Position)
            {
                activatedBonuses.Add(triggered);
            }
        }

        return new ExplosionResult(area, destroyed, activatedBonuses);
    }

    private static IReadOnlyList<GridPosition> BuildArea(BombBonus bonus, BoardState board)
    {
        var affected = new List<GridPosition>();

        var fromRow = bonus.Position.Row - bonus.Radius;
        var toRow = bonus.Position.Row + bonus.Radius;
        var fromColumn = bonus.Position.Column - bonus.Radius;
        var toColumn = bonus.Position.Column + bonus.Radius;

        for (var row = fromRow; row <= toRow; row++)
        {
            if (row < 0 || row >= board.Height)
            {
                continue;
            }

            for (var column = fromColumn; column <= toColumn; column++)
            {
                if (column < 0 || column >= board.Width)
                {
                    continue;
                }

                affected.Add(new GridPosition(row, column));
            }
        }

        return affected;
    }
}
