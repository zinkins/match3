using System.Runtime.CompilerServices;

namespace Match3.Presentation.Animation.Engine;

public readonly struct AnimationBinding : IEquatable<AnimationBinding>
{
    public AnimationBinding(object target, AnimationChannel channel)
    {
        Target = target ?? throw new ArgumentNullException(nameof(target));
        Channel = channel;
    }

    public object Target { get; }

    public AnimationChannel Channel { get; }

    public bool Equals(AnimationBinding other)
    {
        return ReferenceEquals(Target, other.Target) && Channel == other.Channel;
    }

    public override bool Equals(object? obj)
    {
        return obj is AnimationBinding other && Equals(other);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(RuntimeHelpers.GetHashCode(Target), (int)Channel);
    }

    public static bool operator ==(AnimationBinding left, AnimationBinding right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(AnimationBinding left, AnimationBinding right)
    {
        return !left.Equals(right);
    }
}
