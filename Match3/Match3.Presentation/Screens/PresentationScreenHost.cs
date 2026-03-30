using System;
using Match3.Presentation.Animation;
using Match3.Presentation.Rendering;
using Match3.Presentation.Runtime;

namespace Match3.Presentation.Screens;

public sealed class PresentationScreenHost : IGameScreenHost
{
    private readonly ScreenFlowController flowController;
    private readonly GameplayInteractionController gameplayInteractionController;
    private readonly GameplayRuntimeUpdater gameplayRuntimeUpdater;
    private readonly SpriteBatchRenderer renderer;

    public PresentationScreenHost(
        ScreenFlowController flowController,
        SpriteBatchRenderer renderer,
        GameplayRuntimeUpdater? gameplayRuntimeUpdater = null,
        GameplayInteractionController? gameplayInteractionController = null)
    {
        this.flowController = flowController ?? throw new ArgumentNullException(nameof(flowController));
        this.renderer = renderer ?? throw new ArgumentNullException(nameof(renderer));
        this.gameplayRuntimeUpdater = gameplayRuntimeUpdater ?? new GameplayRuntimeUpdater();
        this.gameplayInteractionController = gameplayInteractionController ?? new GameplayInteractionController();
    }

    /// <summary>
    /// Advances the active presentation screen, updates gameplay runtime systems, processes input, and applies screen-flow transitions.
    /// </summary>
    /// <param name="elapsed">Elapsed frame time.</param>
    /// <param name="inputState">Current pointer and viewport input state.</param>
    public void Update(TimeSpan elapsed, InputState inputState)
    {
        flowController.UpdateLayout(inputState.ViewportWidth, inputState.ViewportHeight);

        if (flowController.CurrentScreen is GameplayScreen gameplay)
        {
            gameplayRuntimeUpdater.Update(gameplay, elapsed);
        }

        HandleInput(inputState);
        flowController.Tick();
    }

    /// <summary>
    /// Draws the current screen after synchronizing layout with the canvas viewport.
    /// </summary>
    /// <param name="canvas">Target canvas used for rendering.</param>
    public void Draw(IGameCanvas canvas)
    {
        flowController.UpdateLayout(canvas.ViewportWidth, canvas.ViewportHeight);
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
            case MainMenuScreen mainMenu when ScreenLayoutMetrics.GetMainMenuPlayButtonBounds(inputState.ViewportWidth, inputState.ViewportHeight).Contains(inputState.PointerPosition):
                mainMenu.PlayButton.Click();
                break;
            case GameplayScreen gameplay:
                gameplayInteractionController.HandleClick(inputState, gameplay);
                break;
        }
    }
}
