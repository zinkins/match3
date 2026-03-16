namespace Match3.Presentation.Animation.Engine;

public sealed class FloatTween : ITimedAnimation
{
    private readonly Action<float> setter;
    private readonly AnimationBinding[] bindings;
    private readonly float from;
    private readonly float to;
    private readonly float durationSeconds;
    private readonly Func<float, float, float, float> interpolate;
    private float elapsedSeconds;

    public FloatTween(
        object target,
        AnimationChannel channel,
        Func<float> getter,
        Action<float> setter,
        float from,
        float to,
        float durationSeconds,
        Func<float, float, float, float> interpolate,
        bool blocksInput = false)
    {
        ArgumentNullException.ThrowIfNull(target);
        ArgumentNullException.ThrowIfNull(getter);
        this.setter = setter ?? throw new ArgumentNullException(nameof(setter));
        this.interpolate = interpolate ?? throw new ArgumentNullException(nameof(interpolate));
        this.from = from;
        this.to = to;
        this.durationSeconds = durationSeconds;
        bindings = [new AnimationBinding(target, channel)];
        BlocksInput = blocksInput;
        _ = getter();
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
