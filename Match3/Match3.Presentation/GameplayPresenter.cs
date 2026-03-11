using System;
using Match3.Core.GameCore.Board;
using Match3.Core.GameCore.ValueObjects;
using Match3.Core.GameFlow.Pipeline;
using Match3.Core.GameFlow.Sessions;
using Match3.Core.GameFlow.StateMachine;
using Match3.Presentation.Animation;

namespace Match3.Presentation;

public sealed class GameplayPresenter
{
    private readonly TurnProcessor turnProcessor;
    private readonly GameplayStateMachine stateMachine;
    private readonly GameSession session;

    public GameplayPresenter(
        TurnProcessor turnProcessor,
        GameplayStateMachine stateMachine,
        GameSession session,
        AnimationQueue animationQueue)
    {
        this.turnProcessor = turnProcessor ?? throw new ArgumentNullException(nameof(turnProcessor));
        this.stateMachine = stateMachine ?? throw new ArgumentNullException(nameof(stateMachine));
        this.session = session ?? throw new ArgumentNullException(nameof(session));
        AnimationQueue = animationQueue ?? throw new ArgumentNullException(nameof(animationQueue));
    }

    public AnimationQueue AnimationQueue { get; }

    public int Score { get; private set; }

    public TimeSpan RemainingTime => session.RemainingTime;

    public bool ShouldShowGameOverOverlay => session.IsGameOver && !AnimationQueue.HasRunningAnimations;

    public void Update(TimeSpan elapsed)
    {
        session.UpdateTimer(elapsed);

        if (AnimationQueue.HasRunningAnimations)
        {
            AnimationQueue.Update((float)elapsed.TotalSeconds);
        }
    }

    public TurnPipelineResult ProcessMove(BoardState board, Move move)
    {
        var result = turnProcessor.ProcessTurnPipelineWithEvents(
            board,
            move,
            session,
            stateMachine,
            currentScore: Score);

        AnimationQueue.Enqueue(result.Events);

        foreach (var domainEvent in result.Events)
        {
            if (domainEvent is Core.GameFlow.Events.ScoreAdded scoreAdded)
            {
                Score += scoreAdded.Points;
            }
        }

        return result;
    }
}
