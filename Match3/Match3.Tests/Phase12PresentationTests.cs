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
    public void Gameplay_OkButton_ReturnsToMainMenu_AfterGameOver()
    {
        var flow = new ScreenFlowController();

        flow.MainMenu.PlayButton.Click();
        flow.Gameplay.OkButton.Click();

        Assert.Same(flow.MainMenu, flow.CurrentScreen);
    }

    [Fact]
    public void GameplayScreen_HasGameOverMessageAndSingleOkButton()
    {
        var screen = new GameplayScreen(
            new GameplayPresenter(
                new Match3.Core.GameFlow.Pipeline.TurnProcessor(),
                new Match3.Core.GameFlow.StateMachine.GameplayStateMachine(),
                new GameSession(),
                new AnimationQueue()),
            new BoardState(),
            new Match3.Presentation.Input.BoardInputHandler(
                new BoardTransform(48f, new System.Numerics.Vector2(40f, 100f), 8, 8),
                new Match3.Core.GameFlow.Sessions.SelectionController()),
            new GameplayEffectsController(),
            new BoardRenderer(),
            new HudRenderer(),
            new BoardTransform(48f, new System.Numerics.Vector2(40f, 100f), 8, 8),
            () => { });

        Assert.Equal("Game Over", screen.GameOverMessage);
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

        presenter.Update(TimeSpan.FromMilliseconds(20));
        Assert.True(presenter.ShouldShowGameOverOverlay);
    }

    [Fact]
    public void AnimationQueue_AdvancesByElapsedTime_InRenderLoop()
    {
        var queue = new AnimationQueue();
        queue.Enqueue([new Match3.Core.GameFlow.Events.PiecesSwapped(new Move(new GridPosition(0, 0), new GridPosition(0, 1)))]);

        Assert.Equal(nameof(Match3.Core.GameFlow.Events.PiecesSwapped), queue.CurrentStep?.Name);

        queue.Update(0.1f);
        Assert.True(queue.HasRunningAnimations);

        queue.Update(0.2f);
        Assert.False(queue.HasRunningAnimations);
    }

    [Fact]
    public void GameplayPresenter_Update_DecreasesRemainingTime()
    {
        var presenter = new GameplayPresenter(
            turnProcessor: new Match3.Core.GameFlow.Pipeline.TurnProcessor(),
            stateMachine: new Match3.Core.GameFlow.StateMachine.GameplayStateMachine(),
            session: new GameSession(),
            animationQueue: new AnimationQueue());

        presenter.Update(TimeSpan.FromSeconds(1.5));

        Assert.Equal(TimeSpan.FromSeconds(58.5), presenter.RemainingTime);
    }

    private static BoardState CreateBoardForSwapWithMatch()
    {
        var board = new BoardState();
        var types = PieceCatalog.All;
        for (var row = 0; row < board.Height; row++)
        {
            for (var column = 0; column < board.Width; column++)
            {
                board.SetPiece(new GridPosition(row, column), types[(row + column) % types.Count]);
            }
        }

        board.SetPiece(new GridPosition(0, 0), PieceType.Red);
        board.SetPiece(new GridPosition(0, 1), PieceType.Red);
        board.SetPiece(new GridPosition(0, 2), PieceType.Blue);
        board.SetPiece(new GridPosition(1, 2), PieceType.Red);
        return board;
    }
}