using System;
using System.Collections.Generic;
using Match3.Core.GameCore.Board;
using Match3.Core.GameFlow.Events;

namespace Match3.Core.GameFlow.Pipeline;

public sealed class TurnPipelineCascadeStep
{
    public TurnPipelineCascadeStep(
        BoardState startBoard,
        BoardState endBoard,
        IReadOnlyList<IDomainEvent> events)
    {
        StartBoard = startBoard ?? throw new ArgumentNullException(nameof(startBoard));
        EndBoard = endBoard ?? throw new ArgumentNullException(nameof(endBoard));
        Events = events ?? throw new ArgumentNullException(nameof(events));
    }

    public BoardState StartBoard { get; }

    public BoardState EndBoard { get; }

    public IReadOnlyList<IDomainEvent> Events { get; }
}
