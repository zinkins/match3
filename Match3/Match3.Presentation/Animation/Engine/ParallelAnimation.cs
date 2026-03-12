namespace Match3.Presentation.Animation.Engine;

public sealed class ParallelAnimation(params IAnimation[] children) : IAnimation
{
    private readonly List<IAnimation> children = [.. children.Where(child => child is not null)];

    public bool IsCompleted => children.All(child => child.IsCompleted);

    public bool BlocksInput => children.Any(child => child.BlocksInput);

    public IReadOnlyCollection<AnimationBinding> ActiveBindings => children
        .Where(child => !child.IsCompleted)
        .SelectMany(child => child.ActiveBindings)
        .Distinct()
        .ToArray();

    public void Update(float deltaTime)
    {
        if (deltaTime < 0f || IsCompleted)
        {
            return;
        }

        foreach (var child in children)
        {
            child.Update(deltaTime);
        }
    }
}
