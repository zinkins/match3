using Match3.Core.GameCore.Board;
using Match3.Core.GameCore.Bonuses;
using Match3.Core.GameCore.Pieces;
using Match3.Core.GameCore.ValueObjects;
using Match3.Core.Runtime;
using Match3.Presentation.Animation;
using Match3.Presentation.Input;
using Match3.Presentation.Rendering;
using Match3.Presentation.Screens;
using Match3.Presentation.UI;
using Match3.Presentation.Animation.Engine;

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

        var snapshot = renderer.BuildSnapshot(120, TimeSpan.FromSeconds(14), 800f, 480f);

        Assert.Equal(2, snapshot.Labels.Count);
        Assert.Contains(snapshot.Labels, label => label.Text == "Score: 120");
        Assert.Contains(snapshot.Labels, label => label.Text == "Time: 14");
    }

    [Fact]
    public void LayoutCalculator_ProducesStableGameplayLayout_ForDifferentViewportSizes()
    {
        var calculator = new LayoutCalculator();

        var hd = calculator.CalculateGameplayLayout(1280f, 720f);
        var fullHd = calculator.CalculateGameplayLayout(1920f, 1080f);

        Assert.True(hd.BoardTransform.CellSize > 0f);
        Assert.True(fullHd.BoardTransform.CellSize >= hd.BoardTransform.CellSize);
        Assert.True(hd.BoardTransform.Origin.X >= hd.SafeBounds.X);
        Assert.True(fullHd.BoardTransform.Origin.X >= fullHd.SafeBounds.X);
    }

    [Fact]
    public void LayoutCalculator_ProducesStableGameplayLayout_ForSupportedMobileOrientations()
    {
        var calculator = new LayoutCalculator();

        var phoneLandscape = calculator.CalculateGameplayLayout(1280f, 720f);
        var tabletLandscape = calculator.CalculateGameplayLayout(2732f, 2048f);

        Assert.True(phoneLandscape.BoardTransform.CellSize > 0f);
        Assert.True(tabletLandscape.BoardTransform.CellSize > 0f);
        Assert.True(phoneLandscape.BoardTransform.Origin.Y >= phoneLandscape.SafeBounds.Y);
        Assert.True(tabletLandscape.BoardTransform.Origin.Y >= tabletLandscape.SafeBounds.Y);
    }

    [Fact]
    public void BoardRenderer_RendersLineBonus_AsFlattenedDiamond()
    {
        var renderer = new BoardRenderer();
        var transform = new BoardTransform(48f, new System.Numerics.Vector2(20f, 20f));
        var board = CreateBoard();
        board.SetBonus(new GridPosition(0, 0), new LineBonus(new GridPosition(0, 0), PieceColor.Red, LineOrientation.Horizontal));

        var snapshot = renderer.BuildSnapshot(board, transform);
        var piece = Assert.Single(snapshot.Pieces, p => p.Position == new GridPosition(0, 0));

        Assert.Equal(PieceVisualConstants.ShapeDiamond, piece.Shape);
        Assert.True(piece.Width > piece.Height);
    }

    [Fact]
    public void BoardRenderer_RendersBombBonus_AsCircle()
    {
        var renderer = new BoardRenderer();
        var transform = new BoardTransform(48f, new System.Numerics.Vector2(20f, 20f));
        var board = CreateBoard();
        board.SetBonus(new GridPosition(0, 1), new BombBonus(new GridPosition(0, 1), PieceColor.Blue));

        var snapshot = renderer.BuildSnapshot(board, transform);
        var piece = Assert.Single(snapshot.Pieces, p => p.Position == new GridPosition(0, 1));

        Assert.Equal(PieceVisualConstants.ShapeCircle, piece.Shape);
        Assert.Equal(piece.Width, piece.Height);
    }

    [Fact]
    public void PieceNodeRenderer_UsesAnimatedNodeState_ForDrawing()
    {
        var viewState = new BoardViewState();
        var renderer = new PieceNodeRenderer();
        var position = new GridPosition(0, 0);
        var snapshot = new BoardRenderSnapshot(
            [],
            [
                new RenderPiece(position, PieceVisualConstants.ShapeSquare, PieceVisualConstants.TintRed, 20f, 20f, 32f, 32f)
            ]);
        var node = new PieceNode(
            NodeId.New(),
            position,
            new System.Numerics.Vector2(40f, 50f),
            new System.Numerics.Vector2(1.5f, 0.5f),
            rotation: 0.3f,
            opacity: 1f,
            tint: PieceVisualConstants.TintBlue,
            glow: 0f,
            isVisible: true);
        viewState.AddOrUpdate(node);

        var renderedSnapshot = renderer.BuildSnapshot(snapshot, viewState);
        var piece = Assert.Single(renderedSnapshot.Pieces);

        Assert.Equal(32f * 1.5f, piece.Width);
        Assert.Equal(32f * 0.5f, piece.Height);
        Assert.Equal(40f - ((piece.Width - 32f) / 2f), piece.X);
        Assert.Equal(50f - ((piece.Height - 32f) / 2f), piece.Y);
        Assert.Equal(0.3f, piece.Rotation);
        Assert.Equal(PieceVisualConstants.TintBlue, piece.Tint);
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
        var okBounds = ScreenLayoutMetrics.GetGameOverOkButtonBounds(800, 480);
        host.Update(
            TimeSpan.FromMilliseconds(16),
            new InputState(true, new System.Numerics.Vector2(okBounds.X + (okBounds.Width / 2f), okBounds.Y + (okBounds.Height / 2f)), true, false, 800, 480));

        Assert.Same(flow.MainMenu, flow.CurrentScreen);
    }

    [Fact]
    public void PresentationScreenHost_ClickingBoard_StoresSelection()
    {
        var flow = new ScreenFlowController();
        var host = new PresentationScreenHost(flow, new SpriteBatchRenderer());
        flow.MainMenu.PlayButton.Click();
        flow.UpdateLayout(800, 480);
        var cellWorld = flow.Gameplay.BoardTransform.GridToWorld(new GridPosition(0, 0));

        host.Update(
            TimeSpan.FromMilliseconds(16),
            new InputState(true, new System.Numerics.Vector2(cellWorld.X + 8f, cellWorld.Y + 8f), true, false, 800, 480));

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
        var cellWorld = gameplay.BoardTransform.GridToWorld(new GridPosition(0, 0));

        gameplay.EffectsController.QueueSwap(snapshot, new Move(new GridPosition(0, 0), new GridPosition(0, 1)), rollback: false);

        host.Update(
            TimeSpan.FromMilliseconds(16),
            new InputState(true, new System.Numerics.Vector2(cellWorld.X + 8f, cellWorld.Y + 8f), true, false, 800, 480));

        Assert.Null(gameplay.SelectedCell);
    }

    [Fact]
    public void PresentationScreenHost_AllowsBoardInput_WhenOnlyAnimationQueueIsRunning()
    {
        var flow = new ScreenFlowController();
        var host = new PresentationScreenHost(flow, new SpriteBatchRenderer());
        flow.MainMenu.PlayButton.Click();
        flow.Tick();
        flow.UpdateLayout(800, 480);
        var gameplay = flow.Gameplay;
        var cellWorld = gameplay.BoardTransform.GridToWorld(new GridPosition(0, 0));

        gameplay.AnimationQueue.Enqueue([new Match3.Core.GameFlow.Events.MatchResolved(3)]);

        host.Update(
            TimeSpan.FromMilliseconds(16),
            new InputState(true, new System.Numerics.Vector2(cellWorld.X + 8f, cellWorld.Y + 8f), true, false, 800, 480));

        Assert.NotNull(gameplay.SelectedCell);
    }

    [Fact]
    public void PresentationScreenHost_UsesTurnAnimationBuilder_InsteadOfManualQueueCalls()
    {
        var builder = new RecordingTurnAnimationBuilder();
        var flow = new ScreenFlowController(turnAnimationBuilderFactory: () => builder);
        var host = new PresentationScreenHost(flow, new SpriteBatchRenderer());
        flow.MainMenu.PlayButton.Click();
        flow.UpdateLayout(800, 480);
        var gameplay = flow.Gameplay;
        var firstClick = CreateBoardMoveTarget(gameplay, new GridPosition(0, 0));
        var secondClick = CreateBoardMoveTarget(gameplay, new GridPosition(0, 1));

        host.Update(
            TimeSpan.FromMilliseconds(16),
            new InputState(true, firstClick, true, false, 800, 480));

        host.Update(
            TimeSpan.FromMilliseconds(16),
            new InputState(true, secondClick, true, false, 800, 480));

        Assert.True(builder.WasBuildCalled);
        Assert.True(gameplay.AnimationPlayer.HasActiveAnimations);
    }

    [Fact]
    public void PresentationScreenHost_IgnoresBoardInput_WhenSessionIsGameOver()
    {
        var flow = new ScreenFlowController(CreateExpiredSession);
        var host = new PresentationScreenHost(flow, new SpriteBatchRenderer());
        flow.MainMenu.PlayButton.Click();
        flow.UpdateLayout(800, 480);
        var gameplay = flow.Gameplay;
        var cellWorld = gameplay.BoardTransform.GridToWorld(new GridPosition(0, 0));

        host.Update(
            TimeSpan.FromMilliseconds(16),
            new InputState(true, new System.Numerics.Vector2(cellWorld.X + 8f, cellWorld.Y + 8f), true, false, 800, 480));

        Assert.Null(gameplay.SelectedCell);
    }

    [Fact]
    public void ScreenFlowController_WaitsForTransientEffects_BeforeShowingGameOver()
    {
        var flow = new ScreenFlowController(CreateExpiredSession);
        flow.MainMenu.PlayButton.Click();
        var gameplay = flow.Gameplay;
        var snapshot = gameplay.BoardRenderer.BuildSnapshot(gameplay.Board, gameplay.BoardTransform);

        gameplay.EffectsController.QueueSwap(snapshot, new Move(new GridPosition(0, 0), new GridPosition(0, 1)), rollback: false);

        flow.Tick();
        Assert.Same(gameplay, flow.CurrentScreen);

        gameplay.EffectsController.Update(TimeSpan.FromSeconds(1));
        flow.Tick();

        Assert.Same(gameplay, flow.CurrentScreen);
    }

    private static BoardState CreateBoard()
    {
        var board = new BoardState();
        for (var row = 0; row < board.Height; row++)
        {
            for (var column = 0; column < board.Width; column++)
            {
                board.SetPiece(new GridPosition(row, column), PieceCatalog.All[(row + column) % PieceCatalog.All.Count]);
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

    private static System.Numerics.Vector2 CreateBoardMoveTarget(GameplayScreen gameplay, GridPosition position)
    {
        var cellWorld = gameplay.BoardTransform.GridToWorld(position);
        return new System.Numerics.Vector2(cellWorld.X + 8f, cellWorld.Y + 8f);
    }

    private sealed class RecordingTurnAnimationBuilder : Match3.Presentation.Animation.ITurnAnimationBuilder
    {
        public bool WasBuildCalled { get; private set; }

        public Match3.Presentation.Animation.Engine.IAnimation Build(Match3.Presentation.Animation.TurnAnimationContext context)
        {
            WasBuildCalled = true;
            return new DelayAnimation(0.5f, blocksInput: true);
        }
    }
}
