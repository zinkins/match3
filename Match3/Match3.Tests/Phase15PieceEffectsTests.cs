using Match3.Core.GameCore.ValueObjects;
using Match3.Presentation.Animation;
using Match3.Presentation.Animation.Engine;
using Match3.Presentation.Rendering;

namespace Match3.Tests;

public class Phase15PieceEffectsTests
{
    [Fact]
    public void CompositePieceEffect_CombinesScaleAndRotation()
    {
        var piece = new RenderPiece(new GridPosition(0, 0), PieceVisualConstants.ShapeSquare, PieceVisualConstants.TintRed, 10f, 10f, 20f, 20f);
        var effect = new CompositePieceEffect(
            new PulseScaleEffect(0.12f),
            new RotationEffect(0.18f));

        var result = effect.Apply(piece, new PieceEffectClock(0.5f, 1f, Loop: true));

        Assert.True(result.Width > piece.Width);
        Assert.NotEqual(0f, result.Rotation);
    }

    [Fact]
    public void SequencePieceEffect_PerformsRollbackMotion()
    {
        var piece = new RenderPiece(new GridPosition(0, 0), PieceVisualConstants.ShapeSquare, PieceVisualConstants.TintRed, 0f, 0f, 20f, 20f);
        var effect = new SequencePieceEffect(
            (new MovePieceEffect(new System.Numerics.Vector2(0f, 0f), new System.Numerics.Vector2(20f, 0f)), 0.2f),
            (new MovePieceEffect(new System.Numerics.Vector2(20f, 0f), new System.Numerics.Vector2(0f, 0f)), 0.2f));

        var forward = effect.Apply(piece, new PieceEffectClock(0.1f, 0.4f, Loop: false));
        var backward = effect.Apply(piece, new PieceEffectClock(0.3f, 0.4f, Loop: false));

        Assert.True(forward.X > piece.X);
        Assert.True(backward.X < 20f);
    }

    [Fact]
    public void GameplayEffectsController_QueuesDestroyerVisualOverlay()
    {
        var controller = new GameplayEffectsController();
        var player = new AnimationPlayer();
        var viewState = new BoardViewState();
        var transform = new BoardTransform(48f, new System.Numerics.Vector2(20f, 20f));
        var snapshot = new BoardRenderSnapshot(
            [],
            [
                new RenderPiece(new GridPosition(0, 0), PieceVisualConstants.ShapeSquare, PieceVisualConstants.TintRed, 20f, 20f, 32f, 32f)
            ]);

        controller.QueueDestroyer(viewState, player, new GridPosition(0, 1), [new GridPosition(0, 0), new GridPosition(0, 1), new GridPosition(0, 2)], transform);
        controller.Update(TimeSpan.FromSeconds(0.05f));
        player.Update(0.05f);

        var pieces = controller.BuildPieces(snapshot, null, viewState);

        Assert.Contains(pieces, piece => piece.Shape == PieceVisualConstants.ShapeDiamond && piece.Tint == PieceVisualConstants.TintWhite);
    }

    [Fact]
    public void GameplayEffectsController_QueuesTwoDestroyers_FromBonusCenter()
    {
        var controller = new GameplayEffectsController();
        var player = new AnimationPlayer();
        var viewState = new BoardViewState();
        var transform = new BoardTransform(48f, new System.Numerics.Vector2(20f, 20f));
        var snapshot = new BoardRenderSnapshot([], []);

        controller.QueueDestroyer(
            viewState,
            player,
            new GridPosition(0, 2),
            [new GridPosition(0, 0), new GridPosition(0, 1), new GridPosition(0, 2), new GridPosition(0, 3), new GridPosition(0, 4)],
            transform);
        controller.Update(TimeSpan.FromSeconds(0.05f));
        player.Update(0.05f);

        var pieces = controller.BuildPieces(snapshot, null, viewState);

        Assert.Equal(2, pieces.Count(piece => piece.Shape == PieceVisualConstants.ShapeDiamond && piece.Tint == PieceVisualConstants.TintWhite));
    }

