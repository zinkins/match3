using System.Collections.Generic;
using Match3.Core.GameFlow.Events;

namespace Match3.Core.GameFlow.Pipeline;

public sealed class TurnPipelineResult
{
    public TurnPipelineResult(bool isSwapApplied, IReadOnlyList<IDomainEvent> events)
    {
        IsSwapApplied = isSwapApplied;
        Events = events;
    }

    public bool IsSwapApplied { get; }

    public IReadOnlyList<IDomainEvent> Events { get; }
}
