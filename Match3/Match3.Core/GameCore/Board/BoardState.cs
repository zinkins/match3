using System;
using Match3.Core.GameCore.Pieces;
using Match3.Core.GameCore.ValueObjects;

namespace Match3.Core.GameCore.Board;

public sealed class BoardState
{
    private readonly PieceType?[,] cells = new PieceType?[HeightValue, WidthValue];
    private const int WidthValue = 8;
    private const int HeightValue = 8;

    public int Width => WidthValue;
    public int Height => HeightValue;

    public PieceType? GetCell(GridPosition position)
    {
        EnsureInBounds(position);
        return cells[position.Row, position.Column];
    }

    public void SetCell(GridPosition position, PieceType? pieceType)
    {
        EnsureInBounds(position);
        cells[position.Row, position.Column] = pieceType;
    }

    private static void EnsureInBounds(GridPosition position)
    {
        if (position.Row < 0 || position.Row >= HeightValue || position.Column < 0 || position.Column >= WidthValue)
        {
            throw new ArgumentOutOfRangeException(nameof(position), position, "Position is outside the board.");
        }
    }
}