    [Fact]
    public void GameplayEffectsController_HidesPiecesInsideExplosionArea_WhileBombEffectRuns()
    {
        var controller = new GameplayEffectsController();
        var player = new AnimationPlayer();
        var viewState = new BoardViewState();
        var transform = new BoardTransform(48f, new System.Numerics.Vector2(20f, 20f));
        var affected = new GridPosition(2, 2);
        var unaffected = new GridPosition(0, 0);
        var snapshot = new BoardRenderSnapshot(
            [],
            [
                new RenderPiece(unaffected, PieceVisualConstants.ShapeSquare, PieceVisualConstants.TintRed, 20f, 20f, 32f, 32f),
                new RenderPiece(affected, PieceVisualConstants.ShapeSquare, PieceVisualConstants.TintBlue, 116f, 116f, 32f, 32f)
            ]);

        controller.QueueExplosion(viewState, player, [affected], transform);
        player.Update(0.05f);

        var pieces = controller.BuildPieces(snapshot, null, viewState);

        Assert.DoesNotContain(pieces, piece => piece.Position == affected);
        Assert.Contains(pieces, piece => piece.Position == unaffected);
    }

    [Fact]
    public void GameplayEffectsController_MovesDestroyerTransientNode_AlongPath()
    {
        var controller = new GameplayEffectsController();
        var player = new AnimationPlayer();
        var viewState = new BoardViewState();
        var transform = new BoardTransform(48f, new System.Numerics.Vector2(20f, 20f));
        var origin = new GridPosition(0, 1);
        var mid = new GridPosition(0, 2);
        var tail = new GridPosition(0, 3);
        var snapshot = new BoardRenderSnapshot(
            [],
            [
                new RenderPiece(origin, PieceVisualConstants.ShapeSquare, PieceVisualConstants.TintRed, 68f, 20f, 32f, 32f),
                new RenderPiece(mid, PieceVisualConstants.ShapeSquare, PieceVisualConstants.TintBlue, 116f, 20f, 32f, 32f),
                new RenderPiece(tail, PieceVisualConstants.ShapeSquare, PieceVisualConstants.TintGreen, 164f, 20f, 32f, 32f)
            ]);

        controller.QueueDestroyer(viewState, player, origin, [new GridPosition(0, 0), origin, mid, tail], transform);
        controller.Update(TimeSpan.FromSeconds(0.05f));
        player.Update(0.05f);

        var earlyPieces = controller.BuildPieces(snapshot, null, viewState);
        var earlyDestroyerX = earlyPieces
            .Where(piece => piece.Shape == PieceVisualConstants.ShapeDiamond)
            .Max(piece => piece.X);

        AdvanceRuntime(controller, player, 0.55f);

        var latePieces = controller.BuildPieces(snapshot, null, viewState);
        var lateDestroyerX = latePieces
            .Where(piece => piece.Shape == PieceVisualConstants.ShapeDiamond)
            .Max(piece => piece.X);

        Assert.True(lateDestroyerX > earlyDestroyerX);
    }

    [Fact]
    public void GameplayEffectsController_DoesNotHideDelayedSettleTargets_BeforeSettleStarts()
    {
        var controller = new GameplayEffectsController();
        var player = new AnimationPlayer();
        var viewState = new BoardViewState();
        var renderer = new PieceNodeRenderer();
        var beforeSnapshot = new BoardRenderSnapshot(
            [],
            [
                new RenderPiece(new GridPosition(0, 0), PieceVisualConstants.ShapeSquare, PieceVisualConstants.TintRed, 20f, 20f, 32f, 32f)
            ]);
        var afterSnapshot = new BoardRenderSnapshot(
            [],
            [
                new RenderPiece(new GridPosition(1, 0), PieceVisualConstants.ShapeSquare, PieceVisualConstants.TintRed, 20f, 68f, 32f, 32f)
            ]);

        controller.QueueBoardSettle(viewState, player, beforeSnapshot, afterSnapshot, 48f, initialDelaySeconds: 0.8f);

        var nodeSnapshot = renderer.BuildSnapshot(afterSnapshot, viewState);
        var pieces = controller.BuildPieces(nodeSnapshot, null, viewState);
        var movedPiece = Assert.Single(pieces, piece => piece.Position == new GridPosition(1, 0));

        Assert.Equal(20f, movedPiece.Y);
    }

