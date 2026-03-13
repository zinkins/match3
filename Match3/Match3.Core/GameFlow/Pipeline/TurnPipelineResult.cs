using System.Collections.Generic;
using Match3.Core.GameFlow.Events;

namespace Match3.Core.GameFlow.Pipeline;

public sealed class TurnPipelineResult
{
    public TurnPipelineResult(bool isSwapApplied, IReadOnlyList<IDomainEvent> events, IReadOnlyList<TurnPipelineCascadeStep> cascadeSteps = null)
    {
        IsSwapApplied = isSwapApplied;
        Events = events;
        CascadeSteps = cascadeSteps ?? [];
    }

    public bool IsSwapApplied { get; }

    public IReadOnlyList<IDomainEvent> Events { get; }

    public IReadOnlyList<TurnPipelineCascadeStep> CascadeSteps { get; }
}
