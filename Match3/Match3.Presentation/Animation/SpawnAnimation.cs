using System.Numerics;

namespace Match3.Presentation.Animation;

public sealed class SpawnAnimation
{
    public SpawnAnimation(Vector2 spawnAbove, Vector2 target)
    {
        SpawnAbove = spawnAbove;
        Target = target;
    }

    public Vector2 SpawnAbove { get; }

    public Vector2 Target { get; }

    public Vector2 Evaluate(float progress)
    {
        var eased = Easing.SmoothStep(progress);
        return Vector2.Lerp(SpawnAbove, Target, eased);
    }
}
