using System;
using Match3.Core.GameCore.Board;
using Match3.Core.GameCore.ValueObjects;
using Match3.Core.GameFlow.Pipeline;
using Match3.Core.GameFlow.Sessions;
using Match3.Core.GameFlow.StateMachine;
namespace Match3.Presentation;

public sealed class GameplayPresenter
{
    private readonly TurnProcessor turnProcessor;
    private readonly GameplayStateMachine stateMachine;
    private readonly GameSession session;

    public GameplayPresenter(
        TurnProcessor turnProcessor,
        GameplayStateMachine stateMachine,
        GameSession session)
    {
        this.turnProcessor = turnProcessor ?? throw new ArgumentNullException(nameof(turnProcessor));
        this.stateMachine = stateMachine ?? throw new ArgumentNullException(nameof(stateMachine));
        this.session = session ?? throw new ArgumentNullException(nameof(session));
    }

    public int Score { get; private set; }

    public TimeSpan RemainingTime => session.RemainingTime;

    public bool IsGameOver => session.IsGameOver;

    public bool CanAcceptInput => session.CanAcceptInput;

    public void Update(TimeSpan elapsed)
    {
        session.UpdateTimer(elapsed);
    }

    public TurnPipelineResult ProcessMove(BoardState board, Move move)
    {
        var result = turnProcessor.ProcessTurnPipelineWithEvents(
            board,
            move,
            session,
            stateMachine,
            currentScore: Score);

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
