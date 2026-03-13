using System.Collections.Generic;
using System.Linq;
using Match3.Core.GameCore.Board;
using Match3.Core.GameCore.Pieces;
using Match3.Core.GameCore.ValueObjects;

namespace Match3.Core.GameCore.Bonuses;

public sealed class BonusFactory
{
    public BonusToken Create(IReadOnlyList<MatchGroup> groups, GridPosition lastMovedCell)
    {
        var intersection = FindIntersection(groups);
        if (intersection is not null)
        {
            var intersectionGroup = groups.First(group => group.Positions.Contains(intersection.Value));
            var intersectionColor = PieceCatalog.GetColor(intersectionGroup.PieceType);
            return new BombBonus(intersection.Value, intersectionColor);
        }

        var longestGroup = groups
            .OrderByDescending(group => group.Positions.Count)
            .First();
        var color = PieceCatalog.GetColor(longestGroup.PieceType);

        if (longestGroup.Positions.Count >= 5)
        {
            return new BombBonus(lastMovedCell, color);
        }

        var orientation = GetOrientation(longestGroup);
        return new LineBonus(lastMovedCell, color, orientation);
    }

    private static GridPosition? FindIntersection(IReadOnlyList<MatchGroup> groups)
    {
        for (var i = 0; i < groups.Count; i++)
        {
            var current = groups[i].Positions;
            for (var j = i + 1; j < groups.Count; j++)
            {
                foreach (var position in current)
                {
                    if (groups[j].Positions.Contains(position))
                    {
                        return position;
                    }
                }
            }
        }

        return null;
    }

    private static LineOrientation GetOrientation(MatchGroup group)
    {
        var row = group.Positions[0].Row;
        var isHorizontal = group.Positions.All(position => position.Row == row);
        return isHorizontal ? LineOrientation.Horizontal : LineOrientation.Vertical;
    }
}