    [Fact]
    public void GameplayEffectsController_DoesNotAnimatePiecesUpward_DuringSettle()
    {
        var controller = new GameplayEffectsController();
        var player = new AnimationPlayer();
        var viewState = new BoardViewState();
        var renderer = new PieceNodeRenderer();
        var beforeSnapshot = new BoardRenderSnapshot(
            [],
            [
                new RenderPiece(new GridPosition(0, 0), PieceVisualConstants.ShapeSquare, PieceVisualConstants.TintRed, 20f, 20f, 32f, 32f),
                new RenderPiece(new GridPosition(1, 0), PieceVisualConstants.ShapeSquare, PieceVisualConstants.TintBlue, 20f, 68f, 32f, 32f),
                new RenderPiece(new GridPosition(2, 0), PieceVisualConstants.ShapeSquare, PieceVisualConstants.TintRed, 20f, 116f, 32f, 32f)
            ]);
        var afterSnapshot = new BoardRenderSnapshot(
            [],
            [
                new RenderPiece(new GridPosition(1, 0), PieceVisualConstants.ShapeSquare, PieceVisualConstants.TintRed, 20f, 68f, 32f, 32f),
                new RenderPiece(new GridPosition(2, 0), PieceVisualConstants.ShapeSquare, PieceVisualConstants.TintBlue, 20f, 116f, 32f, 32f)
            ]);

        controller.QueueBoardSettle(viewState, player, beforeSnapshot, afterSnapshot, 48f);
        player.Update(0.05f);

        var nodeSnapshot = renderer.BuildSnapshot(afterSnapshot, viewState);
        var pieces = controller.BuildPieces(nodeSnapshot, null, viewState);
        var movedBluePiece = Assert.Single(pieces, piece => piece.Position == new GridPosition(2, 0) && piece.Tint == PieceVisualConstants.TintBlue);

        Assert.True(movedBluePiece.Y <= 116f);
    }

    [Fact]
    public void GameplayEffectsController_SpawnsPiecesAboveBoard_BeforeSpawnAnimationStarts()
    {
        var controller = new GameplayEffectsController();
        var player = new AnimationPlayer();
        var viewState = new BoardViewState();
        var renderer = new PieceNodeRenderer();
        var beforeSnapshot = new BoardRenderSnapshot([], []);
        var afterSnapshot = new BoardRenderSnapshot(
            [],
            [
                new RenderPiece(new GridPosition(0, 0), PieceVisualConstants.ShapeDiamond, PieceVisualConstants.TintRed, 20f, 20f, 32f, 32f)
            ]);

        controller.QueueBoardSettle(viewState, player, beforeSnapshot, afterSnapshot, 48f);
        player.Update(0.05f);

        var nodeSnapshot = renderer.BuildSnapshot(afterSnapshot, viewState);
        var pieces = controller.BuildPieces(nodeSnapshot, null, viewState);
        var spawnedPiece = Assert.Single(pieces, piece => piece.Position == new GridPosition(0, 0));

        Assert.True(spawnedPiece.Y < 20f);
    }

    [Fact]
    public void GameplayEffectsController_AnimatesCreatedBonus_FromCreationCell_InsteadOfTopSpawn()
    {
        var controller = new GameplayEffectsController();
        var player = new AnimationPlayer();
        var viewState = new BoardViewState();
        var renderer = new PieceNodeRenderer();
        var beforeSnapshot = new BoardRenderSnapshot([], []);
        var afterSnapshot = new BoardRenderSnapshot(
            [],
            [
                new RenderPiece(new GridPosition(3, 2), PieceVisualConstants.ShapeDiamond, PieceVisualConstants.TintRed, 116f, 164f, 32f, 32f)
            ]);

        var createdBonusTargets = new[] { new GridPosition(3, 2) };
        controller.QueueCreatedBonuses(viewState, player, afterSnapshot, 48f, createdBonusOrigins: [new GridPosition(1, 2)]);
        controller.QueueBoardSettle(viewState, player, beforeSnapshot, afterSnapshot, 48f, excludedTargets: createdBonusTargets);
        player.Update(0.45f);

        var nodeSnapshot = renderer.BuildSnapshot(afterSnapshot, viewState);
        var pieces = controller.BuildPieces(nodeSnapshot, null, viewState);
        var bonus = Assert.Single(pieces, piece => piece.Shape == PieceVisualConstants.ShapeDiamond);

        Assert.True(bonus.Y >= 68f);
    }

    private static void AdvanceRuntime(GameplayEffectsController controller, AnimationPlayer player, float totalSeconds, float stepSeconds = 0.05f)
    {
        var remaining = totalSeconds;
        while (remaining > 0f)
        {
            var delta = MathF.Min(stepSeconds, remaining);
            controller.Update(TimeSpan.FromSeconds(delta));
            player.Update(delta);
            remaining -= delta;
        }
    }
}
