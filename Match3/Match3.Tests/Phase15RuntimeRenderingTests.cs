using Match3.Core.GameCore.Board;
using Match3.Core.GameCore.Pieces;
using Match3.Core.GameCore.ValueObjects;
using Match3.Core.Runtime;
using Match3.Presentation.Animation;
using Match3.Presentation.Input;
using Match3.Presentation.Rendering;
using Match3.Presentation.Screens;

namespace Match3.Tests;

public class Phase15RuntimeRenderingTests
{
    [Fact]
    public void BoardRenderer_ProducesRenderableBoardSnapshot()
    {
        var renderer = new BoardRenderer();
        var transform = new BoardTransform(32f, new System.Numerics.Vector2(10f, 20f));
        var board = CreateBoard();

        var snapshot = renderer.BuildSnapshot(board, transform);

        Assert.Equal(board.Width * board.Height, snapshot.Cells.Count);
        Assert.Equal(board.Width * board.Height, snapshot.Pieces.Count);
    }

    [Fact]
    public void HudRenderer_ProducesHudSnapshot()
    {
        var renderer = new HudRenderer();

        var snapshot = renderer.BuildSnapshot(120, TimeSpan.FromSeconds(14), 800f);

        Assert.Equal(2, snapshot.Labels.Count);
        Assert.Contains(snapshot.Labels, label => label.Text == "Score: 120");
        Assert.Contains(snapshot.Labels, label => label.Text == "Time: 14");
    }

    [Fact]
    public void PresentationScreenHost_DrawsCurrentScreen()
    {
        var flow = new ScreenFlowController();
        var host = new PresentationScreenHost(flow, new SpriteBatchRenderer());
        var canvas = new FakeCanvas();

        host.Draw(canvas);

        Assert.NotEmpty(canvas.Texts);
    }

    [Fact]
    public void MouseInputRouter_MapsLeftClickToBoardSelection()
    {
        var router = new MouseInputRouter();

        var shouldHandle = router.ShouldHandleBoardSelection(
            new InputState(true, new System.Numerics.Vector2(10f, 10f), true, false, 800, 480));

        Assert.True(shouldHandle);
    }

    [Fact]
    public void TouchInputRouter_MapsTapToBoardSelection()
    {
        var router = new TouchInputRouter();

        var shouldHandle = router.ShouldHandleBoardSelection(
            new InputState(true, new System.Numerics.Vector2(10f, 10f), true, false, 800, 480));

        Assert.True(shouldHandle);
    }

    [Fact]
    public void PresentationScreenHost_ClickingPlay_TransitionsToGameplay()
    {
        var flow = new ScreenFlowController();
        var host = new PresentationScreenHost(flow, new SpriteBatchRenderer());

        host.Update(
            TimeSpan.FromMilliseconds(16),
            new InputState(true, new System.Numerics.Vector2(300f, 150f), true, false, 800, 480));

        Assert.Same(flow.Gameplay, flow.CurrentScreen);
    }

    [Fact]
    public void PresentationScreenHost_ClickingOk_TransitionsToMainMenu()
    {
        var flow = new ScreenFlowController(CreateExpiredSession);
        var host = new PresentationScreenHost(flow, new SpriteBatchRenderer());

        flow.MainMenu.PlayButton.Click();
        flow.Tick();
        host.Update(
            TimeSpan.FromMilliseconds(16),
            new InputState(true, new System.Numerics.Vector2(300f, 180f), true, false, 800, 480));

        Assert.Same(flow.MainMenu, flow.CurrentScreen);
    }

    [Fact]
    public void PresentationScreenHost_ClickingBoard_StoresSelection()
    {
        var flow = new ScreenFlowController();
        var host = new PresentationScreenHost(flow, new SpriteBatchRenderer());
        flow.MainMenu.PlayButton.Click();

        host.Update(
            TimeSpan.FromMilliseconds(16),
            new InputState(true, new System.Numerics.Vector2(50f, 110f), true, false, 800, 480));

        Assert.NotNull(flow.Gameplay.SelectedCell);
    }


    [Fact]
    public void PresentationScreenHost_IgnoresBoardInput_WhileTransientEffectsAreActive()
    {
        var flow = new ScreenFlowController();
        var host = new PresentationScreenHost(flow, new SpriteBatchRenderer());
        flow.MainMenu.PlayButton.Click();
        flow.Tick();
        var gameplay = flow.Gameplay;
        var snapshot = gameplay.BoardRenderer.BuildSnapshot(gameplay.Board, gameplay.BoardTransform);

        gameplay.EffectsController.QueueSwap(snapshot, new Move(new GridPosition(0, 0), new GridPosition(0, 1)), rollback: false);

        host.Update(
            TimeSpan.FromMilliseconds(16),
            new InputState(true, new System.Numerics.Vector2(50f, 110f), true, false, 800, 480));

        Assert.Null(gameplay.SelectedCell);
    }

    private static BoardState CreateBoard()
    {
        var board = new BoardState();
        for (var row = 0; row < board.Height; row++)
        {
            for (var column = 0; column < board.Width; column++)
            {
                board.SetCell(new GridPosition(row, column), PieceCatalog.All[(row + column) % PieceCatalog.All.Count]);
            }
        }

        return board;
    }

    private static Match3.Core.GameFlow.Sessions.GameSession CreateExpiredSession()
    {
        var session = new Match3.Core.GameFlow.Sessions.GameSession();
        session.UpdateTimer(TimeSpan.FromSeconds(60));
        return session;
    }

    private sealed class FakeCanvas : IGameCanvas
    {
        public int ViewportWidth => 800;

        public int ViewportHeight => 480;

        public List<string> Texts { get; } = [];

        public void Begin()
        {
        }

        public void End()
        {
        }

        public void DrawFilledRectangle(float x, float y, float width, float height, string tint)
        {
        }

        public void DrawShape(string shape, float x, float y, float width, float height, string tint, float rotationRadians = 0f)
        {
        }

        public void DrawText(string text, float x, float y, string tint)
        {
            Texts.Add(text);
        }
    }
}
