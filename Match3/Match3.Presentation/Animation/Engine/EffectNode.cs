using Match3.Core.GameCore.ValueObjects;
using System.Numerics;

namespace Match3.Presentation.Animation.Engine;

public sealed class EffectNode(
    NodeId id,
    GridPosition logicalCell,
    Vector2 position,
    Vector2 scale,
    float rotation,
    float opacity,
    string tint,
    float glow,
    bool isVisible,
    string shape,
    float width,
    float height,
    float layer) : IAnimatableNode
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

    public string Shape { get; set; } = shape;

    public float Width { get; set; } = width;

    public float Height { get; set; } = height;

    public float Layer { get; set; } = layer;
}
