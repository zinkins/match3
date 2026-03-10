using System;

namespace Match3.Core.GameCore.ValueObjects;

public readonly record struct GridPosition(int Row, int Column)
{
    public bool IsAdjacentTo(GridPosition other)
    {
        var rowDistance = Math.Abs(Row - other.Row);
        var columnDistance = Math.Abs(Column - other.Column);
        return rowDistance + columnDistance == 1;
    }
}
