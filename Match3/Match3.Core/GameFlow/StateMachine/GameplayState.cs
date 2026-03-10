namespace Match3.Core.GameFlow.StateMachine;

public enum GameplayState
{
    Idle = 0,
    Selecting = 1,
    Swapping = 2,
    Resolving = 3,
    ApplyingGravity = 4,
    Refilling = 5,
    CheckingEndGame = 6,
    GameOver = 7,
}
