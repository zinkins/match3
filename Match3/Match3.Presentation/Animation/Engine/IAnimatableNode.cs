using Match3.Core.GameCore.ValueObjects;
using System.Numerics;

namespace Match3.Presentation.Animation.Engine;

public interface IAnimatableNode
{
    NodeId Id { get; }

    GridPosition LogicalCell { get; set; }

    Vector2 Position { get; set; }

    Vector2 Scale { get; set; }

    float Rotation { get; set; }

    float Opacity { get; set; }

    string Tint { get; set; }

    float Glow { get; set; }

    bool IsVisible { get; set; }
}
