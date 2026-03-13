using System;
using System.Collections.Generic;
using Match3.Core.GameCore.Board;
using Match3.Core.GameFlow.Events;

namespace Match3.Core.GameFlow.Pipeline;

public sealed class TurnPipelineCascadeStep
{
    public TurnPipelineCascadeStep(
        BoardState resolvedBoard,
        BoardState gravityBoard,
        BoardState endBoard,
        IReadOnlyList<IDomainEvent> events)
    {
        ResolvedBoard = resolvedBoard ?? throw new ArgumentNullException(nameof(resolvedBoard));
        GravityBoard = gravityBoard ?? throw new ArgumentNullException(nameof(gravityBoard));
        EndBoard = endBoard ?? throw new ArgumentNullException(nameof(endBoard));
        Events = events ?? throw new ArgumentNullException(nameof(events));
    }

    public BoardState ResolvedBoard { get; }

    public BoardState GravityBoard { get; }

    public BoardState EndBoard { get; }

    public IReadOnlyList<IDomainEvent> Events { get; }
}
