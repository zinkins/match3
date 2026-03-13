using System;
using System.Linq;
using Match3.Core.GameCore.Board;
using Match3.Core.GameCore.ValueObjects;
using Match3.Core.GameFlow.Events;
using Match3.Core.Runtime;
using Match3.Presentation.Animation;
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
            gameplay.AnimationPlayer.Update((float)elapsed.TotalSeconds);
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
            case GameplayScreen gameplay:
                HandleGameplayInput(inputState, gameplay);
                break;
        }
    }

    private void HandleGameplayInput(InputState inputState, GameplayScreen gameplay)
    {
        if (gameplay.ShouldShowGameOverOverlay)
        {
            if (ScreenLayoutMetrics.GetGameOverOkButtonBounds(inputState.ViewportWidth, inputState.ViewportHeight).Contains(ToNumerics(inputState.PointerPosition)))
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

        var move = gameplay.BoardInputHandler.HandleClick(ToNumerics(inputState.PointerPosition));
        if (move is null)
        {
            return;
        }

        var beforeBoard = gameplay.Board.Clone();
        var beforeSnapshot = gameplay.BoardRenderer.BuildSnapshot(beforeBoard, gameplay.BoardTransform);
        var result = gameplay.Presenter.ProcessMove(gameplay.Board, move.Value);
        var afterSnapshot = gameplay.BoardRenderer.BuildSnapshot(gameplay.Board, gameplay.BoardTransform);
        var swappedBoard = beforeBoard.Clone();
        ApplySwap(swappedBoard, move.Value);
        var swappedSnapshot = gameplay.BoardRenderer.BuildSnapshot(swappedBoard, gameplay.BoardTransform);
        var createdBonusOrigins = GetCreatedBonusOrigins(result.Events);
        var createdBonusTargets = GetCreatedBonusTargets(afterSnapshot, createdBonusOrigins);
        var animation = gameplay.TurnAnimationBuilder.Build(new TurnAnimationContext
        {
            IsSwapApplied = result.IsSwapApplied,
            QueueSwapAnimation = () => GameplayAnimationRuntime.QueueSwap(gameplay.BoardViewState, gameplay.AnimationPlayer, move.Value, rollback: !result.IsSwapApplied),
            QueueResolveAnimation = () => GameplayVisualEffectsTimeline.QueueEvents(gameplay.BoardViewState, gameplay.AnimationPlayer, result.Events, gameplay.BoardTransform),
            QueueGravityAnimation = () => GameplayAnimationRuntime.QueueBoardSettle(
                gameplay.BoardViewState,
                gameplay.AnimationPlayer,
                swappedSnapshot,
                afterSnapshot,
                gameplay.BoardTransform.CellSize,
                0f,
                createdBonusTargets,
                gameplay.VisualState),
            QueueSpawnAnimation = () => GameplayAnimationRuntime.QueueCreatedBonuses(
                gameplay.BoardViewState,
                gameplay.AnimationPlayer,
                afterSnapshot,
                gameplay.BoardTransform.CellSize,
                0f,
                createdBonusOrigins),
            QueueSettleAnimation = static () => { },
            SwapDurationSeconds = result.IsSwapApplied ? 0.22f : 0.36f,
            ResolveDurationSeconds = GameplayVisualEffectsTimeline.GetTotalDuration(result.Events),
            GravityDurationSeconds = 0f,
            SpawnDurationSeconds = 0f,
            SettleDurationSeconds = 1.15f
        });

        gameplay.AnimationPlayer.Play(animation);
    }

    private static System.Numerics.Vector2 ToNumerics(System.Numerics.Vector2 value)
    {
        return value;
    }

    private static void ApplySwap(BoardState board, Move move)
    {
        var fromPiece = board.GetContent(move.From);
        var toPiece = board.GetContent(move.To);
        board.SetContent(move.From, toPiece);
        board.SetContent(move.To, fromPiece);
    }
    private static IReadOnlyList<GridPosition> GetCreatedBonusTargets(BoardRenderSnapshot snapshot, IReadOnlyList<GridPosition> createdBonusOrigins)
    {
        if (createdBonusOrigins.Count == 0)
        {
            return [];
        }

        return snapshot.Pieces
            .Where(piece => piece.Shape == PieceVisualConstants.ShapeDiamond || piece.Shape == PieceVisualConstants.ShapeCircle)
            .Where(piece => createdBonusOrigins.Any(origin => origin.Column == piece.Position.Column && origin.Row <= piece.Position.Row))
            .Select(piece => piece.Position)
            .ToArray();
    }
    private static IReadOnlyList<GridPosition> GetCreatedBonusOrigins(IReadOnlyList<IDomainEvent> events)
    {
        return events
            .Select(domainEvent => domainEvent switch
            {
                LineBonusCreated created => (GridPosition?)created.Position,
                BombBonusCreated created => created.Position,
                _ => null
            })
            .Where(position => position is not null)
            .Select(position => position!.Value)
            .ToArray();
    }

}
