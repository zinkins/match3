using System;
using Match3.Core.GameCore.Board;
using Match3.Core.GameFlow.Pipeline;
using Match3.Core.GameFlow.Sessions;
using Match3.Core.GameFlow.StateMachine;
using Match3.Presentation.Animation;
using Match3.Presentation.Animation.Engine;
using Match3.Presentation.Input;
using Match3.Presentation.Rendering;
using Match3.Presentation.UI;

namespace Match3.Presentation.Screens;

public sealed class ScreenFlowController
{
    private readonly LayoutCalculator layoutCalculator = new();
    private readonly Func<GameSession> sessionFactory;
    private readonly Func<ITurnAnimationBuilder> turnAnimationBuilderFactory;

    public ScreenFlowController(Func<GameSession>? sessionFactory = null, Func<ITurnAnimationBuilder>? turnAnimationBuilderFactory = null)
    {
        this.sessionFactory = sessionFactory ?? (() => new GameSession());
        this.turnAnimationBuilderFactory = turnAnimationBuilderFactory ?? (() => new TurnAnimationBuilder());

        MainMenu = new MainMenuScreen();
        Gameplay = CreateGameplayScreen(this.sessionFactory(), ShowMainMenu);
        CurrentScreen = MainMenu;

        MainMenu.PlayRequested += StartGame;
    }

    public MainMenuScreen MainMenu { get; }

    public GameplayScreen Gameplay { get; private set; }

    public IScreen CurrentScreen { get; private set; }

    /// <summary>
    /// Advances high-level screen flow, including delayed game-over presentation once blocking animations finish.
    /// </summary>
    public void Tick()
    {
        if (CurrentScreen == Gameplay &&
            Gameplay.ShouldShowGameOverOverlay &&
            !Gameplay.AnimationPlayer.HasBlockingAnimations)
        {
            CurrentScreen = Gameplay;
        }
    }

    /// <summary>
    /// Recomputes gameplay layout for the current viewport and updates the active board transform in place.
    /// </summary>
    /// <param name="viewportWidth">Viewport width in pixels.</param>
    /// <param name="viewportHeight">Viewport height in pixels.</param>
    public void UpdateLayout(int viewportWidth, int viewportHeight)
    {
        var gameplayLayout = layoutCalculator.CalculateGameplayLayout(
            viewportWidth,
            viewportHeight,
            Gameplay.Board.Height,
            Gameplay.Board.Width);
        Gameplay.BoardTransform.UpdateLayout(
            gameplayLayout.BoardTransform.CellSize,
            gameplayLayout.BoardTransform.Origin,
            gameplayLayout.BoardTransform.Rows,
            gameplayLayout.BoardTransform.Columns);
    }

    private void StartGame()
    {
        Gameplay = CreateGameplayScreen(sessionFactory(), ShowMainMenu);
        CurrentScreen = Gameplay;
    }

    private void ShowMainMenu()
    {
        CurrentScreen = MainMenu;
    }

    private GameplayScreen CreateGameplayScreen(GameSession session, Action onOk)
    {
        var board = new BoardGenerator().Generate();
        var boardTransform = new BoardTransform(
            LayoutMetrics.InitialBoardCellSize,
            new System.Numerics.Vector2(LayoutMetrics.InitialBoardOriginX, LayoutMetrics.InitialBoardOriginY),
            board.Height,
            board.Width);
        var presenter = new GameplayPresenter(
            new TurnProcessor(),
            new GameplayStateMachine(),
            session);

        return new GameplayScreen(
            presenter,
            board,
            new BoardInputHandler(boardTransform, new SelectionController()),
            new AnimationPlayer(),
            turnAnimationBuilderFactory(),
            new BoardRenderer(),
            new HudRenderer(),
            boardTransform,
            onOk);
    }
}
