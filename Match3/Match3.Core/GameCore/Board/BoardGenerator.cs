using System;
using Match3.Core.GameCore.Pieces;
using Match3.Core.GameCore.ValueObjects;

namespace Match3.Core.GameCore.Board;

public sealed class BoardGenerator
{
    private readonly IRandomSource randomSource;

    public BoardGenerator()
        : this(new SystemRandomSource())
    {
    }

    public BoardGenerator(IRandomSource randomSource)
    {
        this.randomSource = randomSource ?? throw new ArgumentNullException(nameof(randomSource));
    }

    public BoardState Generate()
    {
        var board = new BoardState();

        for (var row = 0; row < board.Height; row++)
        {
            for (var column = 0; column < board.Width; column++)
            {
                var type = NextPieceType();
                board.SetCell(new GridPosition(row, column), type);
            }
        }

        return board;
    }

    private PieceType NextPieceType()
    {
        var index = randomSource.Next(0, PieceCatalog.All.Count);
        return PieceCatalog.All[index];
    }
}
