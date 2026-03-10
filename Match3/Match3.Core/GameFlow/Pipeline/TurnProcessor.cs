using System;
using System.Collections.Generic;
using System.Linq;
using Match3.Core.GameCore.Board;
using Match3.Core.GameCore.ValueObjects;
using Match3.Core.GameFlow.Events;
using Match3.Core.GameFlow.Sessions;
using Match3.Core.GameFlow.StateMachine;

namespace Match3.Core.GameFlow.Pipeline;

public sealed class TurnProcessor
{
    private readonly MatchFinder matchFinder;
    private readonly GravityResolver gravityResolver;
    private readonly RefillResolver refillResolver;
    private readonly ScoreCalculator scoreCalculator;

    public TurnProcessor()
        : this(new MatchFinder(), new GravityResolver(), new RefillResolver(), new ScoreCalculator())
    {
    }

    public TurnProcessor(MatchFinder matchFinder, GravityResolver gravityResolver, RefillResolver refillResolver)
        : this(matchFinder, gravityResolver, refillResolver, new ScoreCalculator())
    {
    }

    public TurnProcessor(
        MatchFinder matchFinder,
        GravityResolver gravityResolver,
        RefillResolver refillResolver,
        ScoreCalculator scoreCalculator)
    {
        this.matchFinder = matchFinder ?? throw new ArgumentNullException(nameof(matchFinder));
        this.gravityResolver = gravityResolver ?? throw new ArgumentNullException(nameof(gravityResolver));
        this.refillResolver = refillResolver ?? throw new ArgumentNullException(nameof(refillResolver));
        this.scoreCalculator = scoreCalculator ?? throw new ArgumentNullException(nameof(scoreCalculator));
    }

    public bool TryProcessMove(BoardState board, Move move)
    {
        Swap(board, move);

        var matches = matchFinder.FindMatches(board);
        if (matches.Count > 0)
        {
            return true;
        }

        Swap(board, move);
        return false;
    }

    public bool RecheckAfterGravityAndRefill(BoardState board)
    {
        gravityResolver.Apply(board);
        refillResolver.Refill(board);
        return matchFinder.FindMatches(board).Count > 0;
    }

    public void ExecuteAtomicResolvingStep(GameSession session, GameplayStateMachine stateMachine, Action resolveAction)
    {
        stateMachine.TransitionToResolving();
        resolveAction();
        stateMachine.AdvanceAfterPhase(session);
    }

    public bool ProcessTurnPipeline(BoardState board, Move move, GameSession session, GameplayStateMachine stateMachine)
    {
        return ProcessTurnPipelineWithEvents(board, move, session, stateMachine).IsSwapApplied;
    }

    public TurnPipelineResult ProcessTurnPipelineWithEvents(
        BoardState board,
        Move move,
        GameSession session,
        GameplayStateMachine stateMachine,
        int currentScore = 0,
        Action<GameplayState, GameSession> onPhaseCompleted = null)
    {
        var events = new List<IDomainEvent>();

        stateMachine.TransitionToSelecting();
        onPhaseCompleted?.Invoke(stateMachine.State, session);

        stateMachine.TransitionToSwapping();
        onPhaseCompleted?.Invoke(stateMachine.State, session);

        Swap(board, move);
        events.Add(new PiecesSwapped(move));

        var matches = matchFinder.FindMatches(board);
        var applied = matches.Count > 0;
        if (!applied)
        {
            Swap(board, move);
            events.Add(new SwapReverted(move));
        }

        stateMachine.TransitionToResolving();
        onPhaseCompleted?.Invoke(stateMachine.State, session);

        if (applied)
        {
            var destroyedPieces = matches
                .SelectMany(group => group.Positions)
                .Distinct()
                .Count();

            events.Add(new MatchResolved(destroyedPieces));
            var updatedScore = scoreCalculator.AddScore(currentScore, destroyedPieces);
            events.Add(new ScoreAdded(updatedScore - currentScore));
        }

        if (session.IsGameOver)
        {
            return FinishWithGameOver(stateMachine, events, applied);
        }

        stateMachine.TransitionToApplyingGravity();
        gravityResolver.Apply(board);
        events.Add(new PiecesFell());
        onPhaseCompleted?.Invoke(stateMachine.State, session);

        if (session.IsGameOver)
        {
            return FinishWithGameOver(stateMachine, events, applied);
        }

        stateMachine.TransitionToRefilling();
        refillResolver.Refill(board);
        events.Add(new PiecesSpawned());
        onPhaseCompleted?.Invoke(stateMachine.State, session);

        stateMachine.TransitionToCheckingEndGame();
        if (session.IsGameOver)
        {
            return FinishWithGameOver(stateMachine, events, applied);
        }

        stateMachine.TransitionToIdle();
        return new TurnPipelineResult(applied, events);
    }

    private static TurnPipelineResult FinishWithGameOver(
        GameplayStateMachine stateMachine,
        List<IDomainEvent> events,
        bool applied)
    {
        stateMachine.TransitionToCheckingEndGame();
        stateMachine.TransitionToGameOver();
        events.Add(new GameEnded());
        return new TurnPipelineResult(applied, events);
    }

    private static void Swap(BoardState board, Move move)
    {
        var from = board.GetCell(move.From);
        var to = board.GetCell(move.To);
        board.SetCell(move.From, to);
        board.SetCell(move.To, from);
    }
}
