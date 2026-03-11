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
}
