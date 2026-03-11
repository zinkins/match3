using System;
using Match3.Core.GameCore.Board;
using Match3.Core.GameFlow.Pipeline;
using Match3.Core.GameFlow.Sessions;
using Match3.Core.GameFlow.StateMachine;
using Match3.Presentation.Animation;
using Match3.Presentation.Input;
using Match3.Presentation.Rendering;
using Match3.Presentation.UI;

namespace Match3.Presentation.Screens;

public sealed class ScreenFlowController
{
    private readonly LayoutCalculator layoutCalculator = new();
    private readonly Func<GameSession> sessionFactory;

    public ScreenFlowController(Func<GameSession>? sessionFactory = null)
    {
        this.sessionFactory = sessionFactory ?? (() => new GameSession());

        MainMenu = new MainMenuScreen();
        GameOver = new GameOverScreen();
        Gameplay = CreateGameplayScreen(this.sessionFactory());
        CurrentScreen = MainMenu;

        MainMenu.PlayRequested += StartGame;
        GameOver.OkRequested += ShowMainMenu;
    }

    public MainMenuScreen MainMenu { get; }

    public GameplayScreen Gameplay { get; private set; }

    public GameOverScreen GameOver { get; }

    public IScreen CurrentScreen { get; private set; }

    public void Tick()
    {
        if (CurrentScreen == Gameplay && Gameplay.ShouldShowGameOverOverlay)
        {
            CurrentScreen = GameOver;
        }
    }

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
        Gameplay = CreateGameplayScreen(sessionFactory());
        CurrentScreen = Gameplay;
    }

    private void ShowMainMenu()
    {
        CurrentScreen = MainMenu;
    }

    private static GameplayScreen CreateGameplayScreen(GameSession session)
    {
        var board = new BoardGenerator().Generate();
        var boardTransform = new BoardTransform(48f, new System.Numerics.Vector2(40f, 100f), board.Height, board.Width);
        var presenter = new GameplayPresenter(
            new TurnProcessor(),
            new GameplayStateMachine(),
            session,
            new AnimationQueue());

        return new GameplayScreen(
            presenter,
            board,
            new BoardInputHandler(boardTransform, new SelectionController()),
            new GameplayEffectsController(),
            new BoardRenderer(),
            new HudRenderer(),
            boardTransform);
    }
}
