using System.Numerics;
using Match3.Core.GameCore.ValueObjects;
using Match3.Presentation.Animation.Engine;

namespace Match3.Presentation.Rendering;

public sealed class PieceNodeRenderer
{
    public BoardRenderSnapshot BuildSnapshot(BoardRenderSnapshot snapshot, BoardViewState viewState, AnimationPlayer? animationPlayer = null)
    {
        ArgumentNullException.ThrowIfNull(snapshot);
        ArgumentNullException.ThrowIfNull(viewState);

        var activePositions = snapshot.Pieces.Select(piece => piece.Position).ToHashSet();
        foreach (var node in viewState.PieceNodes.ToArray())
        {
            if (!activePositions.Contains(node.LogicalCell))
            {
                viewState.RemoveNode(node.Id);
            }
        }

        foreach (var piece in snapshot.Pieces)
        {
            var node = viewState.GetPieceNode(piece.Position);
            if (node is not null)
            {
                node.LogicalCell = piece.Position;
                if (OperatingSystem.IsAndroid() && (animationPlayer is null || !animationPlayer.HasBinding(node, AnimationChannel.Position)))
                {
                    node.Position = new Vector2(piece.X, piece.Y);
                }

                if (OperatingSystem.IsAndroid() && (animationPlayer is null || !animationPlayer.HasBinding(node, AnimationChannel.Rotation)))
                {
                    node.Rotation = piece.Rotation;
                }

                if (OperatingSystem.IsAndroid() && (animationPlayer is null || !animationPlayer.HasBinding(node, AnimationChannel.Scale)))
                {
                    node.Scale = new Vector2(1f, 1f);
                }

                if (!node.IsVisible)
                {
                    node.Tint = piece.Tint;
                    node.IsVisible = true;
                }
                viewState.AddOrUpdate(node);
                continue;
            }

            viewState.AddOrUpdate(new PieceNode(
                NodeId.New(),
                piece.Position,
                new Vector2(piece.X, piece.Y),
                new Vector2(1f, 1f),
                piece.Rotation,
                opacity: 1f,
                piece.Tint,
                glow: 0f,
                isVisible: true));
        }

        var pieces = snapshot.Pieces
            .Select(piece => BuildPiece(piece, viewState.GetPieceNode(piece.Position)))
            .Where(piece => piece is not null)
            .Select(piece => piece!)
            .ToArray();

        return new BoardRenderSnapshot(snapshot.Cells, pieces);
    }

    private static RenderPiece? BuildPiece(RenderPiece basePiece, PieceNode? node)
    {
        if (node is null || !node.IsVisible)
        {
            return null;
        }

        var width = basePiece.Width * node.Scale.X;
        var height = basePiece.Height * node.Scale.Y;
        var x = node.Position.X - ((width - basePiece.Width) / 2f);
        var y = node.Position.Y - ((height - basePiece.Height) / 2f);
        return new RenderPiece(
            basePiece.Position,
            basePiece.Shape,
            node.Tint,
            x,
            y,
            width,
            height,
            node.Rotation,
            basePiece.Layer);
    }
}

