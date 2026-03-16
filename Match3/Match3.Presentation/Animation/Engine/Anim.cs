using System.Numerics;

namespace Match3.Presentation.Animation.Engine;

public static class Anim
{
    public static MoveTween MoveTo(IAnimatableNode node, Vector2 to, float durationSeconds, bool blocksInput = false)
    {
        ArgumentNullException.ThrowIfNull(node);
        return new MoveTween(node, to, durationSeconds, blocksInput);
    }

    public static ScaleTween ScaleTo(IAnimatableNode node, Vector2 to, float durationSeconds, bool blocksInput = false)
    {
        ArgumentNullException.ThrowIfNull(node);
        return new ScaleTween(node, to, durationSeconds, blocksInput);
    }

    public static RotateTween RotateTo(IAnimatableNode node, float to, float durationSeconds, bool blocksInput = false)
    {
        ArgumentNullException.ThrowIfNull(node);
        return new RotateTween(node, to, durationSeconds, blocksInput);
    }

    public static FadeTween FadeTo(IAnimatableNode node, float to, float durationSeconds, bool blocksInput = false)
    {
        ArgumentNullException.ThrowIfNull(node);
        return new FadeTween(node, to, durationSeconds, blocksInput);
    }

    public static SequenceAnimation Sequence()
    {
        return new SequenceAnimation();
    }

    public static ParallelAnimation Parallel(params IAnimation[] animations)
    {
        return new ParallelAnimation(animations);
    }
}

