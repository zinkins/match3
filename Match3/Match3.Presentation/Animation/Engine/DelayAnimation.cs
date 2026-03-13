namespace Match3.Presentation.Animation.Engine;

public sealed class DelayAnimation(float durationSeconds, bool blocksInput = false) : ITimedAnimation
{
    public bool IsCompleted => elapsedSeconds >= durationSeconds;

    public bool BlocksInput { get; } = blocksInput;

    public IReadOnlyCollection<AnimationBinding> ActiveBindings => [];

    private float elapsedSeconds;

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
        var consumed = MathF.Min(deltaTime, remaining);
        elapsedSeconds += consumed;
        return deltaTime - consumed;
    }
}

