using System;
using Match3.Core.GameCore.Pieces;
using Match3.Core.GameCore.ValueObjects;

namespace Match3.Core.GameCore.Board;

public sealed class BoardGenerator
{
    private readonly PieceFillPolicy fillPolicy;
    private readonly IRandomSource randomSource;

    public BoardGenerator()
        : this(new SystemRandomSource(), new PieceFillPolicy())
    {
    }

    public BoardGenerator(IRandomSource randomSource)
        : this(randomSource, new PieceFillPolicy())
    {
    }

    public BoardGenerator(IRandomSource randomSource, PieceFillPolicy fillPolicy)
    {
        this.randomSource = randomSource ?? throw new ArgumentNullException(nameof(randomSource));
        this.fillPolicy = fillPolicy ?? throw new ArgumentNullException(nameof(fillPolicy));
    }

    public BoardState Generate()
    {
        var board = new BoardState();

        for (var row = 0; row < board.Height; row++)
        {
            for (var column = 0; column < board.Width; column++)
            {
                var type = fillPolicy.ChoosePiece(board, new GridPosition(row, column), randomSource);
                board.SetContent(new GridPosition(row, column), new CellContent(type));
            }
        }

        return board;
    }
}
