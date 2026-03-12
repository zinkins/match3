namespace Match3.Presentation.Animation.Engine;

public sealed class AnimationHandle
{
    internal AnimationHandle(bool isAccepted, IAnimation? animation)
    {
        IsAccepted = isAccepted;
        animationRef = animation;
    }

    private readonly IAnimation? animationRef;

    public bool IsAccepted { get; }

    public bool IsCompleted => !IsAccepted || animationRef is null || animationRef.IsCompleted;
}
