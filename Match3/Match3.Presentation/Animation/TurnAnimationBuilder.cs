using Match3.Presentation.Animation.Engine;

namespace Match3.Presentation.Animation;

public sealed class TurnAnimationBuilder : ITurnAnimationBuilder
{
    public IAnimation Build(TurnAnimationContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        var sequence = Anim.Sequence();

        AppendSwapPhase(sequence, context);

        if (!context.IsSwapApplied)
        {
            return sequence;
        }

        AppendResolvePhase(sequence, context);
        AppendGravityPhase(sequence, context);
        AppendSpawnPhase(sequence, context);
        AppendSettlePhase(sequence, context);

        return sequence;
    }

    private static void AppendSwapPhase(SequenceAnimation sequence, TurnAnimationContext context)
    {
        AppendPhase(sequence, context.QueueSwapAnimation, context.SwapDurationSeconds);
    }

    private static void AppendResolvePhase(SequenceAnimation sequence, TurnAnimationContext context)
    {
        AppendPhase(sequence, context.QueueResolveAnimation, context.ResolveDurationSeconds);
    }

    private static void AppendGravityPhase(SequenceAnimation sequence, TurnAnimationContext context)
    {
        AppendPhase(sequence, context.QueueGravityAnimation, context.GravityDurationSeconds);
    }

    private static void AppendSpawnPhase(SequenceAnimation sequence, TurnAnimationContext context)
    {
        AppendPhase(sequence, context.QueueSpawnAnimation, context.SpawnDurationSeconds);
    }

    private static void AppendSettlePhase(SequenceAnimation sequence, TurnAnimationContext context)
    {
        AppendPhase(sequence, context.QueueSettleAnimation, context.SettleDurationSeconds);
    }

    private static void AppendPhase(SequenceAnimation sequence, Action action, float durationSeconds)
    {
        sequence.Append(new CallbackAnimation(action));

        if (durationSeconds > 0f)
        {
            sequence.Append(new DelayAnimation(durationSeconds, blocksInput: true));
        }
    }
}
