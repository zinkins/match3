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

    /// <summary>
    /// Advances session time and updates game-over state derived from the timer.
    /// </summary>
    /// <param name="elapsed">Elapsed frame time.</param>
    public void Update(TimeSpan elapsed)
    {
        session.UpdateTimer(elapsed);
    }

    /// <summary>
    /// Processes a player move through the full gameplay pipeline and accumulates score changes emitted by domain events.
    /// </summary>
    /// <param name="board">Mutable board state that will be updated in place.</param>
    /// <param name="move">Player move to execute.</param>
    /// <returns>The pipeline result for the processed move.</returns>
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
