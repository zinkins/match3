using System.Collections.Generic;
using Match3.Core.GameFlow.Events;

namespace Match3.Presentation.Animation;

public sealed class AnimationQueue
{
    private readonly Queue<AnimationStep> pending = new();

    public bool HasRunningAnimations => pending.Count > 0;

    public void Enqueue(IReadOnlyList<IDomainEvent> events)
    {
        foreach (var domainEvent in events)
        {
            pending.Enqueue(new AnimationStep(domainEvent.GetType().Name));
        }
    }

    public AnimationStep? Dequeue()
    {
        return pending.Count == 0 ? null : pending.Dequeue();
    }
}
