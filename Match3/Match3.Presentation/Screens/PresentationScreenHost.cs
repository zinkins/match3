using System;
using System.Linq;
using Match3.Core.GameCore.Board;
using Match3.Core.GameCore.ValueObjects;
using Match3.Core.GameFlow.Events;
using Match3.Core.GameFlow.Pipeline;
using Match3.Core.Runtime;
using Match3.Presentation.Animation;
using Match3.Presentation.Input;
using Match3.Presentation.Rendering;

namespace Match3.Presentation.Screens;

public sealed class PresentationScreenHost : IGameScreenHost
{
    private const float MinimumResolveDurationSeconds = 0.18f;

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
            gameplay.VisualState.Update((float)elapsed.TotalSeconds);
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
            case MainMenuScreen mainMenu when ScreenLayoutMetrics.GetMainMenuPlayButtonBounds(inputState.ViewportWidth, inputState.ViewportHeight).Contains(inputState.PointerPosition):
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

        var beforeBoard = gameplay.Board.Clone();
        gameplay.SetVisualBoard(beforeBoard);
        var result = gameplay.Presenter.ProcessMove(gameplay.Board, move.Value);
        var animation = gameplay.TurnAnimationBuilder.Build(new TurnAnimationContext
        {
            IsSwapApplied = result.IsSwapApplied,
            QueueSwapAnimation = () => GameplayAnimationRuntime.QueueSwap(gameplay.BoardViewState, gameplay.AnimationPlayer, move.Value, rollback: !result.IsSwapApplied),
            SwapDurationSeconds = result.IsSwapApplied ? 0.22f : 0.36f,
            CascadeSteps = BuildCascadeSteps(gameplay, result)
        });

        gameplay.AnimationPlayer.Play(animation);
    }

    private static void ApplySwap(BoardState board, Move move)
    {
        var fromPiece = board.GetContent(move.From);
        var toPiece = board.GetContent(move.To);
        board.SetContent(move.From, toPiece);
        board.SetContent(move.To, fromPiece);
    }

    private static IReadOnlyList<TurnAnimationCascadeStep> BuildCascadeSteps(GameplayScreen gameplay, TurnPipelineResult result)
    {
        return result.CascadeSteps
            .Select(step =>
            {
                var resolvedSnapshot = gameplay.BoardRenderer.BuildSnapshot(step.ResolvedBoard, gameplay.BoardTransform);
                var gravitySnapshot = gameplay.BoardRenderer.BuildSnapshot(step.GravityBoard, gameplay.BoardTransform);
                var endSnapshot = gameplay.BoardRenderer.BuildSnapshot(step.EndBoard, gameplay.BoardTransform);
                var createdBonusOrigins = GetCreatedBonusOrigins(step.Events);
                var createdBonusTargets = GetCreatedBonusTargets(endSnapshot, createdBonusOrigins);
                var resolveBoard = CloneClearing(step.ResolvedBoard, createdBonusTargets);
                var gravityBoard = CloneClearing(step.GravityBoard, createdBonusTargets);
                return new TurnAnimationCascadeStep
                {
                    QueueResolveAnimation = () =>
                    {
                        var beforeResolveSnapshot = gameplay.BoardRenderer.BuildSnapshot(gameplay.VisualBoard, gameplay.BoardTransform);
                        gameplay.SetVisualBoard(resolveBoard);
                        GameplayAnimationRuntime.QueueMatchPop(
                            gameplay.BoardViewState,
                            gameplay.AnimationPlayer,
                            beforeResolveSnapshot,
                            gameplay.BoardRenderer.BuildSnapshot(resolveBoard, gameplay.BoardTransform),
                            step.Events);
                        GameplayVisualEffectsTimeline.QueueEvents(gameplay.BoardViewState, gameplay.AnimationPlayer, step.Events, gameplay.BoardTransform);
                    },
                    QueueGravityAnimation = () =>
                    {
                        gameplay.SetVisualBoard(gravityBoard);
                        GameplayAnimationRuntime.QueueGravity(
                            gameplay.BoardViewState,
                            gameplay.AnimationPlayer,
                            resolvedSnapshot,
                            gravitySnapshot,
                            0f,
                            createdBonusTargets,
                            gameplay.VisualState);
                    },
                    QueueSpawnAnimation = () =>
                    {
                        gameplay.SetVisualBoard(step.EndBoard);
                        GameplayAnimationRuntime.QueueSpawn(
                            gameplay.BoardViewState,
                            gameplay.AnimationPlayer,
                            gravitySnapshot,
                            endSnapshot,
                            gameplay.BoardTransform.CellSize,
                            0f,
                            createdBonusTargets);
                        GameplayAnimationRuntime.QueueCreatedBonuses(
                            gameplay.BoardViewState,
                            gameplay.AnimationPlayer,
                            endSnapshot,
                            gameplay.BoardTransform.CellSize,
                            0f,
                            createdBonusOrigins);
                    },
                    QueueSettleAnimation = () => gameplay.SetVisualBoard(step.EndBoard),
                    ResolveDurationSeconds = GetResolveDurationSeconds(step.Events),
                    GravityDurationSeconds = HasGravityMovement(resolvedSnapshot, gravitySnapshot) ? 0.65f : 0f,
                    SpawnDurationSeconds = HasSpawnMovement(gravitySnapshot, endSnapshot, createdBonusTargets, createdBonusOrigins) ? 0.75f : 0f,
                    SettleDurationSeconds = 0f
                };
            })
            .ToArray();
    }

    private static BoardState CloneClearing(BoardState source, IReadOnlyList<GridPosition> createdBonusTargets)
    {
        var clone = source.Clone();
        foreach (var position in createdBonusTargets)
        {
            clone.SetContent(position, null);
        }

        return clone;
    }

    private static bool HasGravityMovement(BoardRenderSnapshot startSnapshot, BoardRenderSnapshot endSnapshot)
    {
        for (var column = 0; column < 8; column++)
        {
            var beforeColumn = startSnapshot.Pieces
                .Where(piece => piece.Position.Column == column)
                .OrderBy(piece => piece.Position.Row)
                .ToArray();
            var afterColumn = endSnapshot.Pieces
                .Where(piece => piece.Position.Column == column)
                .OrderBy(piece => piece.Position.Row)
                .ToArray();
            var survivorCount = Math.Min(beforeColumn.Length, afterColumn.Length);
            for (var i = 0; i < survivorCount; i++)
            {
                if (beforeColumn[i].Position != afterColumn[i].Position)
                {
                    return true;
                }
            }
        }

        return false;
    }

    private static bool HasSpawnMovement(
        BoardRenderSnapshot startSnapshot,
        BoardRenderSnapshot endSnapshot,
        IReadOnlyList<GridPosition> createdBonusTargets,
        IReadOnlyList<GridPosition> createdBonusOrigins)
    {
        var beforeCount = startSnapshot.Pieces.Count;
        var afterCount = endSnapshot.Pieces.Count;
        return afterCount > beforeCount || createdBonusTargets.Count > 0 || createdBonusOrigins.Count > 0;
    }

    private static float GetResolveDurationSeconds(IReadOnlyList<IDomainEvent> events)
    {
        return MathF.Max(MinimumResolveDurationSeconds, GameplayVisualEffectsTimeline.GetTotalDuration(events));
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
