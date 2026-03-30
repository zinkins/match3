using System;
using Match3.Core.GameFlow.Sessions;
using Match3.Presentation.Animation;
using Match3.Presentation.UI;

namespace Match3.Presentation.Screens;

public sealed class ScreenFlowController
{
    private readonly LayoutCalculator layoutCalculator = new();
    private readonly GameplayScreenFactory gameplayScreenFactory;
    private readonly Func<GameSession> sessionFactory;

    public ScreenFlowController(Func<GameSession>? sessionFactory = null, Func<ITurnAnimationBuilder>? turnAnimationBuilderFactory = null)
    {
        this.sessionFactory = sessionFactory ?? (() => new GameSession());
        gameplayScreenFactory = new GameplayScreenFactory(turnAnimationBuilderFactory: turnAnimationBuilderFactory);

        MainMenu = new MainMenuScreen();
        Gameplay = gameplayScreenFactory.Create(this.sessionFactory(), ShowMainMenu);
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
        Gameplay = gameplayScreenFactory.Create(sessionFactory(), ShowMainMenu);
        CurrentScreen = Gameplay;
    }

    private void ShowMainMenu()
    {
        CurrentScreen = MainMenu;
    }
}
