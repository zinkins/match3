using System;
using Match3.Core.GameCore.Bonuses;
using Match3.Core.GameCore.Pieces;
using Match3.Core.GameCore.ValueObjects;

namespace Match3.Core.GameCore.Board;

public sealed class BoardState
{
    private readonly CellContent[,] cells = new CellContent[HeightValue, WidthValue];
    private const int WidthValue = 8;
    private const int HeightValue = 8;

    public int Width => WidthValue;
    public int Height => HeightValue;

    public PieceType? GetCell(GridPosition position)
    {
        EnsureInBounds(position);
        return cells[position.Row, position.Column]?.PieceType;
    }

    public void SetCell(GridPosition position, PieceType? pieceType)
    {
        EnsureInBounds(position);
        cells[position.Row, position.Column] = pieceType is null ? null : new CellContent(pieceType.Value);
    }

    public CellContent GetContent(GridPosition position)
    {
        EnsureInBounds(position);
        return cells[position.Row, position.Column];
    }

    public void SetContent(GridPosition position, CellContent content)
    {
        EnsureInBounds(position);
        cells[position.Row, position.Column] = content is null ? null : NormalizeContent(position, content);
    }

    public BonusToken GetBonus(GridPosition position)
    {
        EnsureInBounds(position);
        return cells[position.Row, position.Column]?.Bonus;
    }

    public void SetBonus(GridPosition position, BonusToken bonus)
    {
        EnsureInBounds(position);
        var current = cells[position.Row, position.Column];
        if (current is null)
        {
            throw new InvalidOperationException("Cannot assign a bonus to an empty cell.");
        }

        cells[position.Row, position.Column] = NormalizeContent(position, current with { Bonus = bonus, IsFreshBonus = false });
    }

    public void MarkAllBonusesAsSettled()
    {
        for (var row = 0; row < HeightValue; row++)
        {
            for (var column = 0; column < WidthValue; column++)
            {
                var current = cells[row, column];
                if (current?.Bonus is null || !current.IsFreshBonus)
                {
                    continue;
                }

                var position = new GridPosition(row, column);
                cells[row, column] = NormalizeContent(position, current with { IsFreshBonus = false });
            }
        }
    }

    public BoardState Clone()
    {
        var clone = new BoardState();
        for (var row = 0; row < HeightValue; row++)
        {
            for (var column = 0; column < WidthValue; column++)
            {
                var position = new GridPosition(row, column);
                clone.cells[row, column] = cells[row, column] is null ? null : NormalizeContent(position, cells[row, column]);
            }
        }

        return clone;
    }

    private static CellContent NormalizeContent(GridPosition position, CellContent content)
    {
        if (content.Bonus is null)
        {
            return content;
        }

        var bonus = content.Bonus switch
        {
            LineBonus line => line with { Position = position, Color = PieceCatalog.GetColor(content.PieceType) },
            BombBonus bomb => bomb with { Position = position, Color = PieceCatalog.GetColor(content.PieceType) },
            _ => content.Bonus
        };

        return content with { Bonus = bonus };
    }

    private static void EnsureInBounds(GridPosition position)
    {
        if (position.Row < 0 || position.Row >= HeightValue || position.Column < 0 || position.Column >= WidthValue)
        {
            throw new ArgumentOutOfRangeException(nameof(position), position, "Position is outside the board.");
        }
    }
}
