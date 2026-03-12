namespace Match3.Presentation.Animation.Engine;

public sealed class SequenceAnimation : IAnimation
{
    private readonly List<List<IAnimation>> steps = [];
    private int currentStepIndex;

    public bool IsCompleted => currentStepIndex >= steps.Count;

    public bool BlocksInput => GetCurrentStep().Any(animation => animation.BlocksInput);

    public IReadOnlyCollection<AnimationBinding> ActiveBindings => GetCurrentStep()
        .Where(animation => !animation.IsCompleted)
        .SelectMany(animation => animation.ActiveBindings)
        .Distinct()
        .ToArray();

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
        if (deltaTime < 0f || IsCompleted)
        {
            return;
        }

        while (!IsCompleted)
        {
            var step = GetCurrentStep();
            foreach (var animation in step.Where(animation => !animation.IsCompleted))
            {
                animation.Update(deltaTime);
            }

            if (step.Any(animation => !animation.IsCompleted))
            {
                return;
            }

            currentStepIndex++;
        }
    }

    private IReadOnlyList<IAnimation> GetCurrentStep()
    {
        return IsCompleted ? [] : steps[currentStepIndex];
    }
}
