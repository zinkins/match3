using System;
using Match3.Core.GameCore.Pieces;
using Match3.Core.GameCore.ValueObjects;

namespace Match3.Core.GameCore.Board;

public sealed class PieceFillPolicy
{
    public PieceType ChoosePiece(BoardState board, GridPosition position, IRandomSource randomSource)
    {
        ArgumentNullException.ThrowIfNull(board);
        ArgumentNullException.ThrowIfNull(randomSource);

        for (var attempt = 0; attempt < PieceCatalog.All.Count * 2; attempt++)
        {
            var candidate = NextPieceType(randomSource);
            if (!CreatesImmediateMatch(board, position, candidate))
            {
                return candidate;
            }
        }

        foreach (var candidate in PieceCatalog.All)
        {
            if (!CreatesImmediateMatch(board, position, candidate))
            {
                return candidate;
            }
        }

        return NextPieceType(randomSource);
    }

    private static bool CreatesImmediateMatch(BoardState board, GridPosition position, PieceType candidate)
    {
        if (position.Column >= 2 &&
            board.GetContent(new GridPosition(position.Row, position.Column - 1))?.PieceType == candidate &&
            board.GetContent(new GridPosition(position.Row, position.Column - 2))?.PieceType == candidate)
        {
            return true;
        }

        if (position.Row >= 2 &&
            board.GetContent(new GridPosition(position.Row - 1, position.Column))?.PieceType == candidate &&
            board.GetContent(new GridPosition(position.Row - 2, position.Column))?.PieceType == candidate)
        {
            return true;
        }

        return false;
    }

    private static PieceType NextPieceType(IRandomSource randomSource)
    {
        var index = randomSource.Next(0, PieceCatalog.All.Count);
        return PieceCatalog.All[index];
    }
}
