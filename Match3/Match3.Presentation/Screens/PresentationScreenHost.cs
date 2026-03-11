using System;
using Match3.Core.Runtime;
using Match3.Presentation.Input;
using Match3.Presentation.Rendering;

namespace Match3.Presentation.Screens;

public sealed class PresentationScreenHost : IGameScreenHost
{
    private readonly ScreenFlowController flowController;
    private readonly SpriteBatchRenderer renderer;
    private readonly MouseInputRouter mouseInputRouter = new();
    private readonly TouchInputRouter touchInputRouter = new();

    public PresentationScreenHost(ScreenFlowController flowController, SpriteBatchRenderer renderer)
    {
        this.flowController = flowController ?? throw new ArgumentNullException(nameof(flowController));
        this.renderer = renderer ?? throw new ArgumentNullException(nameof(renderer));
    }

    public void Update(TimeSpan elapsed, InputState inputState)
    {
        if (flowController.CurrentScreen is GameplayScreen gameplay)
        {
            gameplay.Presenter.Update(elapsed);
        }

        HandleInput(inputState);
        flowController.Tick();
    }

    public void Draw(IGameCanvas canvas)
    {
        renderer.Draw(canvas, flowController.CurrentScreen);
    }

    private void HandleInput(InputState inputState)
    {
        if (!inputState.IsPrimaryClick)
        {
            return;
        }

        switch (flowController.CurrentScreen)
        {
            case MainMenuScreen mainMenu when ScreenLayoutMetrics.GetMainMenuPlayButtonBounds(inputState.ViewportWidth).Contains(ToNumerics(inputState.PointerPosition)):
                mainMenu.PlayButton.Click();
                break;
            case GameOverScreen gameOver when ScreenLayoutMetrics.GetGameOverOkButtonBounds(inputState.ViewportWidth).Contains(ToNumerics(inputState.PointerPosition)):
                gameOver.OkButton.Click();
                break;
            case GameplayScreen gameplay:
                HandleGameplayInput(inputState, gameplay);
                break;
        }
    }

    private void HandleGameplayInput(InputState inputState, GameplayScreen gameplay)
    {
        if (!mouseInputRouter.ShouldHandleBoardSelection(inputState) &&
            !touchInputRouter.ShouldHandleBoardSelection(inputState))
        {
            return;
        }

        var move = gameplay.BoardInputHandler.HandleClick(ToNumerics(inputState.PointerPosition));
        if (move is not null)
        {
            gameplay.Presenter.ProcessMove(gameplay.Board, move.Value);
        }
    }

    private static System.Numerics.Vector2 ToNumerics(System.Numerics.Vector2 value)
    {
        return value;
    }
}
