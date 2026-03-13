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

        foreach (var cascadeStep in context.CascadeSteps)
        {
            AppendResolvePhase(sequence, cascadeStep);
            AppendGravityPhase(sequence, cascadeStep);
            AppendSpawnPhase(sequence, cascadeStep);
            AppendSettlePhase(sequence, cascadeStep);
        }

        return sequence;
    }

    private static void AppendSwapPhase(SequenceAnimation sequence, TurnAnimationContext context)
    {
        AppendPhase(sequence, context.QueueSwapAnimation, context.SwapDurationSeconds);
    }

    private static void AppendResolvePhase(SequenceAnimation sequence, TurnAnimationCascadeStep cascadeStep)
    {
        AppendPhase(sequence, cascadeStep.QueueResolveAnimation, cascadeStep.ResolveDurationSeconds);
    }

    private static void AppendGravityPhase(SequenceAnimation sequence, TurnAnimationCascadeStep cascadeStep)
    {
        AppendPhase(sequence, cascadeStep.QueueGravityAnimation, cascadeStep.GravityDurationSeconds);
    }

    private static void AppendSpawnPhase(SequenceAnimation sequence, TurnAnimationCascadeStep cascadeStep)
    {
        AppendPhase(sequence, cascadeStep.QueueSpawnAnimation, cascadeStep.SpawnDurationSeconds);
    }

    private static void AppendSettlePhase(SequenceAnimation sequence, TurnAnimationCascadeStep cascadeStep)
    {
        AppendPhase(sequence, cascadeStep.QueueSettleAnimation, cascadeStep.SettleDurationSeconds);
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
