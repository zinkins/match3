using Match3.Core.GameCore.ValueObjects;
using Match3.Presentation.Animation;
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
        var transform = new BoardTransform(48f, new System.Numerics.Vector2(20f, 20f));
        var snapshot = new BoardRenderSnapshot(
            [],
            [
                new RenderPiece(new GridPosition(0, 0), PieceVisualConstants.ShapeSquare, PieceVisualConstants.TintRed, 20f, 20f, 32f, 32f)
            ]);

        controller.QueueDestroyer(new GridPosition(0, 1), [new GridPosition(0, 0), new GridPosition(0, 1), new GridPosition(0, 2)], transform);
        controller.Update(TimeSpan.FromMilliseconds(50));

        var pieces = controller.BuildPieces(snapshot, null);

        Assert.Contains(pieces, piece => piece.Shape == PieceVisualConstants.ShapeDiamond && piece.Tint == PieceVisualConstants.TintWhite);
    }

    [Fact]
    public void GameplayEffectsController_QueuesTwoDestroyers_FromBonusCenter()
    {
        var controller = new GameplayEffectsController();
        var transform = new BoardTransform(48f, new System.Numerics.Vector2(20f, 20f));
        var snapshot = new BoardRenderSnapshot([], []);

        controller.QueueDestroyer(
            new GridPosition(0, 2),
            [new GridPosition(0, 0), new GridPosition(0, 1), new GridPosition(0, 2), new GridPosition(0, 3), new GridPosition(0, 4)],
            transform);
        controller.Update(TimeSpan.FromMilliseconds(50));

        var pieces = controller.BuildPieces(snapshot, null);

        Assert.Equal(2, pieces.Count(piece => piece.Shape == PieceVisualConstants.ShapeDiamond && piece.Tint == PieceVisualConstants.TintWhite));
    }

    [Fact]
    public void GameplayEffectsController_HidesPiecesInsideExplosionArea_WhileBombEffectRuns()
    {
        var controller = new GameplayEffectsController();
        var transform = new BoardTransform(48f, new System.Numerics.Vector2(20f, 20f));
        var affected = new GridPosition(2, 2);
        var unaffected = new GridPosition(0, 0);
        var snapshot = new BoardRenderSnapshot(
            [],
            [
                new RenderPiece(unaffected, PieceVisualConstants.ShapeSquare, PieceVisualConstants.TintRed, 20f, 20f, 32f, 32f),
                new RenderPiece(affected, PieceVisualConstants.ShapeSquare, PieceVisualConstants.TintBlue, 116f, 116f, 32f, 32f)
            ]);

        controller.QueueExplosion([affected], transform);
        controller.Update(TimeSpan.FromMilliseconds(50));

        var pieces = controller.BuildPieces(snapshot, null);

        Assert.DoesNotContain(pieces, piece => piece.Position == affected);
        Assert.Contains(pieces, piece => piece.Position == unaffected);
    }

    [Fact]
    public void GameplayEffectsController_ClearsDestroyerPathProgressively()
    {
        var controller = new GameplayEffectsController();
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

        controller.QueueDestroyer(origin, [new GridPosition(0, 0), origin, mid, tail], transform);
        controller.Update(TimeSpan.FromMilliseconds(50));

        var earlyPieces = controller.BuildPieces(snapshot, null);

        Assert.DoesNotContain(earlyPieces, piece => piece.Position == origin);
        Assert.Contains(earlyPieces, piece => piece.Position == mid);
        Assert.Contains(earlyPieces, piece => piece.Position == tail);

        controller.Update(TimeSpan.FromMilliseconds(550));

        var latePieces = controller.BuildPieces(snapshot, null);

        Assert.DoesNotContain(latePieces, piece => piece.Position == mid);
        Assert.Contains(latePieces, piece => piece.Position == tail);
    }

    [Fact]
    public void GameplayEffectsController_DoesNotHideDelayedSettleTargets_BeforeSettleStarts()
    {
        var controller = new GameplayEffectsController();
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

        controller.QueueBoardSettle(beforeSnapshot, afterSnapshot, 48f, initialDelaySeconds: 0.8f);
        controller.Update(TimeSpan.FromMilliseconds(50));

        var pieces = controller.BuildPieces(afterSnapshot, null);

        Assert.Contains(pieces, piece => piece.Position == new GridPosition(1, 0));
    }
}
