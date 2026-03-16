using System.Numerics;

namespace Match3.Presentation.Animation.Engine;

public sealed class PopScaleTween : ITimedAnimation
{
    private readonly IAnimatableNode node;
    private readonly AnimationBinding[] bindings;
    private readonly float burstAmplitude;
    private readonly float durationSeconds;
    private float elapsedSeconds;

    public PopScaleTween(IAnimatableNode node, float burstAmplitude, float durationSeconds, bool blocksInput = false)
    {
        this.node = node ?? throw new ArgumentNullException(nameof(node));
        this.burstAmplitude = burstAmplitude;
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
        var burst = 1f + (burstAmplitude * MathF.Sin(progress * MathF.PI));
        var shrink = 1f - Easing.SmoothStep(progress);
        var value = MathF.Max(0f, burst * shrink);
        node.Scale = new Vector2(value, value);

        return durationSeconds <= 0f ? deltaTime : deltaTime - consumed;
    }
}
