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
            PieceType.Red => new PieceVisual(PieceVisualConstants.ShapeSquare, PieceVisualConstants.TintRed),
            PieceType.Green => new PieceVisual(PieceVisualConstants.ShapeSquare, PieceVisualConstants.TintGreen),
            PieceType.Blue => new PieceVisual(PieceVisualConstants.ShapeSquare, PieceVisualConstants.TintBlue),
            PieceType.Yellow => new PieceVisual(PieceVisualConstants.ShapeSquare, PieceVisualConstants.TintYellow),
            PieceType.Purple => new PieceVisual(PieceVisualConstants.ShapeSquare, PieceVisualConstants.TintPurple),
            _ => new PieceVisual(PieceVisualConstants.ShapeUnknown, PieceVisualConstants.TintWhite)
        };
    }
}
