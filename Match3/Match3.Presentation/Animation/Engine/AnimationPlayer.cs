namespace Match3.Presentation.Animation.Engine;

public sealed class AnimationPlayer
{
    private readonly List<IAnimation> activeAnimations = [];
    private readonly Dictionary<AnimationBinding, IAnimation> reservedBindings = [];

    public bool HasActiveAnimations => activeAnimations.Count > 0;

    public bool HasBlockingAnimations => activeAnimations.Any(animation => animation.BlocksInput);

    public IReadOnlyList<IAnimation> ActiveAnimations => activeAnimations;

    public AnimationHandle Play(IAnimation animation, ChannelConflictPolicy conflictPolicy = ChannelConflictPolicy.Reject)
    {
        ArgumentNullException.ThrowIfNull(animation);

        if (!TryReserveBindings(animation, conflictPolicy))
        {
            return new AnimationHandle(isAccepted: false, animation: null);
        }

        activeAnimations.Add(animation);
        return new AnimationHandle(isAccepted: true, animation);
    }

    public void Update(float deltaTime)
    {
        if (deltaTime < 0f)
        {
            return;
        }

        for (var i = activeAnimations.Count - 1; i >= 0; i--)
        {
            var animation = activeAnimations[i];
            animation.Update(deltaTime);
            if (!animation.IsCompleted)
            {
                continue;
            }

            ReleaseBindings(animation);
            activeAnimations.RemoveAt(i);
        }
    }

    private bool TryReserveBindings(IAnimation animation, ChannelConflictPolicy conflictPolicy)
    {
        foreach (var binding in animation.ActiveBindings)
        {
            if (!reservedBindings.TryGetValue(binding, out var existing))
            {
                continue;
            }

            if (conflictPolicy == ChannelConflictPolicy.Replace)
            {
                ReleaseBindings(existing);
                activeAnimations.Remove(existing);
                continue;
            }

            return false;
        }

        foreach (var binding in animation.ActiveBindings)
        {
            reservedBindings[binding] = animation;
        }

        return true;
    }

    private void ReleaseBindings(IAnimation animation)
    {
        foreach (var binding in animation.ActiveBindings)
        {
            if (reservedBindings.TryGetValue(binding, out var owner) && ReferenceEquals(owner, animation))
            {
                reservedBindings.Remove(binding);
            }
        }
    }
}
