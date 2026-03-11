using Match3.Core.GameCore.Board;
using Match3.Core.GameCore.Pieces;
using Match3.Core.GameCore.ValueObjects;

namespace Match3.Tests;

public static class BoardTestExtensions
{
    public static PieceType? GetPiece(this BoardState board, GridPosition position)
    {
        return board.GetContent(position)?.PieceType;
    }

    public static void SetPiece(this BoardState board, GridPosition position, PieceType? pieceType)
    {
        board.SetContent(position, pieceType is null ? null : new CellContent(pieceType.Value));
    }
}
