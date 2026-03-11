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
                if (board.GetContent(position) is null)
                {
                    board.SetContent(position, new CellContent(NextPieceTypeAvoidingImmediateMatch(board, row, column)));
                }
            }
        }
    }

    private PieceType NextPieceTypeAvoidingImmediateMatch(BoardState board, int row, int column)
    {
        for (var attempt = 0; attempt < PieceCatalog.All.Count * 2; attempt++)
        {
            var candidate = NextPieceType();
            if (!CreatesImmediateMatch(board, row, column, candidate))
            {
                return candidate;
            }
        }

        foreach (var candidate in PieceCatalog.All)
        {
            if (!CreatesImmediateMatch(board, row, column, candidate))
            {
                return candidate;
            }
        }

        return NextPieceType();
    }

    private static bool CreatesImmediateMatch(BoardState board, int row, int column, PieceType candidate)
    {
        if (column >= 2 &&
            board.GetContent(new GridPosition(row, column - 1))?.PieceType == candidate &&
            board.GetContent(new GridPosition(row, column - 2))?.PieceType == candidate)
        {
            return true;
        }

        if (row >= 2 &&
            board.GetContent(new GridPosition(row - 1, column))?.PieceType == candidate &&
            board.GetContent(new GridPosition(row - 2, column))?.PieceType == candidate)
        {
            return true;
        }

        return false;
    }

    private PieceType NextPieceType()
    {
        var index = randomSource.Next(0, PieceCatalog.All.Count);
        return PieceCatalog.All[index];
    }
}
