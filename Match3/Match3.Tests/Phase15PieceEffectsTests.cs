using Match3.Core.GameCore.ValueObjects;
using Match3.Core.GameFlow.Events;
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
    public void GameplayAnimationRuntime_QueuesDestroyerVisualOverlay()
    {
        var visualState = new GameplayVisualState();
        var player = new AnimationPlayer();
        var viewState = new BoardViewState();
        var transform = new BoardTransform(48f, new System.Numerics.Vector2(20f, 20f));
        var snapshot = new BoardRenderSnapshot(
            [],
            [
                new RenderPiece(new GridPosition(0, 0), PieceVisualConstants.ShapeSquare, PieceVisualConstants.TintRed, 20f, 20f, 32f, 32f)
            ]);

        GameplayAnimationRuntime.QueueDestroyer(viewState, player, new GridPosition(0, 1), [new GridPosition(0, 0), new GridPosition(0, 1), new GridPosition(0, 2)], transform);
        player.Update(0.05f);

        var pieces = visualState.BuildPieces(snapshot, null, viewState);

        Assert.Contains(pieces, piece => piece.Shape == PieceVisualConstants.ShapeDiamond && piece.Tint == PieceVisualConstants.TintWhite);
    }

    [Fact]
    public void GameplayAnimationRuntime_QueuesTwoDestroyers_FromBonusCenter()
    {
        var visualState = new GameplayVisualState();
        var player = new AnimationPlayer();
        var viewState = new BoardViewState();
        var transform = new BoardTransform(48f, new System.Numerics.Vector2(20f, 20f));
        var snapshot = new BoardRenderSnapshot([], []);

        GameplayAnimationRuntime.QueueDestroyer(
            viewState,
            player,
            new GridPosition(0, 2),
            [new GridPosition(0, 0), new GridPosition(0, 1), new GridPosition(0, 2), new GridPosition(0, 3), new GridPosition(0, 4)],
            transform);
        player.Update(0.05f);

        var pieces = visualState.BuildPieces(snapshot, null, viewState);

        Assert.Equal(2, pieces.Count(piece => piece.Shape == PieceVisualConstants.ShapeDiamond && piece.Tint == PieceVisualConstants.TintWhite));
    }

    [Fact]
    public void GameplayAnimationRuntime_UsesSameSpeed_ForBothDestroyerBranches()
    {
        var visualState = new GameplayVisualState();
        var player = new AnimationPlayer();
        var viewState = new BoardViewState();
        var transform = new BoardTransform(48f, new System.Numerics.Vector2(20f, 20f));
        var snapshot = new BoardRenderSnapshot([], []);

        GameplayAnimationRuntime.QueueDestroyer(
            viewState,
            player,
            new GridPosition(0, 2),
            [new GridPosition(0, 0), new GridPosition(0, 1), new GridPosition(0, 2), new GridPosition(0, 3), new GridPosition(0, 4)],
            transform);

        AdvanceRuntime(player, 0.2f, stepSeconds: 0.2f);
        var pieces = visualState.BuildPieces(snapshot, null, viewState)
            .Where(piece => piece.Shape == PieceVisualConstants.ShapeDiamond)
            .OrderBy(piece => piece.X)
            .ToArray();

        Assert.Equal(2, pieces.Length);
        Assert.Equal(96f, pieces[1].X - pieces[0].X, 0.01f);
    }

    [Fact]
    public void GameplayAnimationRuntime_HidesPiecesInsideExplosionArea_WhileBombEffectRuns()
    {
        var visualState = new GameplayVisualState();
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

        GameplayAnimationRuntime.QueueExplosion(viewState, player, [affected], transform);
        player.Update(0.05f);

        var pieces = visualState.BuildPieces(snapshot, null, viewState);

        Assert.DoesNotContain(pieces, piece => piece.Position == affected);
        Assert.Contains(pieces, piece => piece.Position == unaffected);
    }

    [Fact]
    public void GameplayAnimationRuntime_QueuesPopEffect_ForConsumedMatchPieces()
    {
        var visualState = new GameplayVisualState();
        var player = new AnimationPlayer();
        var viewState = new BoardViewState();
        var consumed = new GridPosition(0, 0);
        var survivor = new GridPosition(1, 0);
        var beforeSnapshot = new BoardRenderSnapshot(
            [],
            [
                new RenderPiece(consumed, PieceVisualConstants.ShapeSquare, PieceVisualConstants.TintRed, 20f, 20f, 32f, 32f),
                new RenderPiece(survivor, PieceVisualConstants.ShapeSquare, PieceVisualConstants.TintBlue, 68f, 20f, 32f, 32f)
            ]);
        var afterSnapshot = new BoardRenderSnapshot(
            [],
            [
                new RenderPiece(survivor, PieceVisualConstants.ShapeSquare, PieceVisualConstants.TintBlue, 68f, 20f, 32f, 32f)
            ]);

        GameplayAnimationRuntime.QueueMatchPop(viewState, player, beforeSnapshot, afterSnapshot);

        Assert.Single(viewState.EffectNodes);

        player.Update(0.05f);
        var earlyPieces = visualState.BuildPieces(afterSnapshot, null, viewState);
        var poppedPiece = Assert.Single(earlyPieces, piece => piece.Position == consumed);

        Assert.True(poppedPiece.Width > 0f);
        Assert.Equal(PieceVisualConstants.TintRed, poppedPiece.Tint);

        player.Update(0.20f);

        Assert.Empty(viewState.EffectNodes);
        var finalPieces = visualState.BuildPieces(afterSnapshot, null, viewState);
        Assert.DoesNotContain(finalPieces, piece => piece.Position == consumed);
    }

    [Fact]
    public void GameplayAnimationRuntime_DelaysPopEffect_UntilDestroyerReachesEachCell()
    {
        var visualState = new GameplayVisualState();
        var player = new AnimationPlayer();
        var viewState = new BoardViewState();
        var origin = new GridPosition(0, 0);
        var tail = new GridPosition(0, 2);
        var beforeSnapshot = new BoardRenderSnapshot(
            [],
            [
                new RenderPiece(origin, PieceVisualConstants.ShapeSquare, PieceVisualConstants.TintRed, 20f, 20f, 32f, 32f),
                new RenderPiece(tail, PieceVisualConstants.ShapeSquare, PieceVisualConstants.TintBlue, 116f, 20f, 32f, 32f)
            ]);
        var afterSnapshot = new BoardRenderSnapshot([], []);
        IDomainEvent[] events =
        [
            new DestroyerSpawned(origin, [origin, new GridPosition(0, 1), tail])
        ];

        GameplayAnimationRuntime.QueueMatchPop(viewState, player, beforeSnapshot, afterSnapshot, events);

        var initialPieces = visualState.BuildPieces(afterSnapshot, null, viewState);
        Assert.Contains(initialPieces, piece => piece.Position == origin);
        Assert.Contains(initialPieces, piece => piece.Position == tail);

        player.Update(0.2f);
        var midPieces = visualState.BuildPieces(afterSnapshot, null, viewState);
        Assert.DoesNotContain(midPieces, piece => piece.Position == origin);
        Assert.Contains(midPieces, piece => piece.Position == tail);

        player.Update(0.8f);
        var finalPieces = visualState.BuildPieces(afterSnapshot, null, viewState);
        Assert.DoesNotContain(finalPieces, piece => piece.Position == tail);
    }

    [Fact]
    public void GameplayAnimationRuntime_MovesDestroyerTransientNode_AlongPath()
    {
        var visualState = new GameplayVisualState();
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

        GameplayAnimationRuntime.QueueDestroyer(viewState, player, origin, [new GridPosition(0, 0), origin, mid, tail], transform);
        player.Update(0.05f);

        var earlyPieces = visualState.BuildPieces(snapshot, null, viewState);
        var earlyDestroyerX = earlyPieces
            .Where(piece => piece.Shape == PieceVisualConstants.ShapeDiamond)
            .Max(piece => piece.X);

        AdvanceRuntime(player, 0.15f);

        var latePieces = visualState.BuildPieces(snapshot, null, viewState);
        var lateDestroyerX = latePieces
            .Where(piece => piece.Shape == PieceVisualConstants.ShapeDiamond)
            .Max(piece => piece.X);

        Assert.True(lateDestroyerX > earlyDestroyerX);
    }

    [Fact]
    public void GameplayAnimationRuntime_MovesDestroyerSmoothly_ThroughSegmentBoundaries()
    {
        var visualState = new GameplayVisualState();
        var player = new AnimationPlayer();
        var viewState = new BoardViewState();
        var transform = new BoardTransform(48f, new System.Numerics.Vector2(20f, 20f));
        var snapshot = new BoardRenderSnapshot([], []);

        GameplayAnimationRuntime.QueueDestroyer(viewState, player, new GridPosition(0, 0), [new GridPosition(0, 0), new GridPosition(0, 1), new GridPosition(0, 2), new GridPosition(0, 3)], transform);
        AdvanceRuntime(player, 0.10f, stepSeconds: 0.10f);
        var beforeBoundaryX = visualState.BuildPieces(snapshot, null, viewState)
            .Single(piece => piece.Shape == PieceVisualConstants.ShapeDiamond)
            .X;

        AdvanceRuntime(player, 0.40f, stepSeconds: 0.40f);
        var afterBoundaryX = visualState.BuildPieces(snapshot, null, viewState)
            .Single(piece => piece.Shape == PieceVisualConstants.ShapeDiamond)
            .X;

        Assert.True(afterBoundaryX > beforeBoundaryX);
    }

    [Fact]
    public void GameplayAnimationRuntime_DoesNotHideDelayedSettleTargets_BeforeSettleStarts()
    {
        var visualState = new GameplayVisualState();
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

        GameplayAnimationRuntime.QueueBoardSettle(viewState, player, beforeSnapshot, afterSnapshot, 48f, initialDelaySeconds: 0.8f, visualState: visualState);

        var nodeSnapshot = renderer.BuildSnapshot(afterSnapshot, viewState);
        var pieces = visualState.BuildPieces(nodeSnapshot, null, viewState);
        var movedPiece = Assert.Single(pieces, piece => piece.Position == new GridPosition(1, 0));

        Assert.Equal(20f, movedPiece.Y);
    }

    [Fact]
    public void GameplayAnimationRuntime_DoesNotAnimatePiecesUpward_DuringSettle()
    {
        var visualState = new GameplayVisualState();
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

        GameplayAnimationRuntime.QueueBoardSettle(viewState, player, beforeSnapshot, afterSnapshot, 48f, visualState: visualState);
        player.Update(0.05f);

        var nodeSnapshot = renderer.BuildSnapshot(afterSnapshot, viewState);
        var pieces = visualState.BuildPieces(nodeSnapshot, null, viewState);
        var movedBluePiece = Assert.Single(pieces, piece => piece.Position == new GridPosition(2, 0) && piece.Tint == PieceVisualConstants.TintBlue);

        Assert.True(movedBluePiece.Y <= 116f);
    }

    [Fact]
    public void GameplayAnimationRuntime_SpawnsPiecesAboveBoard_BeforeSpawnAnimationStarts()
    {
        var visualState = new GameplayVisualState();
        var player = new AnimationPlayer();
        var viewState = new BoardViewState();
        var renderer = new PieceNodeRenderer();
        var beforeSnapshot = new BoardRenderSnapshot([], []);
        var afterSnapshot = new BoardRenderSnapshot(
            [],
            [
                new RenderPiece(new GridPosition(0, 0), PieceVisualConstants.ShapeDiamond, PieceVisualConstants.TintRed, 20f, 20f, 32f, 32f)
            ]);

        GameplayAnimationRuntime.QueueBoardSettle(viewState, player, beforeSnapshot, afterSnapshot, 48f, visualState: visualState);
        player.Update(0.05f);

        var nodeSnapshot = renderer.BuildSnapshot(afterSnapshot, viewState);
        var pieces = visualState.BuildPieces(nodeSnapshot, null, viewState);
        var spawnedPiece = Assert.Single(pieces, piece => piece.Position == new GridPosition(0, 0));

        Assert.True(spawnedPiece.Y < 20f);
    }

    [Fact]
    public void GameplayAnimationRuntime_AnimatesCreatedBonus_FromCreationCell_InsteadOfTopSpawn()
    {
        var visualState = new GameplayVisualState();
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
        GameplayAnimationRuntime.QueueCreatedBonuses(viewState, player, afterSnapshot, 48f, createdBonusOrigins: [new GridPosition(1, 2)]);
        GameplayAnimationRuntime.QueueBoardSettle(viewState, player, beforeSnapshot, afterSnapshot, 48f, excludedTargets: createdBonusTargets, visualState: visualState);
        player.Update(0.45f);

        var nodeSnapshot = renderer.BuildSnapshot(afterSnapshot, viewState);
        var pieces = visualState.BuildPieces(nodeSnapshot, null, viewState);
        var bonus = Assert.Single(pieces, piece => piece.Shape == PieceVisualConstants.ShapeDiamond);

        Assert.True(bonus.Y >= 68f);
    }

    private static void AdvanceRuntime(AnimationPlayer player, float totalSeconds, float stepSeconds = 0.05f)
    {
        var remaining = totalSeconds;
        while (remaining > 0f)
        {
            var delta = MathF.Min(stepSeconds, remaining);
            player.Update(delta);
            remaining -= delta;
        }
    }
}
