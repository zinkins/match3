using System;
using Match3.Core.Runtime;
using Match3.Core.GameCore.Board;
using Match3.Core.GameFlow.Events;
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
        flowController.UpdateLayout(inputState.ViewportWidth, inputState.ViewportHeight);

        if (flowController.CurrentScreen is GameplayScreen gameplay)
        {
            gameplay.Presenter.Update(elapsed);
            gameplay.EffectsController.Update(elapsed);
        }

        HandleInput(inputState);
        flowController.Tick();
    }

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
            case MainMenuScreen mainMenu when ScreenLayoutMetrics.GetMainMenuPlayButtonBounds(inputState.ViewportWidth, inputState.ViewportHeight).Contains(ToNumerics(inputState.PointerPosition)):
                mainMenu.PlayButton.Click();
                break;
            case GameOverScreen gameOver when ScreenLayoutMetrics.GetGameOverOkButtonBounds(inputState.ViewportWidth, inputState.ViewportHeight).Contains(ToNumerics(inputState.PointerPosition)):
                gameOver.OkButton.Click();
                break;
            case GameplayScreen gameplay:
                HandleGameplayInput(inputState, gameplay);
                break;
        }
    }

    private void HandleGameplayInput(InputState inputState, GameplayScreen gameplay)
    {
        if (gameplay.EffectsController.HasActiveBlockingEffects)
        {
            return;
        }

        if (!mouseInputRouter.ShouldHandleBoardSelection(inputState) &&
            !touchInputRouter.ShouldHandleBoardSelection(inputState))
        {
            return;
        }

        var move = gameplay.BoardInputHandler.HandleClick(ToNumerics(inputState.PointerPosition));
        if (move is not null)
        {
            var beforeBoard = gameplay.Board.Clone();
            var beforeSnapshot = gameplay.BoardRenderer.BuildSnapshot(beforeBoard, gameplay.BoardTransform);
            var result = gameplay.Presenter.ProcessMove(gameplay.Board, move.Value);
            var afterSnapshot = gameplay.BoardRenderer.BuildSnapshot(gameplay.Board, gameplay.BoardTransform);
            QueueVisualEvents(gameplay, result.Events);
            gameplay.EffectsController.QueueSwap(beforeSnapshot, move.Value, rollback: !result.IsSwapApplied);
            if (result.IsSwapApplied)
            {
                var swappedBoard = beforeBoard.Clone();
                ApplySwap(swappedBoard, move.Value);
                var swappedSnapshot = gameplay.BoardRenderer.BuildSnapshot(swappedBoard, gameplay.BoardTransform);
                gameplay.EffectsController.QueueBoardSettle(swappedSnapshot, afterSnapshot, gameplay.BoardTransform.CellSize);
            }
        }
    }

    private static System.Numerics.Vector2 ToNumerics(System.Numerics.Vector2 value)
    {
        return value;
    }

    private static void ApplySwap(BoardState board, Match3.Core.GameCore.ValueObjects.Move move)
    {
        var fromPiece = board.GetContent(move.From);
        var toPiece = board.GetContent(move.To);
        board.SetContent(move.From, toPiece);
        board.SetContent(move.To, fromPiece);
    }

    private static void QueueVisualEvents(GameplayScreen gameplay, IReadOnlyList<IDomainEvent> events)
    {
        foreach (var domainEvent in events)
        {
            switch (domainEvent)
            {
                case DestroyerSpawned destroyer:
                    gameplay.EffectsController.QueueDestroyer(destroyer.Position, destroyer.Path, gameplay.BoardTransform);
                    break;
                case BombExploded explosion:
                    gameplay.EffectsController.QueueExplosion(explosion.Area, gameplay.BoardTransform);
                    break;
            }
        }
    }
}
