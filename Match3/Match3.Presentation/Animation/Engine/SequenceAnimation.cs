namespace Match3.Presentation.Animation.Engine;

public sealed class SequenceAnimation : ITimedAnimation
{
    private readonly List<List<IAnimation>> steps = [];
    private int currentStepIndex;

    public bool IsCompleted => currentStepIndex >= steps.Count;

    public bool BlocksInput => GetCurrentStep().Any(animation => animation.BlocksInput);

    public IReadOnlyCollection<AnimationBinding> ActiveBindings
    {
        get
        {
            var bindings = new List<AnimationBinding>();
            foreach (var animation in GetCurrentStep())
            {
                if (animation.IsCompleted)
                {
                    continue;
                }

                bindings.AddRange(animation.ActiveBindings);
            }

            return bindings;
        }
    }

    public SequenceAnimation Append(IAnimation animation)
    {
        ArgumentNullException.ThrowIfNull(animation);
        steps.Add([animation]);
        return this;
    }

    public SequenceAnimation Join(IAnimation animation)
    {
        ArgumentNullException.ThrowIfNull(animation);
        if (steps.Count == 0)
        {
            steps.Add([]);
        }

        steps[^1].Add(animation);
        return this;
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

        var remainingTime = deltaTime;
        while (!IsCompleted)
        {
            var step = GetCurrentStep();
            var stepUnused = remainingTime;
            foreach (var animation in step.Where(animation => !animation.IsCompleted))
            {
                var childUnused = animation is ITimedAnimation timedAnimation
                    ? timedAnimation.Advance(remainingTime)
                    : AdvanceUntimedChild(animation, remainingTime);
                stepUnused = MathF.Min(stepUnused, childUnused);
            }

            if (step.Any(animation => !animation.IsCompleted))
            {
                return 0f;
            }

            currentStepIndex++;
            remainingTime = stepUnused;
        }

        return remainingTime;
    }

    private IReadOnlyList<IAnimation> GetCurrentStep()
    {
        return IsCompleted ? [] : steps[currentStepIndex];
    }

    private static float AdvanceUntimedChild(IAnimation animation, float deltaTime)
    {
        animation.Update(deltaTime);
        return 0f;
    }
}
