using Match3.Core.GameCore.Board;
using Match3.Core.GameFlow.Sessions;

namespace Match3.Tests;

public class Phase8ScoreAndTimerTests
{
    [Fact]
    public void ScoreCalculator_AddsPointsPerDestroyedPiece()
    {
        var calculator = new ScoreCalculator();

        var score = calculator.AddScore(currentScore: 100, destroyedPieces: 4);

        Assert.Equal(140, score);
    }

    [Fact]
    public void GameSession_InitializesTimerToSixtySeconds()
    {
        var session = new GameSession();

        Assert.Equal(TimeSpan.FromSeconds(60), session.RemainingTime);
    }

    [Fact]
    public void GameSession_DecreasesTimer()
    {
        var session = new GameSession();

        session.UpdateTimer(TimeSpan.FromSeconds(7));

        Assert.Equal(TimeSpan.FromSeconds(53), session.RemainingTime);
    }

    [Fact]
    public void GameSession_BecomesGameOver_WhenTimerExpires()
    {
        var session = new GameSession();

        session.UpdateTimer(TimeSpan.FromSeconds(60));

        Assert.True(session.IsGameOver);
    }

    [Fact]
    public void GameSession_BlocksNewInput_AfterTimerExpires()
    {
        var session = new GameSession();

        session.UpdateTimer(TimeSpan.FromSeconds(61));

        Assert.False(session.CanAcceptInput);
    }
}
