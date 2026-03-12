namespace Match3.Presentation.Animation.Engine;

public sealed class DelayAnimation(float durationSeconds, bool blocksInput = false) : IAnimation
{
    public bool IsCompleted => elapsedSeconds >= durationSeconds;

    public bool BlocksInput { get; } = blocksInput;

    public IReadOnlyCollection<AnimationBinding> ActiveBindings => [];

    private float elapsedSeconds;

    public void Update(float deltaTime)
    {
        if (deltaTime < 0f || IsCompleted)
        {
            return;
        }

        elapsedSeconds += deltaTime;
    }
}

