using Match3.Core.GameCore.ValueObjects;

namespace Match3.Core.GameFlow.Events;

public sealed record PiecesSwapped(Move Move) : IDomainEvent;

public sealed record SwapReverted(Move Move) : IDomainEvent;

public sealed record MatchResolved(int DestroyedPieces) : IDomainEvent;

public sealed record PiecesFell : IDomainEvent;

public sealed record PiecesSpawned : IDomainEvent;

public sealed record ScoreAdded(int Points) : IDomainEvent;

public sealed record GameEnded : IDomainEvent;
