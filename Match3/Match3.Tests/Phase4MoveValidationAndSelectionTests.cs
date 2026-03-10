using Match3.Core.GameCore.Board;
using Match3.Core.GameCore.ValueObjects;
using Match3.Core.GameFlow.Sessions;

namespace Match3.Tests;

public class Phase4MoveValidationAndSelectionTests
{
    [Fact]
    public void GridPosition_IsAdjacentTo_HorizontalNeighbor()
    {
        var left = new GridPosition(2, 3);
        var right = new GridPosition(2, 4);

        Assert.True(left.IsAdjacentTo(right));
    }

    [Fact]
    public void GridPosition_IsAdjacentTo_VerticalNeighbor()
    {
        var top = new GridPosition(2, 3);
        var bottom = new GridPosition(3, 3);

        Assert.True(top.IsAdjacentTo(bottom));
    }

    [Fact]
    public void GridPosition_IsAdjacentTo_DiagonalCell_ReturnsFalse()
    {
        var a = new GridPosition(2, 3);
        var b = new GridPosition(3, 4);

        Assert.False(a.IsAdjacentTo(b));
    }

    [Fact]
    public void MoveValidator_AllowsAdjacentSwap()
    {
        var validator = new MoveValidator();
        var move = new Move(new GridPosition(0, 0), new GridPosition(0, 1));

        Assert.True(validator.IsValid(move));
    }

    [Fact]
    public void MoveValidator_RejectsNonAdjacentSwap()
    {
        var validator = new MoveValidator();
        var move = new Move(new GridPosition(0, 0), new GridPosition(0, 2));

        Assert.False(validator.IsValid(move));
    }

    [Fact]
    public void MoveValidator_RejectsSameCellSwap()
    {
        var validator = new MoveValidator();
        var position = new GridPosition(1, 1);
        var move = new Move(position, position);

        Assert.False(validator.IsValid(move));
    }

    [Fact]
    public void SelectionController_StoresFirstSelectedCell()
    {
        var controller = new SelectionController();
        var first = new GridPosition(2, 2);

        var move = controller.Select(first);

        Assert.Null(move);
        Assert.Equal(first, controller.SelectedCell);
    }

    [Fact]
    public void SelectionController_ResetsSelection_WhenSecondCellIsNotAdjacent()
    {
        var controller = new SelectionController();
        controller.Select(new GridPosition(2, 2));

        var move = controller.Select(new GridPosition(2, 5));

        Assert.Null(move);
        Assert.Null(controller.SelectedCell);
    }

    [Fact]
    public void SelectionController_CreatesMove_WhenSecondCellIsAdjacent()
    {
        var controller = new SelectionController();
        var first = new GridPosition(2, 2);
        var second = new GridPosition(2, 3);

        controller.Select(first);
        var move = controller.Select(second);

        Assert.NotNull(move);
        Assert.Equal(new Move(first, second), move!.Value);
        Assert.Null(controller.SelectedCell);
    }
}
