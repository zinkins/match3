namespace Match3.Presentation.Animation.Engine;

public sealed class ParallelAnimation(params IAnimation[] children) : ITimedAnimation
{
    private readonly List<IAnimation> children = [.. children.Where(child => child is not null)];

    public bool IsCompleted => children.All(child => child.IsCompleted);

    public bool BlocksInput => children.Any(child => child.BlocksInput);

    public IReadOnlyCollection<AnimationBinding> ActiveBindings
    {
        get
        {
            var bindings = new List<AnimationBinding>();
            foreach (var child in children)
            {
                if (child.IsCompleted)
                {
                    continue;
                }

                bindings.AddRange(child.ActiveBindings);
            }

            return bindings;
        }
    }

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

        var unusedTime = deltaTime;
        foreach (var child in children)
        {
            var childUnused = child is ITimedAnimation timedChild
                ? timedChild.Advance(deltaTime)
                : AdvanceUntimedChild(child, deltaTime);
            unusedTime = MathF.Min(unusedTime, childUnused);
        }

        return IsCompleted ? unusedTime : 0f;
    }

    private static float AdvanceUntimedChild(IAnimation child, float deltaTime)
    {
        child.Update(deltaTime);
        return child.IsCompleted ? 0f : 0f;
    }
}
