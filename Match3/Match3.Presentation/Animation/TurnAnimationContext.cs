namespace Match3.Presentation.Animation;

public sealed class TurnAnimationContext
{
    public required bool IsSwapApplied { get; init; }

    public required Action QueueVisualEffects { get; init; }

    public required Action QueueSwapAnimation { get; init; }

    public required Action QueueCreatedBonusAnimation { get; init; }

    public required Action QueueBoardSettleAnimation { get; init; }

    public float SwapDurationSeconds { get; init; } = 0.22f;

    public float SettleDelaySeconds { get; init; }

    public float SettleDurationSeconds { get; init; } = 1.15f;
}

