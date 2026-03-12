namespace Match3.Presentation.Animation.Engine;

public sealed class CallbackAnimation(Action callback) : IAnimation
{
    private readonly Action callback = callback ?? throw new ArgumentNullException(nameof(callback));
    private bool hasInvoked;

    public bool IsCompleted => hasInvoked;

    public bool BlocksInput => false;

    public IReadOnlyCollection<AnimationBinding> ActiveBindings => [];

    public void Update(float deltaTime)
    {
        if (hasInvoked)
        {
            return;
        }

        callback();
        hasInvoked = true;
    }
}
