using System;
using Match3.Core.GameCore.Board;
using Match3.Core.GameFlow.Pipeline;
using Match3.Core.GameFlow.Sessions;
using Match3.Core.GameFlow.StateMachine;
using Match3.Presentation.Animation;
using Match3.Presentation.Input;
using Match3.Presentation.Rendering;

namespace Match3.Presentation.Screens;

public sealed class ScreenFlowController
{
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
        var boardTransform = new BoardTransform(48f, new System.Numerics.Vector2(40f, 100f));
        var presenter = new GameplayPresenter(
            new TurnProcessor(),
            new GameplayStateMachine(),
            session,
            new AnimationQueue());

        return new GameplayScreen(
            presenter,
            board,
            new BoardInputHandler(boardTransform, new SelectionController()),
            new BoardRenderer(),
            new HudRenderer(),
            boardTransform);
    }
}
