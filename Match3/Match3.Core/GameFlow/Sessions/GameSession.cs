using System;

namespace Match3.Core.GameFlow.Sessions;

public sealed class GameSession
{
    public int Score { get; private set; }

    public TimeSpan RemainingTime { get; private set; } = TimeSpan.FromSeconds(60);

    public bool IsGameOver => RemainingTime <= TimeSpan.Zero;

    public bool CanAcceptInput => !IsGameOver;

    public void UpdateTimer(TimeSpan elapsed)
    {
        if (elapsed <= TimeSpan.Zero || IsGameOver)
        {
            return;
        }

        var updated = RemainingTime - elapsed;
        RemainingTime = updated > TimeSpan.Zero ? updated : TimeSpan.Zero;
    }

    public void AddScore(int points)
    {
        if (points <= 0)
        {
            return;
        }

        Score += points;
    }
}
