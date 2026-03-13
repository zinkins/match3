using Match3.Presentation.Animation.Engine;

namespace Match3.Presentation.Animation;

public sealed class TurnAnimationBuilder : ITurnAnimationBuilder
{
    public IAnimation Build(TurnAnimationContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        var sequence = Anim.Sequence();

        AppendPhase(sequence, context.QueueSwapAnimation, context.SwapDurationSeconds);

        if (!context.IsSwapApplied)
        {
            return sequence;
        }

        AppendPhase(sequence, context.QueueResolveAnimation, context.ResolveDurationSeconds);
        AppendPhase(sequence, context.QueueGravityAnimation, context.GravityDurationSeconds);
        AppendPhase(sequence, context.QueueSpawnAnimation, context.SpawnDurationSeconds);
        AppendPhase(sequence, context.QueueSettleAnimation, context.SettleDurationSeconds);

        return sequence;
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
