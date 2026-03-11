using Match3.Core.GameCore.Board;
using Match3.Core.GameCore.Pieces;
using Match3.Core.GameCore.ValueObjects;

namespace Match3.Tests;

public class Phase3BoardAndGenerationTests
{
    [Fact]
    public void PieceCatalog_ContainsExactlyFivePieceTypes()
    {
        Assert.Equal(5, PieceCatalog.All.Count);
    }

    [Fact]
    public void PieceType_HasColor()
    {
        foreach (var pieceType in PieceCatalog.All)
        {
            var color = PieceCatalog.GetColor(pieceType);
            Assert.Contains(color, Enum.GetValues<PieceColor>());
        }
    }

    [Fact]
    public void BoardState_HasWidthEight()
    {
        var board = new BoardState();
        Assert.Equal(8, board.Width);
    }

    [Fact]
    public void BoardState_HasHeightEight()
    {
        var board = new BoardState();
        Assert.Equal(8, board.Height);
    }

    [Fact]
    public void BoardState_ReadsEmptyCellAsNull()
    {
        var board = new BoardState();
        var value = board.GetPiece(new GridPosition(0, 0));
        Assert.Null(value);
    }

    [Fact]
    public void BoardState_WritesPieceIntoCell()
    {
        var board = new BoardState();
        var position = new GridPosition(2, 3);
        board.SetPiece(position, PieceType.Blue);
        Assert.Equal(PieceType.Blue, board.GetPiece(position));
    }

    [Fact]
    public void BoardState_ThrowsWhenPositionIsOutOfBounds()
    {
        var board = new BoardState();
        Assert.Throws<ArgumentOutOfRangeException>(() => board.GetPiece(new GridPosition(-1, 0)));
        Assert.Throws<ArgumentOutOfRangeException>(() => board.GetPiece(new GridPosition(8, 0)));
        Assert.Throws<ArgumentOutOfRangeException>(() => board.SetPiece(new GridPosition(0, 8), PieceType.Red));
    }

    [Fact]
    public void BoardState_AlwaysUses8x8Invariant()
    {
        var board = new BoardState();
        Assert.Equal(8, board.Width);
        Assert.Equal(8, board.Height);
    }

    [Fact]
    public void BoardGenerator_FillsEveryCell()
    {
        var boardGenerator = new BoardGenerator(new SequenceRandomSource(0, 1, 2, 3, 4));
        var board = boardGenerator.Generate();

        for (var row = 0; row < board.Height; row++)
        {
            for (var column = 0; column < board.Width; column++)
            {
                Assert.NotNull(board.GetPiece(new GridPosition(row, column)));
            }
        }
    }

    [Fact]
    public void BoardGenerator_UsesRandomPieceTypes()
    {
        var boardGenerator = new BoardGenerator(new SequenceRandomSource(0, 1, 2, 3, 4));
        var board = boardGenerator.Generate();

        var types = new HashSet<PieceType>();
        for (var row = 0; row < board.Height; row++)
        {
            for (var column = 0; column < board.Width; column++)
            {
                var value = board.GetPiece(new GridPosition(row, column));
                Assert.NotNull(value);
                types.Add(value!.Value);
            }
        }

        Assert.True(types.Count > 1);
    }

    [Fact]
    public void BoardGenerator_DoesNotCreateStartingMatches()
    {
        var boardGenerator = new BoardGenerator(new SequenceRandomSource(0, 1, 2, 3, 4));
        var board = boardGenerator.Generate();
        var matchFinder = new MatchFinder();

        var matches = matchFinder.FindMatches(board);

        Assert.Empty(matches);
    }

    [Fact]
    public void RefillResolver_UsesRandomPieceTypes()
    {
        var board = new BoardState();
        for (var row = 0; row < board.Height; row++)
        {
            for (var column = 0; column < board.Width; column++)
            {
                board.SetPiece(new GridPosition(row, column), PieceType.Purple);
            }
        }

        board.SetPiece(new GridPosition(0, 0), null);
        board.SetPiece(new GridPosition(1, 0), null);

        var refillResolver = new RefillResolver(new SequenceRandomSource(0, 1));
        refillResolver.Refill(board);

        Assert.Equal(PieceType.Red, board.GetPiece(new GridPosition(0, 0)));
        Assert.Equal(PieceType.Green, board.GetPiece(new GridPosition(1, 0)));
    }

    [Fact]
    public void RefillResolver_DoesNotCreateImmediateMatches()
    {
        var board = new BoardState();
        for (var row = 0; row < board.Height; row++)
        {
            for (var column = 0; column < board.Width; column++)
            {
                board.SetPiece(new GridPosition(row, column), PieceCatalog.All[(row + column) % PieceCatalog.All.Count]);
            }
        }

        board.SetPiece(new GridPosition(0, 0), null);
        board.SetPiece(new GridPosition(0, 1), null);
        board.SetPiece(new GridPosition(0, 2), null);

        var refillResolver = new RefillResolver(new SequenceRandomSource(0));
        refillResolver.Refill(board);

        var matches = new MatchFinder().FindMatches(board);

        Assert.Empty(matches);
    }

    private sealed class SequenceRandomSource(params int[] values) : IRandomSource
    {
        private readonly int[] values = values.Length == 0 ? [0] : values;
        private int index;

        public int Next(int minInclusive, int maxExclusive)
        {
            var value = values[index % values.Length];
            index++;
            var range = maxExclusive - minInclusive;
            return minInclusive + (value % range);
        }
    }
}
