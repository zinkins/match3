using System;
using Match3.Core.GameCore.Board;
using Match3.Core.GameCore.ValueObjects;
using Match3.Core.GameFlow.Sessions;
using Match3.Core.GameFlow.StateMachine;

namespace Match3.Core.GameFlow.Pipeline;

public sealed class TurnProcessor
{
    private readonly MatchFinder matchFinder;
    private readonly GravityResolver gravityResolver;
    private readonly RefillResolver refillResolver;

    public TurnProcessor()
        : this(new MatchFinder(), new GravityResolver(), new RefillResolver())
    {
    }

    public TurnProcessor(MatchFinder matchFinder, GravityResolver gravityResolver, RefillResolver refillResolver)
    {
        this.matchFinder = matchFinder ?? throw new ArgumentNullException(nameof(matchFinder));
        this.gravityResolver = gravityResolver ?? throw new ArgumentNullException(nameof(gravityResolver));
        this.refillResolver = refillResolver ?? throw new ArgumentNullException(nameof(refillResolver));
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
        stateMachine.TransitionToSelecting();
        stateMachine.TransitionToSwapping();

        var applied = TryProcessMove(board, move);
        stateMachine.TransitionToResolving();

        if (session.IsGameOver)
        {
            stateMachine.TransitionToCheckingEndGame();
            stateMachine.TransitionToGameOver();
            return applied;
        }

        stateMachine.TransitionToApplyingGravity();
        gravityResolver.Apply(board);

        if (session.IsGameOver)
        {
            stateMachine.TransitionToCheckingEndGame();
            stateMachine.TransitionToGameOver();
            return applied;
        }

        stateMachine.TransitionToRefilling();
        refillResolver.Refill(board);

        stateMachine.TransitionToCheckingEndGame();
        if (session.IsGameOver)
        {
            stateMachine.TransitionToGameOver();
        }
        else
        {
            stateMachine.TransitionToIdle();
        }

        return applied;
    }

    private static void Swap(BoardState board, Move move)
    {
        var from = board.GetCell(move.From);
        var to = board.GetCell(move.To);
        board.SetCell(move.From, to);
        board.SetCell(move.To, from);
    }
}
