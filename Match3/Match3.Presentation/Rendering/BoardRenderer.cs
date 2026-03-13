using System.Collections.Generic;
using Match3.Core.GameCore.Board;
using Match3.Core.GameCore.Bonuses;
using Match3.Core.GameCore.Pieces;
using Match3.Core.GameCore.ValueObjects;

namespace Match3.Presentation.Rendering;

public sealed class BoardRenderer
{
    public BoardState? LastRenderedBoard { get; private set; }

    /// <summary>
    /// Stores the last board passed through the renderer for inspection-oriented scenarios.
    /// </summary>
    /// <param name="board">Board state considered the current rendered board.</param>
    public void Render(BoardState board)
    {
        LastRenderedBoard = board;
    }

    /// <summary>
    /// Builds a render snapshot of board cells and visible pieces using the current board transform.
    /// </summary>
    /// <param name="board">Board state to project.</param>
    /// <param name="transform">Board transform that converts grid positions into world-space rectangles.</param>
    /// <returns>A snapshot that can be consumed by rendering and animation systems.</returns>
    public BoardRenderSnapshot BuildSnapshot(
        BoardState board,
        BoardTransform transform)
    {
        var cells = new List<RenderQuad>(board.Width * board.Height);
        var pieces = new List<RenderPiece>(board.Width * board.Height);
        var inset = transform.CellSize * BoardRenderStyle.PieceInsetFactor;

        for (var row = 0; row < board.Height; row++)
        {
            for (var column = 0; column < board.Width; column++)
            {
                var position = new GridPosition(row, column);
                var world = transform.GridToWorld(position);
                cells.Add(new RenderQuad(
                    world.X,
                    world.Y,
                    transform.CellSize - BoardRenderStyle.CellBorderThickness,
                    transform.CellSize - BoardRenderStyle.CellBorderThickness,
                    PieceVisualConstants.TintDarkGray));

                var content = board.GetContent(position);
                if (content is null)
                {
                    continue;
                }

                var pieceVisual = content.Bonus is not null
                    ? GetBonusVisual(content.Bonus)
                    : GetVisual(content.PieceType);
                var pieceBounds = content.Bonus is not null
                    ? GetBonusBounds(transform, world, inset, content.Bonus)
                    : (X: world.X + inset, Y: world.Y + inset, Width: transform.CellSize - (2f * inset), Height: transform.CellSize - (2f * inset));
                pieces.Add(new RenderPiece(
                    position,
                    pieceVisual.Shape,
                    pieceVisual.Tint,
                    pieceBounds.X,
                    pieceBounds.Y,
                    pieceBounds.Width,
                    pieceBounds.Height));
            }
        }

        return new BoardRenderSnapshot(cells, pieces);
    }

    /// <summary>
    /// Resolves the visual representation for a regular piece type.
    /// </summary>
    /// <param name="pieceType">Logical piece type.</param>
    /// <returns>Shape and tint used to render the piece.</returns>
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

    /// <summary>
    /// Resolves the visual representation for a bonus token.
    /// </summary>
    /// <param name="bonus">Bonus token to render.</param>
    /// <returns>Shape and tint used to render the bonus.</returns>
    public PieceVisual GetBonusVisual(BonusToken bonus)
    {
        return bonus switch
        {
            LineBonus => new PieceVisual(PieceVisualConstants.ShapeDiamond, GetTint(bonus.Color)),
            BombBonus => new PieceVisual(PieceVisualConstants.ShapeCircle, GetTint(bonus.Color)),
            _ => new PieceVisual(PieceVisualConstants.ShapeUnknown, PieceVisualConstants.TintWhite)
        };
    }

    private static (float X, float Y, float Width, float Height) GetBonusBounds(
        BoardTransform transform,
        System.Numerics.Vector2 world,
        float inset,
        BonusToken bonus)
    {
        var baseSize = transform.CellSize - (2f * inset);
        return bonus switch
        {
            LineBonus { Orientation: LineOrientation.Horizontal } =>
                (world.X + (transform.CellSize * BoardRenderStyle.HorizontalLineOffsetXFactor), world.Y + (transform.CellSize * BoardRenderStyle.HorizontalLineOffsetYFactor), transform.CellSize * BoardRenderStyle.HorizontalLineWidthFactor, transform.CellSize * BoardRenderStyle.HorizontalLineHeightFactor),
            LineBonus { Orientation: LineOrientation.Vertical } =>
                (world.X + (transform.CellSize * BoardRenderStyle.VerticalLineOffsetXFactor), world.Y + (transform.CellSize * BoardRenderStyle.VerticalLineOffsetYFactor), transform.CellSize * BoardRenderStyle.VerticalLineWidthFactor, transform.CellSize * BoardRenderStyle.VerticalLineHeightFactor),
            BombBonus =>
                (world.X + inset, world.Y + inset, baseSize, baseSize),
            _ =>
                (world.X + inset, world.Y + inset, baseSize, baseSize)
        };
    }

    private static string GetTint(PieceColor color)
    {
        return color switch
        {
            PieceColor.Red => PieceVisualConstants.TintRed,
            PieceColor.Green => PieceVisualConstants.TintGreen,
            PieceColor.Blue => PieceVisualConstants.TintBlue,
            PieceColor.Yellow => PieceVisualConstants.TintYellow,
            PieceColor.Purple => PieceVisualConstants.TintPurple,
            _ => PieceVisualConstants.TintWhite
        };
    }
}
