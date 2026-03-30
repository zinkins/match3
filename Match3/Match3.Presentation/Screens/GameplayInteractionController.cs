using System;
using Match3.Presentation.Input;
using Match3.Presentation.Runtime;

namespace Match3.Presentation.Screens;

public sealed class GameplayInteractionController
{
    private readonly MouseInputRouter mouseInputRouter = new();
    private readonly TouchInputRouter touchInputRouter = new();
    private readonly GameplayTurnAnimationCoordinator turnAnimationCoordinator;

    public GameplayInteractionController(GameplayTurnAnimationCoordinator? turnAnimationCoordinator = null)
    {
        this.turnAnimationCoordinator = turnAnimationCoordinator ?? new GameplayTurnAnimationCoordinator();
    }

    public void HandleClick(InputState inputState, GameplayScreen gameplay)
    {
        ArgumentNullException.ThrowIfNull(gameplay);

        if (gameplay.ShouldShowGameOverOverlay)
        {
            if (ScreenLayoutMetrics.GetGameOverOkButtonBounds(inputState.ViewportWidth, inputState.ViewportHeight).Contains(inputState.PointerPosition))
            {
                gameplay.OkButton.Click();
            }

            return;
        }

        if (!gameplay.Presenter.CanAcceptInput ||
            gameplay.AnimationPlayer.HasBlockingAnimations)
        {
            return;
        }

        if (!mouseInputRouter.ShouldHandleBoardSelection(inputState) &&
            !touchInputRouter.ShouldHandleBoardSelection(inputState))
        {
            return;
        }

        var move = gameplay.BoardInputHandler.HandleClick(inputState.PointerPosition);
        if (move is null)
        {
            return;
        }

        turnAnimationCoordinator.PlayTurn(gameplay, move.Value);
    }
}
