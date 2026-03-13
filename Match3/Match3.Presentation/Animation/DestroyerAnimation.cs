using System;
using System.Collections.Generic;
using System.Numerics;

namespace Match3.Presentation.Animation;

public sealed class DestroyerAnimation
{
    private readonly IReadOnlyList<Vector2> path;
    private readonly float[] cumulativeLengths;
    private readonly float totalLength;

    public DestroyerAnimation(IReadOnlyList<Vector2> path)
    {
        this.path = path ?? throw new ArgumentNullException(nameof(path));
        cumulativeLengths = BuildCumulativeLengths(path, out totalLength);
    }

    public Vector2 Evaluate(float progress)
    {
        if (path.Count == 0)
        {
            return Vector2.Zero;
        }

        if (path.Count == 1 || totalLength <= 0f)
        {
            return path[0];
        }

        var clamped = Math.Clamp(progress, 0f, 1f);
        var distance = totalLength * clamped;
        var segmentIndex = 0;
        while (segmentIndex < cumulativeLengths.Length - 1 && cumulativeLengths[segmentIndex + 1] < distance)
        {
            segmentIndex++;
        }

        var segmentStart = cumulativeLengths[segmentIndex];
        var segmentEnd = cumulativeLengths[segmentIndex + 1];
        var segmentLength = segmentEnd - segmentStart;
        if (segmentLength <= 0f)
        {
            return path[segmentIndex + 1];
        }

        var local = (distance - segmentStart) / segmentLength;
        return Vector2.Lerp(path[segmentIndex], path[segmentIndex + 1], local);
    }

    private static float[] BuildCumulativeLengths(IReadOnlyList<Vector2> path, out float totalLength)
    {
        if (path.Count == 0)
        {
            totalLength = 0f;
            return [0f];
        }

        var cumulative = new float[path.Count];
        totalLength = 0f;
        for (var i = 1; i < path.Count; i++)
        {
            totalLength += Vector2.Distance(path[i - 1], path[i]);
            cumulative[i] = totalLength;
        }

        return cumulative;
    }
}
