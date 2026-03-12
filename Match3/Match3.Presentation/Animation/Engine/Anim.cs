using System.Numerics;

namespace Match3.Presentation.Animation.Engine;

public static class Anim
{
    public static PropertyTween<Vector2> MoveTo(IAnimatableNode node, Vector2 to, float durationSeconds, bool blocksInput = false)
    {
        ArgumentNullException.ThrowIfNull(node);

        return new PropertyTween<Vector2>(
            node,
            AnimationChannel.Position,
            () => node.Position,
            value => node.Position = value,
            node.Position,
            to,
            durationSeconds,
            static (from, target, progress) => Vector2.Lerp(from, target, progress),
            blocksInput);
    }

    public static PropertyTween<Vector2> ScaleTo(IAnimatableNode node, Vector2 to, float durationSeconds, bool blocksInput = false)
    {
        ArgumentNullException.ThrowIfNull(node);

        return new PropertyTween<Vector2>(
            node,
            AnimationChannel.Scale,
            () => node.Scale,
            value => node.Scale = value,
            node.Scale,
            to,
            durationSeconds,
            static (from, target, progress) => Vector2.Lerp(from, target, progress),
            blocksInput);
    }

    public static PropertyTween<float> RotateTo(IAnimatableNode node, float to, float durationSeconds, bool blocksInput = false)
    {
        ArgumentNullException.ThrowIfNull(node);

        return new PropertyTween<float>(
            node,
            AnimationChannel.Rotation,
            () => node.Rotation,
            value => node.Rotation = value,
            node.Rotation,
            to,
            durationSeconds,
            static (from, target, progress) => from + ((target - from) * progress),
            blocksInput);
    }
    public static PropertyTween<float> FadeTo(IAnimatableNode node, float to, float durationSeconds, bool blocksInput = false)
    {
        ArgumentNullException.ThrowIfNull(node);

        return new PropertyTween<float>(
            node,
            AnimationChannel.Opacity,
            () => node.Opacity,
            value => node.Opacity = value,
            node.Opacity,
            to,
            durationSeconds,
            static (from, target, progress) => from + ((target - from) * progress),
            blocksInput);
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

