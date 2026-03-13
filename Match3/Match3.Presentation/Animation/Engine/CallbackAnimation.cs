namespace Match3.Presentation.Animation.Engine;

public sealed class CallbackAnimation(Action callback) : ITimedAnimation
{
    private readonly Action callback = callback ?? throw new ArgumentNullException(nameof(callback));
    private bool hasInvoked;

    public bool IsCompleted => hasInvoked;

    public bool BlocksInput => false;

    public IReadOnlyCollection<AnimationBinding> ActiveBindings => [];

    public void Update(float deltaTime)
    {
        _ = Advance(deltaTime);
    }

    public float Advance(float deltaTime)
    {
        if (hasInvoked)
        {
            return deltaTime;
        }

        callback();
        hasInvoked = true;
        return deltaTime;
    }
}
