using System;
using Match3.Core.GameCore.Pieces;
using Match3.Core.GameCore.ValueObjects;

namespace Match3.Core.GameCore.Board;

public sealed class RefillResolver
{
    private readonly IRandomSource randomSource;

    public RefillResolver()
        : this(new SystemRandomSource())
    {
    }

    public RefillResolver(IRandomSource randomSource)
    {
        this.randomSource = randomSource ?? throw new ArgumentNullException(nameof(randomSource));
    }

    public void Refill(BoardState board)
    {
        for (var row = 0; row < board.Height; row++)
        {
            for (var column = 0; column < board.Width; column++)
            {
                var position = new GridPosition(row, column);
                if (board.GetCell(position) is null)
                {
                    board.SetCell(position, NextPieceType());
                }
            }
        }
    }

    private PieceType NextPieceType()
    {
        var index = randomSource.Next(0, PieceCatalog.All.Count);
        return PieceCatalog.All[index];
    }
}
