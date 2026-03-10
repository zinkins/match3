using Match3.Core.GameCore.Board;
using Match3.Core.GameCore.Pieces;
using Match3.Core.GameCore.ValueObjects;

namespace Match3.Tests;

public class Phase5MatchFinderTests
{
    [Fact]
    public void MatchFinder_FindsHorizontalMatchOfThree()
    {
        var board = CreateCheckerBoard();
        board.SetCell(new GridPosition(2, 1), PieceType.Red);
        board.SetCell(new GridPosition(2, 2), PieceType.Red);
        board.SetCell(new GridPosition(2, 3), PieceType.Red);

        var finder = new MatchFinder();
        var groups = finder.FindMatches(board);

        Assert.Single(groups);
        Assert.Equal(3, groups[0].Positions.Count);
        Assert.All(groups[0].Positions, p => Assert.Equal(2, p.Row));
    }

    [Fact]
    public void MatchFinder_FindsVerticalMatchOfThree()
    {
        var board = CreateCheckerBoard();
        board.SetCell(new GridPosition(1, 4), PieceType.Green);
        board.SetCell(new GridPosition(2, 4), PieceType.Green);
        board.SetCell(new GridPosition(3, 4), PieceType.Green);

        var finder = new MatchFinder();
        var groups = finder.FindMatches(board);

        Assert.Single(groups);
        Assert.Equal(3, groups[0].Positions.Count);
        Assert.All(groups[0].Positions, p => Assert.Equal(4, p.Column));
    }

    [Fact]
    public void MatchFinder_DoesNotReturnMatch_WhenNoSequenceExists()
    {
        var board = CreateCheckerBoard();
        var finder = new MatchFinder();

        var groups = finder.FindMatches(board);

        Assert.Empty(groups);
    }

    [Fact]
    public void MatchFinder_ReturnsMultipleMatchGroups()
    {
        var board = CreateCheckerBoard();
        board.SetCell(new GridPosition(0, 0), PieceType.Blue);
        board.SetCell(new GridPosition(0, 1), PieceType.Blue);
        board.SetCell(new GridPosition(0, 2), PieceType.Blue);

        board.SetCell(new GridPosition(4, 7), PieceType.Yellow);
        board.SetCell(new GridPosition(5, 7), PieceType.Yellow);
        board.SetCell(new GridPosition(6, 7), PieceType.Yellow);

        var finder = new MatchFinder();
        var groups = finder.FindMatches(board);

        Assert.Equal(2, groups.Count);
    }

    private static BoardState CreateCheckerBoard()
    {
        var board = new BoardState();
        var types = PieceCatalog.All;

        for (var row = 0; row < board.Height; row++)
        {
            for (var column = 0; column < board.Width; column++)
            {
                var index = (row + column) % types.Count;
                board.SetCell(new GridPosition(row, column), types[index]);
            }
        }

        return board;
    }
}
