namespace Match3.Presentation.Animation.Engine;

public sealed class PropertyTween<T> : ITimedAnimation
{
    private readonly Func<T> getter;
    private readonly Action<T> setter;
    private readonly Func<T, T, float, T> interpolate;
    private readonly AnimationBinding[] bindings;
    private readonly T from;
    private readonly T to;
    private readonly float durationSeconds;
    private float elapsedSeconds;

    public PropertyTween(
        object target,
        AnimationChannel channel,
        Func<T> getter,
        Action<T> setter,
        T from,
        T to,
        float durationSeconds,
        Func<T, T, float, T> interpolate,
        bool blocksInput = false)
    {
        ArgumentNullException.ThrowIfNull(target);
        this.getter = getter ?? throw new ArgumentNullException(nameof(getter));
        this.setter = setter ?? throw new ArgumentNullException(nameof(setter));
        this.interpolate = interpolate ?? throw new ArgumentNullException(nameof(interpolate));
        this.from = from;
        this.to = to;
        this.durationSeconds = durationSeconds;
        bindings = [new AnimationBinding(target, channel)];
        BlocksInput = blocksInput;
        _ = this.getter();
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
        setter(interpolate(from, to, progress));
        return durationSeconds <= 0f ? deltaTime : deltaTime - consumed;
    }
}
