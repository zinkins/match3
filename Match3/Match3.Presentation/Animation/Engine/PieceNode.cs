using Match3.Core.GameCore.ValueObjects;
using System.Numerics;

namespace Match3.Presentation.Animation.Engine;

public sealed class PieceNode(
    NodeId id,
    GridPosition logicalCell,
    Vector2 position,
    Vector2 scale,
    float rotation,
    float opacity,
    string tint,
    float glow,
    bool isVisible) : IAnimatableNode
{
    public NodeId Id { get; } = id;

    public GridPosition LogicalCell { get; set; } = logicalCell;

    public Vector2 Position { get; set; } = position;

    public Vector2 Scale { get; set; } = scale;

    public float Rotation { get; set; } = rotation;

    public float Opacity { get; set; } = opacity;

    public string Tint { get; set; } = tint;

    public float Glow { get; set; } = glow;

    public bool IsVisible { get; set; } = isVisible;
}
