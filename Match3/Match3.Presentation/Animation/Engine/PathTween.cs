using System.Numerics;

namespace Match3.Presentation.Animation.Engine;

public sealed class PathTween : ITimedAnimation
{
    private readonly IAnimatableNode node;
    private readonly AnimationBinding[] bindings;
    private readonly IReadOnlyList<Vector2> path;
    private readonly float[] cumulativeLengths;
    private readonly float totalLength;
    private readonly float durationSeconds;
    private float elapsedSeconds;

    public PathTween(IAnimatableNode node, IReadOnlyList<Vector2> path, float durationSeconds, bool blocksInput = false)
    {
        this.node = node ?? throw new ArgumentNullException(nameof(node));
        this.path = path ?? throw new ArgumentNullException(nameof(path));
        this.durationSeconds = durationSeconds;
        bindings = [new AnimationBinding(node, AnimationChannel.Position)];
        BlocksInput = blocksInput;

        cumulativeLengths = BuildCumulativeLengths(path, out totalLength);
    }

    public bool IsCompleted => elapsedSeconds >= durationSeconds;

    public bool BlocksInput { get; }

    public IReadOnlyCollection<AnimationBinding> ActiveBindings => bindings;

    public void Update(float deltaTime)
    {
        _ = Advance(deltaTime);
    }

    public float Advance(float deltaTime)
    {
        if (deltaTime < 0f || IsCompleted)
        {
            return 0f;
        }

        var remaining = MathF.Max(0f, durationSeconds - elapsedSeconds);
        var consumed = durationSeconds <= 0f ? 0f : MathF.Min(deltaTime, remaining);
        elapsedSeconds += durationSeconds <= 0f ? 0f : consumed;
        var progress = durationSeconds <= 0f ? 1f : MathF.Min(1f, elapsedSeconds / durationSeconds);
        node.Position = Evaluate(progress);
        return durationSeconds <= 0f ? deltaTime : deltaTime - consumed;
    }

    private Vector2 Evaluate(float progress)
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
