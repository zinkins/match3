using System.Numerics;

namespace Match3.Presentation.Animation.Engine;

public sealed class ScaleTween : ITimedAnimation
{
    private readonly IAnimatableNode node;
    private readonly AnimationBinding[] bindings;
    private readonly Vector2 from;
    private readonly Vector2 to;
    private readonly float durationSeconds;
    private float elapsedSeconds;

    public ScaleTween(IAnimatableNode node, Vector2 to, float durationSeconds, bool blocksInput = false)
    {
        this.node = node ?? throw new ArgumentNullException(nameof(node));
        from = node.Scale;
        this.to = to;
        this.durationSeconds = durationSeconds;
        bindings = [new AnimationBinding(node, AnimationChannel.Scale)];
        BlocksInput = blocksInput;
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
        node.Scale = Vector2.Lerp(from, to, progress);
        return durationSeconds <= 0f ? deltaTime : deltaTime - consumed;
    }
}
