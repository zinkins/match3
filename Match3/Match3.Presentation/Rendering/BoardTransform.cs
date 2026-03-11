using System;
using System.Numerics;
using Match3.Core.GameCore.ValueObjects;

namespace Match3.Presentation.Rendering;

public sealed class BoardTransform
{
    public BoardTransform(float cellSize, Vector2 origin, int rows = 8, int columns = 8)
    {
        if (cellSize <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(cellSize));
        }

        CellSize = cellSize;
        Origin = origin;
        Rows = rows;
        Columns = columns;
    }

    public float CellSize { get; }

    public Vector2 Origin { get; }

    public int Rows { get; }

    public int Columns { get; }

    public Vector2 GridToWorld(GridPosition gridPosition)
    {
        return new Vector2(
            Origin.X + (gridPosition.Column * CellSize),
            Origin.Y + (gridPosition.Row * CellSize));
    }

    public bool TryWorldToGrid(Vector2 worldPosition, out GridPosition gridPosition)
    {
        var column = (int)Math.Floor((worldPosition.X - Origin.X) / CellSize);
        var row = (int)Math.Floor((worldPosition.Y - Origin.Y) / CellSize);

        if (row < 0 || row >= Rows || column < 0 || column >= Columns)
        {
            gridPosition = default;
            return false;
        }

        gridPosition = new GridPosition(row, column);
        return true;
    }
}
