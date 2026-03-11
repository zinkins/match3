using System.Collections.Generic;
using Match3.Core.GameCore.Board;
using Match3.Core.GameCore.Pieces;
using Match3.Core.GameCore.ValueObjects;

namespace Match3.Presentation.Rendering;

public sealed class BoardRenderer
{
    public BoardState? LastRenderedBoard { get; private set; }

    public void Render(BoardState board)
    {
        LastRenderedBoard = board;
    }

    public BoardRenderSnapshot BuildSnapshot(BoardState board, BoardTransform transform)
    {
        var cells = new List<RenderQuad>(board.Width * board.Height);
        var pieces = new List<RenderQuad>(board.Width * board.Height);
        var inset = transform.CellSize * 0.12f;

        for (var row = 0; row < board.Height; row++)
        {
            for (var column = 0; column < board.Width; column++)
            {
                var position = new GridPosition(row, column);
                var world = transform.GridToWorld(position);
                cells.Add(new RenderQuad(
                    world.X,
                    world.Y,
                    transform.CellSize - 1f,
                    transform.CellSize - 1f,
                    PieceVisualConstants.TintDarkGray));

                var pieceType = board.GetCell(position);
                if (pieceType is null)
                {
                    continue;
                }

                var pieceVisual = GetVisual(pieceType.Value);
                pieces.Add(new RenderQuad(
                    world.X + inset,
                    world.Y + inset,
                    transform.CellSize - (2f * inset),
                    transform.CellSize - (2f * inset),
                    pieceVisual.Tint));
            }
        }

        return new BoardRenderSnapshot(cells, pieces);
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
