using Match3.Core.GameCore.Board;
using Match3.Core.GameCore.Pieces;

namespace Match3.Presentation.Rendering;

public sealed class BoardRenderer
{
    public BoardState? LastRenderedBoard { get; private set; }

    public void Render(BoardState board)
    {
        LastRenderedBoard = board;
    }

    public PieceVisual GetVisual(PieceType pieceType)
    {
        return pieceType switch
        {
            PieceType.Red => new PieceVisual("Circle", "Red"),
            PieceType.Green => new PieceVisual("Square", "Green"),
            PieceType.Blue => new PieceVisual("Diamond", "Blue"),
            PieceType.Yellow => new PieceVisual("Triangle", "Yellow"),
            PieceType.Purple => new PieceVisual("Hexagon", "Purple"),
            _ => new PieceVisual("Unknown", "White")
        };
    }
}
