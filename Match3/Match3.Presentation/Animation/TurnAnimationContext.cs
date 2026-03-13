namespace Match3.Presentation.Animation;

public sealed class TurnAnimationContext
{
    public required bool IsSwapApplied { get; init; }

    public required Action QueueSwapAnimation { get; init; }

    public float SwapDurationSeconds { get; init; } = GameplayEffectTimings.SwapAcceptedSeconds;

    public IReadOnlyList<TurnAnimationCascadeStep> CascadeSteps { get; init; } = [];
}

