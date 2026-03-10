using Match3.Core.GameCore.Board;
using Match3.Core.GameCore.Pieces;
using Match3.Core.GameCore.ValueObjects;
using Match3.Core.GameFlow.Pipeline;

namespace Match3.Tests;

public class Phase6TurnProcessorTests
{
    [Fact]
    public void TurnProcessor_PerformsSwap_WhenMatchExists()
    {
        var board = CreateBoardForSwapWithMatch();
        var move = new Move(new GridPosition(0, 2), new GridPosition(1, 2));
        var processor = new TurnProcessor();

        var applied = processor.TryProcessMove(board, move);

        Assert.True(applied);
        Assert.Equal(PieceType.Red, board.GetCell(move.From));
        Assert.Equal(PieceType.Blue, board.GetCell(move.To));
    }

    [Fact]
    public void TurnProcessor_RevertsSwap_WhenNoMatchExists()
    {
        var board = CreateBoardForSwapWithoutMatch();
        var move = new Move(new GridPosition(0, 0), new GridPosition(0, 1));
        var processor = new TurnProcessor();

        var applied = processor.TryProcessMove(board, move);

        Assert.False(applied);
        Assert.Equal(PieceType.Red, board.GetCell(move.From));
        Assert.Equal(PieceType.Green, board.GetCell(move.To));
    }

    [Fact]
    public void TurnProcessor_RestoresBoardState_WhenSwapIsReverted()
    {
        var board = CreateBoardForSwapWithoutMatch();
        var before = Snapshot(board);
        var move = new Move(new GridPosition(0, 0), new GridPosition(0, 1));
        var processor = new TurnProcessor();

        var applied = processor.TryProcessMove(board, move);

        Assert.False(applied);
        var after = Snapshot(board);
        Assert.Equal(before, after);
    }

    private static BoardState CreateBoardForSwapWithMatch()
    {
        var board = CreateCheckerBoard();
        board.SetCell(new GridPosition(0, 0), PieceType.Red);
        board.SetCell(new GridPosition(0, 1), PieceType.Red);
        board.SetCell(new GridPosition(0, 2), PieceType.Blue);
        board.SetCell(new GridPosition(1, 2), PieceType.Red);
        return board;
    }

    private static BoardState CreateBoardForSwapWithoutMatch()
    {
        var board = CreateCheckerBoard();
        board.SetCell(new GridPosition(0, 0), PieceType.Red);
        board.SetCell(new GridPosition(0, 1), PieceType.Green);
        return board;
    }

    private static string Snapshot(BoardState board)
    {
        var parts = new List<string>();
        for (var row = 0; row < board.Height; row++)
        {
            for (var column = 0; column < board.Width; column++)
            {
                parts.Add(board.GetCell(new GridPosition(row, column))?.ToString() ?? "null");
            }
        }

        return string.Join("|", parts);
    }

    private static BoardState CreateCheckerBoard()
    {
        var board = new BoardState();
        var types = PieceCatalog.All;

        for (var row = 0; row < board.Height; row++)
        {
            for (var column = 0; column < board.Width; column++)
            {
                board.SetCell(new GridPosition(row, column), types[(row + column) % types.Count]);
            }
        }

        return board;
    }
}
