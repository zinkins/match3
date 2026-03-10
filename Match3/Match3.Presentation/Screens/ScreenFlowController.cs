using System;
using Match3.Core.GameFlow.Pipeline;
using Match3.Core.GameFlow.Sessions;
using Match3.Core.GameFlow.StateMachine;
using Match3.Presentation.Animation;
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
        var presenter = new GameplayPresenter(
            new TurnProcessor(),
            new GameplayStateMachine(),
            session,
            new AnimationQueue());

        return new GameplayScreen(
            presenter,
            new BoardRenderer(),
            new HudRenderer());
    }
}
