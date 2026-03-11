using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Match3.Core.GameCore.Pieces;
using Match3.Core.GameCore.ValueObjects;
using Match3.Core.GameFlow.Sessions;
using Match3.Presentation.Animation;
using Match3.Presentation.Input;
using Match3.Presentation.Rendering;
using Match3.Presentation.UI;

namespace Match3.Tests;

public class Phase13MathInputAndAnimationTests
{
    [Fact]
    public void BoardTransform_ConvertsGridToWorld()
    {
        var transform = new BoardTransform(64f, new Vector2(100f, 50f));

        var world = transform.GridToWorld(new GridPosition(2, 3));

        Assert.Equal(new Vector2(292f, 178f), world);
    }

    [Fact]
    public void BoardTransform_ConvertsWorldToGrid()
    {
        var transform = new BoardTransform(64f, new Vector2(100f, 50f));

        var ok = transform.TryWorldToGrid(new Vector2(292f, 178f), out var grid);

        Assert.True(ok);
        Assert.Equal(new GridPosition(2, 3), grid);
    }

    [Fact]
    public void BoardTransform_PerformsHitTesting()
    {
        var transform = new BoardTransform(64f, new Vector2(0f, 0f));

        var inside = transform.TryWorldToGrid(new Vector2(10f, 10f), out _);
        var outside = transform.TryWorldToGrid(new Vector2(-1f, 10f), out _);

        Assert.True(inside);
        Assert.False(outside);
    }

    [Fact]
    public void BoardRenderer_HasVisualsForFivePieceTypes()
    {
        var renderer = new BoardRenderer();

        var visuals = new[]
        {
            renderer.GetVisual(PieceType.Red),
            renderer.GetVisual(PieceType.Green),
            renderer.GetVisual(PieceType.Blue),
            renderer.GetVisual(PieceType.Yellow),
            renderer.GetVisual(PieceType.Purple)
        };

        Assert.All(visuals, visual => Assert.Equal(PieceVisualConstants.ShapeSquare, visual.Shape));
        Assert.Equal(5, visuals.Select(v => v.Tint).Distinct().Count());
    }

    [Fact]
    public void BoardInputHandler_SelectsFirstCellOnClick()
    {
        var handler = CreateInputHandler();

        var move = handler.HandleClick(new Vector2(10f, 10f));

        Assert.Null(move);
        Assert.Equal(new GridPosition(0, 0), handler.SelectedCell);
    }

    [Fact]
    public void BoardInputHandler_ResetsSelection_WhenSecondClickIsNonAdjacent()
    {
        var handler = CreateInputHandler();
        handler.HandleClick(new Vector2(10f, 10f));

        var move = handler.HandleClick(new Vector2(200f, 200f));

        Assert.Null(move);
        Assert.Null(handler.SelectedCell);
    }

    [Fact]
    public void SwapAnimation_InterpolatesVector2()
    {
        var animation = new SwapAnimation(new Vector2(0f, 0f), new Vector2(10f, 0f));

        var atStart = animation.Evaluate(0f);
        var atMid = animation.Evaluate(0.5f);
        var atEnd = animation.Evaluate(1f);

        Assert.Equal(new Vector2(0f, 0f), atStart);
        Assert.Equal(new Vector2(5f, 0f), atMid);
        Assert.Equal(new Vector2(10f, 0f), atEnd);
    }

    [Fact]
    public void FallAnimation_InterpolatesVector2()
    {
        var animation = new FallAnimation(new Vector2(0f, 0f), new Vector2(0f, 20f));

        var atMid = animation.Evaluate(0.5f);

        Assert.Equal(new Vector2(0f, 10f), atMid);
    }

    [Fact]
    public void SpawnAnimation_MovesFromTopToTarget()
    {
        var animation = new SpawnAnimation(new Vector2(0f, -20f), new Vector2(0f, 0f));

        var atMid = animation.Evaluate(0.5f);

        Assert.Equal(new Vector2(0f, -10f), atMid);
    }

    [Fact]
    public void Easing_SmoothStepProducesMonotonicValues()
    {
        var a = Easing.SmoothStep(0.25f);
        var b = Easing.SmoothStep(0.5f);
        var c = Easing.SmoothStep(0.75f);

        Assert.True(a < b);
        Assert.True(b < c);
    }

    [Fact]
    public void SelectionHighlight_HasDifferentSelectedVisualState()
    {
        var highlight = new SelectionHighlight();

        Assert.True(highlight.GetScale(isSelected: true) > highlight.GetScale(isSelected: false));
        Assert.True(highlight.GetOpacity(isSelected: true) > highlight.GetOpacity(isSelected: false));
    }

    [Fact]
    public void DestroyerAnimation_MovesAlongPath()
    {
        var animation = new DestroyerAnimation(
            new List<Vector2>
            {
                new(0f, 0f),
                new(10f, 0f),
                new(20f, 0f)
            });

        var atStart = animation.Evaluate(0f);
        var atEnd = animation.Evaluate(1f);

        Assert.Equal(new Vector2(0f, 0f), atStart);
        Assert.Equal(new Vector2(20f, 0f), atEnd);
    }

    [Fact]
    public void SelectionHighlight_CreatesMatrixTransform()
    {
        var highlight = new SelectionHighlight();

        var transform = highlight.CreateTransform(new Vector2(100f, 100f), isSelected: true);

        Assert.NotEqual(Matrix3x2.Identity, transform);
    }

    private static BoardInputHandler CreateInputHandler()
    {
        return new BoardInputHandler(
            new BoardTransform(64f, Vector2.Zero),
            new SelectionController());
    }
}
