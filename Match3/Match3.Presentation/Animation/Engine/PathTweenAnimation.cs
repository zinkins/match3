using System.Numerics;

namespace Match3.Presentation.Animation.Engine;

public sealed class PathTweenAnimation : ITimedAnimation
{
    private readonly AnimationBinding[] bindings;
    private readonly Action<Vector2> setter;
    private readonly Func<float, Vector2> evaluate;
    private readonly float durationSeconds;
    private float elapsedSeconds;

    public PathTweenAnimation(
        object target,
        Action<Vector2> setter,
        Func<float, Vector2> evaluate,
        float durationSeconds,
        bool blocksInput = false)
    {
        ArgumentNullException.ThrowIfNull(target);
        this.setter = setter ?? throw new ArgumentNullException(nameof(setter));
        this.evaluate = evaluate ?? throw new ArgumentNullException(nameof(evaluate));
        this.durationSeconds = durationSeconds;
        bindings = [new AnimationBinding(target, AnimationChannel.Position)];
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
        var consumed = durationSeconds <= 0f
            ? 0f
            : MathF.Min(deltaTime, remaining);
        elapsedSeconds += durationSeconds <= 0f ? 0f : consumed;
        var progress = durationSeconds <= 0f ? 1f : MathF.Min(1f, elapsedSeconds / durationSeconds);
        setter(evaluate(progress));
        return durationSeconds <= 0f ? deltaTime : deltaTime - consumed;
    }
}
