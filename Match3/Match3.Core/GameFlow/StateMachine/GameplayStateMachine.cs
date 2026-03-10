using Match3.Core.GameFlow.Sessions;

namespace Match3.Core.GameFlow.StateMachine;

public sealed class GameplayStateMachine
{
    public GameplayState State { get; private set; } = GameplayState.Idle;

    public void TransitionToSelecting() => State = GameplayState.Selecting;

    public void TransitionToSwapping() => State = GameplayState.Swapping;

    public void TransitionToResolving() => State = GameplayState.Resolving;

    public void TransitionToApplyingGravity() => State = GameplayState.ApplyingGravity;

    public void TransitionToRefilling() => State = GameplayState.Refilling;

    public void TransitionToCheckingEndGame() => State = GameplayState.CheckingEndGame;

    public void TransitionToGameOver() => State = GameplayState.GameOver;

    public void TransitionToIdle() => State = GameplayState.Idle;

    public void AdvanceAfterPhase(GameSession session)
    {
        if (session.IsGameOver &&
            (State == GameplayState.Swapping ||
             State == GameplayState.Resolving ||
             State == GameplayState.ApplyingGravity ||
             State == GameplayState.Refilling))
        {
            TransitionToCheckingEndGame();
            return;
        }

        State = State switch
        {
            GameplayState.Idle => GameplayState.Selecting,
            GameplayState.Selecting => GameplayState.Swapping,
            GameplayState.Swapping => GameplayState.Resolving,
            GameplayState.Resolving => GameplayState.ApplyingGravity,
            GameplayState.ApplyingGravity => GameplayState.Refilling,
            GameplayState.Refilling => GameplayState.CheckingEndGame,
            GameplayState.CheckingEndGame => session.IsGameOver ? GameplayState.GameOver : GameplayState.Idle,
            _ => State
        };
    }
}
