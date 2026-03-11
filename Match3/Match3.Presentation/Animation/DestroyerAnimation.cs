using System;
using System.Collections.Generic;
using System.Numerics;

namespace Match3.Presentation.Animation;

public sealed class DestroyerAnimation
{
    private readonly IReadOnlyList<Vector2> path;

    public DestroyerAnimation(IReadOnlyList<Vector2> path)
    {
        this.path = path ?? throw new ArgumentNullException(nameof(path));
    }

    public Vector2 Evaluate(float progress)
    {
        if (path.Count == 0)
        {
            return Vector2.Zero;
        }

        if (path.Count == 1)
        {
            return path[0];
        }

        var eased = Easing.SmoothStep(progress);
        var scaled = eased * (path.Count - 1);
        var segment = Math.Min((int)scaled, path.Count - 2);
        var local = scaled - segment;
        return Vector2.Lerp(path[segment], path[segment + 1], local);
    }
}
