using Match3.Core.GameCore.Board;
using Match3.Core.GameCore.Pieces;
using Match3.Core.GameCore.ValueObjects;
using Match3.Core.GameFlow.Pipeline;

namespace Match3.Tests;

public class Phase7GravityRefillAndCascadeTests
{
    [Fact]
    public void GravityResolver_DropsPieceIntoEmptyCell()
    {
        var board = CreateFilledBoard(PieceType.Purple);
        for (var row = 0; row < board.Height; row++)
        {
            board.SetCell(new GridPosition(row, 0), null);
        }

        board.SetCell(new GridPosition(6, 0), PieceType.Red);
        board.SetCell(new GridPosition(7, 0), null);

        var gravity = new GravityResolver();
        gravity.Apply(board);

        Assert.Null(board.GetCell(new GridPosition(6, 0)));
        Assert.Equal(PieceType.Red, board.GetCell(new GridPosition(7, 0)));
    }

    [Fact]
    public void GravityResolver_DropsMultiplePiecesInColumn()
    {
        var board = CreateFilledBoard(PieceType.Purple);
        for (var row = 0; row < board.Height; row++)
        {
            board.SetCell(new GridPosition(row, 0), null);
        }

        board.SetCell(new GridPosition(0, 0), PieceType.Red);
        board.SetCell(new GridPosition(3, 0), PieceType.Green);
        board.SetCell(new GridPosition(6, 0), PieceType.Blue);

        var gravity = new GravityResolver();
        gravity.Apply(board);

        Assert.Null(board.GetCell(new GridPosition(4, 0)));
        Assert.Equal(PieceType.Red, board.GetCell(new GridPosition(5, 0)));
        Assert.Equal(PieceType.Green, board.GetCell(new GridPosition(6, 0)));
        Assert.Equal(PieceType.Blue, board.GetCell(new GridPosition(7, 0)));
    }

    [Fact]
    public void RefillResolver_FillsTopEmptyCells()
    {
        var board = CreateFilledBoard(PieceType.Purple);
        board.SetCell(new GridPosition(0, 2), null);
        board.SetCell(new GridPosition(1, 2), null);

        var refill = new RefillResolver(new SequenceRandomSource(0, 1));
        refill.Refill(board);

        Assert.Equal(PieceType.Red, board.GetCell(new GridPosition(0, 2)));
        Assert.Equal(PieceType.Green, board.GetCell(new GridPosition(1, 2)));
    }

    [Fact]
    public void TurnProcessor_RechecksBoardAfterGravityAndRefill()
    {
        var board = CreateFilledBoard(PieceType.Purple);
        board.SetCell(new GridPosition(0, 0), null);
        board.SetCell(new GridPosition(1, 0), null);
        board.SetCell(new GridPosition(2, 0), null);

        var turnProcessor = new TurnProcessor(
            matchFinder: new MatchFinder(),
            gravityResolver: new GravityResolver(),
            refillResolver: new RefillResolver(new SequenceRandomSource(0, 0, 0)));

        var hasCascade = turnProcessor.RecheckAfterGravityAndRefill(board);

        Assert.True(hasCascade);
    }

    private static BoardState CreateFilledBoard(PieceType type)
    {
        var board = new BoardState();
        for (var row = 0; row < board.Height; row++)
        {
            for (var column = 0; column < board.Width; column++)
            {
                board.SetCell(new GridPosition(row, column), type);
            }
        }

        return board;
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
