using Match3.Presentation.Animation.Engine;

namespace Match3.Presentation.Animation;

public sealed class TurnAnimationBuilder : ITurnAnimationBuilder
{
    public IAnimation Build(TurnAnimationContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        var sequence = Anim.Sequence()
            .Append(new CallbackAnimation(context.QueueVisualEffects))
            .Append(new CallbackAnimation(context.QueueSwapAnimation))
            .Append(new DelayAnimation(context.SwapDurationSeconds, blocksInput: true));

        if (!context.IsSwapApplied)
        {
            return sequence;
        }

        return sequence
            .Append(new CallbackAnimation(context.QueueCreatedBonusAnimation))
            .Append(new CallbackAnimation(context.QueueBoardSettleAnimation))
            .Append(new DelayAnimation(context.SettleDelaySeconds + context.SettleDurationSeconds, blocksInput: true));
    }
}
