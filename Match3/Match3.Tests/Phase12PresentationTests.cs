using Match3.Core.GameCore.Board;
using Match3.Core.GameCore.Pieces;
using Match3.Core.GameCore.ValueObjects;
using Match3.Core.GameFlow.Sessions;
using Match3.Presentation;
using Match3.Presentation.Animation;
using Match3.Presentation.Rendering;
using Match3.Presentation.Screens;

namespace Match3.Tests;

public class Phase12PresentationTests
{
    [Fact]
    public void MainMenu_PlayButton_StartsGameSession()
    {
        var flow = new ScreenFlowController();

        flow.MainMenu.PlayButton.Click();

        Assert.Same(flow.Gameplay, flow.CurrentScreen);
    }

    [Fact]
    public void GameOver_OkButton_ReturnsToMainMenu()
    {
        var flow = new ScreenFlowController();

        flow.MainMenu.PlayButton.Click();
        flow.GameOver.OkButton.Click();

        Assert.Same(flow.MainMenu, flow.CurrentScreen);
    }

    [Fact]
    public void GameOverScreen_HasMessageAndSingleOkButton()
    {
        var screen = new GameOverScreen();

        Assert.Equal("Game Over", screen.Message);
        Assert.Equal("Ok", screen.OkButton.Label);
    }

    [Fact]
    public void GameplayPresenter_EnqueuesAnimations_FromDomainEvents()
    {
        var session = new GameSession();
        var presenter = new GameplayPresenter(
            turnProcessor: new Match3.Core.GameFlow.Pipeline.TurnProcessor(),
            stateMachine: new Match3.Core.GameFlow.StateMachine.GameplayStateMachine(),
            session: session,
            animationQueue: new AnimationQueue());

        presenter.ProcessMove(CreateBoardForSwapWithMatch(), new Move(new GridPosition(0, 2), new GridPosition(1, 2)));

        Assert.True(presenter.AnimationQueue.HasRunningAnimations);
    }

    [Fact]
    public void HudRenderer_ShowsScoreAndRemainingTime()
    {
        var hud = new HudRenderer();

        Assert.Equal("Score: 120", hud.FormatScore(120));
        Assert.Equal("Time: 12", hud.FormatRemainingTime(TimeSpan.FromSeconds(11.2)));
    }

    [Fact]
    public void GameplayPresenter_ShowsGameOverOnlyAfterCurrentAnimationsFinish()
    {
        var session = new GameSession();
        session.UpdateTimer(TimeSpan.FromSeconds(60));
        var queue = new AnimationQueue();
        queue.Enqueue([new Match3.Core.GameFlow.Events.GameEnded()]);
        var presenter = new GameplayPresenter(
            turnProcessor: new Match3.Core.GameFlow.Pipeline.TurnProcessor(),
            stateMachine: new Match3.Core.GameFlow.StateMachine.GameplayStateMachine(),
            session: session,
            animationQueue: queue);

        Assert.False(presenter.ShouldShowGameOverOverlay);

        presenter.AnimationQueue.Dequeue();
        Assert.True(presenter.ShouldShowGameOverOverlay);
    }

    private static BoardState CreateBoardForSwapWithMatch()
    {
        var board = new BoardState();
        var types = PieceCatalog.All;
        for (var row = 0; row < board.Height; row++)
        {
            for (var column = 0; column < board.Width; column++)
            {
                board.SetCell(new GridPosition(row, column), types[(row + column) % types.Count]);
            }
        }

        board.SetCell(new GridPosition(0, 0), PieceType.Red);
        board.SetCell(new GridPosition(0, 1), PieceType.Red);
        board.SetCell(new GridPosition(0, 2), PieceType.Blue);
        board.SetCell(new GridPosition(1, 2), PieceType.Red);
        return board;
    }
}
