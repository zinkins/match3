namespace Match3.Presentation.Animation;

public sealed class TurnAnimationContext
{
    public required bool IsSwapApplied { get; init; }

    public required Action QueueResolveAnimation { get; init; }

    public required Action QueueSwapAnimation { get; init; }

    public required Action QueueGravityAnimation { get; init; }

    public required Action QueueSpawnAnimation { get; init; }

    public required Action QueueSettleAnimation { get; init; }

    public float SwapDurationSeconds { get; init; } = 0.22f;

    public float ResolveDurationSeconds { get; init; }

    public float GravityDurationSeconds { get; init; }

    public float SpawnDurationSeconds { get; init; }

    public float SettleDurationSeconds { get; init; } = 1.15f;
}

