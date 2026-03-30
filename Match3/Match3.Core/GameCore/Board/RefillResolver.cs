using System;
using Match3.Core.GameCore.Pieces;
using Match3.Core.GameCore.ValueObjects;

namespace Match3.Core.GameCore.Board;

public sealed class RefillResolver
{
    private readonly PieceFillPolicy fillPolicy;
    private readonly IRandomSource randomSource;

    public RefillResolver()
        : this(new SystemRandomSource(), new PieceFillPolicy())
    {
    }

    public RefillResolver(IRandomSource randomSource)
        : this(randomSource, new PieceFillPolicy())
    {
    }

    public RefillResolver(IRandomSource randomSource, PieceFillPolicy fillPolicy)
    {
        this.randomSource = randomSource ?? throw new ArgumentNullException(nameof(randomSource));
        this.fillPolicy = fillPolicy ?? throw new ArgumentNullException(nameof(fillPolicy));
    }

    public void Refill(BoardState board)
    {
        for (var row = 0; row < board.Height; row++)
        {
            for (var column = 0; column < board.Width; column++)
            {
                var position = new GridPosition(row, column);
                if (board.GetContent(position) is null)
                {
                    board.SetContent(position, new CellContent(fillPolicy.ChoosePiece(board, position, randomSource)));
                }
            }
        }
    }
}
