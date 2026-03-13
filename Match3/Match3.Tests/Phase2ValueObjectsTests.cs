using Match3.Core.GameCore.Bonuses;
using Match3.Core.GameCore.Pieces;
using Match3.Core.GameCore.ValueObjects;

namespace Match3.Tests;

public class Phase2ValueObjectsTests
{
    [Fact]
    public void GridPosition_CreatesWithCoordinates()
    {
        var position = new GridPosition(2, 5);

        Assert.Equal(2, position.Row);
        Assert.Equal(5, position.Column);
    }

    [Fact]
    public void Move_CreatesWithFromAndToPositions()
    {
        var from = new GridPosition(1, 1);
        var to = new GridPosition(1, 2);

        var move = new Move(from, to);

        Assert.Equal(from, move.From);
        Assert.Equal(to, move.To);
    }

    [Fact]
    public void PieceColor_DefinesColorValues()
    {
        Assert.Contains(PieceColor.Red, Enum.GetValues<PieceColor>());
        Assert.Contains(PieceColor.Green, Enum.GetValues<PieceColor>());
        Assert.Contains(PieceColor.Blue, Enum.GetValues<PieceColor>());
    }

    [Fact]
    public void BonusKind_DefinesBasicKinds()
    {
        Assert.Contains(BonusKind.None, Enum.GetValues<BonusKind>());
        Assert.Contains(BonusKind.Line, Enum.GetValues<BonusKind>());
        Assert.Contains(BonusKind.Bomb, Enum.GetValues<BonusKind>());
    }
}
