using Match3.Core.GameCore.Board;
using Match3.Core.GameCore.Pieces;
using Match3.Core.GameCore.ValueObjects;
using Match3.Core.GameFlow.Events;
using Match3.Core.GameFlow.Pipeline;
using Match3.Core.GameFlow.Sessions;
using Match3.Core.GameFlow.StateMachine;

namespace Match3.Tests;

public class Phase10DomainEventsTests
{
    [Fact]
    public void TurnProcessor_ReturnsPiecesSwappedEvent()
    {
        var result = ExecutePipeline(CreateBoardForSwapWithMatch(), CreateMatchMove(), new GameSession());

        Assert.Contains(result.Events, e => e is PiecesSwapped);
    }

    [Fact]
    public void TurnProcessor_ReturnsSwapRevertedEvent()
    {
        var board = CreateBoardForSwapWithoutMatch();
        var move = new Move(new GridPosition(0, 0), new GridPosition(0, 1));
        var result = ExecutePipeline(board, move, new GameSession());

        Assert.Contains(result.Events, e => e is SwapReverted);
    }

    [Fact]
    public void TurnProcessor_DoesNotReturnGravityOrSpawnEvents_WhenSwapIsReverted()
    {
        var board = CreateBoardForSwapWithoutMatch();
        var move = new Move(new GridPosition(0, 0), new GridPosition(0, 1));
        var result = ExecutePipeline(board, move, new GameSession());

        Assert.DoesNotContain(result.Events, e => e is PiecesFell);
        Assert.DoesNotContain(result.Events, e => e is PiecesSpawned);
    }

    [Fact]
    public void TurnProcessor_ReturnsMatchResolvedEvent()
    {
        var result = ExecutePipeline(CreateBoardForSwapWithMatch(), CreateMatchMove(), new GameSession());

        Assert.Contains(result.Events, e => e is MatchResolved);
    }

    [Fact]
    public void TurnProcessor_ReturnsPiecesFellEvent()
    {
        var result = ExecutePipeline(CreateBoardForSwapWithMatch(), CreateMatchMove(), new GameSession());

        Assert.Contains(result.Events, e => e is PiecesFell);
    }

    [Fact]
    public void TurnProcessor_ReturnsPiecesSpawnedEvent()
    {
        var result = ExecutePipeline(CreateBoardForSwapWithMatch(), CreateMatchMove(), new GameSession());

        Assert.Contains(result.Events, e => e is PiecesSpawned);
    }

    [Fact]
    public void TurnProcessor_ReturnsScoreAddedEvent()
    {
        var result = ExecutePipeline(CreateBoardForSwapWithMatch(), CreateMatchMove(), new GameSession());
        var scoreEvent = Assert.IsType<ScoreAdded>(result.Events.First(e => e is ScoreAdded));

        Assert.True(scoreEvent.Points > 0);
    }

    [Fact]
    public void TurnProcessor_ReturnsGameEndedEvent()
    {
        var session = new GameSession();
        session.UpdateTimer(TimeSpan.FromSeconds(60));

        var result = ExecutePipeline(CreateBoardForSwapWithMatch(), CreateMatchMove(), session);

        Assert.Contains(result.Events, e => e is GameEnded);
    }

    [Fact]
    public void TurnProcessor_ReturnsGameEndedEvent_WhenTimerExpiresMidPipeline()
    {
        var session = new GameSession();
        var board = CreateBoardForSwapWithMatch();
        var move = CreateMatchMove();
        var machine = new GameplayStateMachine();
        var processor = CreateProcessor();

        var result = processor.ProcessTurnPipelineWithEvents(
            board,
            move,
            session,
            machine,
            onPhaseCompleted: (state, currentSession) =>
            {
                if (state == GameplayState.ApplyingGravity)
                {
                    currentSession.UpdateTimer(TimeSpan.FromSeconds(60));
                }
            });

        Assert.Contains(result.Events, e => e is GameEnded);
    }

    [Fact]
    public void TurnProcessor_ReturnsDestroyerSpawned_WhenMatchedLineBonusActivates()
    {
        var board = CreateBoardForSwapWithMatch();
        var bonuses = new Dictionary<GridPosition, Match3.Core.GameCore.Bonuses.BonusToken>
        {
            [new GridPosition(0, 1)] = new Match3.Core.GameCore.Bonuses.LineBonus(new GridPosition(0, 1), PieceColor.Red, Match3.Core.GameCore.Bonuses.LineOrientation.Horizontal)
        };

        var result = CreateProcessor().ProcessTurnPipelineWithEvents(
            board,
            CreateMatchMove(),
            new GameSession(),
            new GameplayStateMachine(),
            bonuses: bonuses);

        Assert.Contains(result.Events, e => e is DestroyerSpawned);
    }

    [Fact]
    public void TurnProcessor_ReturnsDestroyerSpawned_WhenMovedBonusCompletesMatch()
    {
        var board = CreateBoardForSwapWithMatch();
        var bonuses = new Dictionary<GridPosition, Match3.Core.GameCore.Bonuses.BonusToken>
        {
            [new GridPosition(1, 2)] = new Match3.Core.GameCore.Bonuses.LineBonus(new GridPosition(1, 2), PieceColor.Red, Match3.Core.GameCore.Bonuses.LineOrientation.Horizontal)
        };

        var result = CreateProcessor().ProcessTurnPipelineWithEvents(
            board,
            new Move(new GridPosition(1, 2), new GridPosition(0, 2)),
            new GameSession(),
            new GameplayStateMachine(),
            bonuses: bonuses);

        Assert.Contains(result.Events, e => e is DestroyerSpawned);
    }

    [Fact]
    public void TurnProcessor_AppliesSwap_WhenMovedBonusFormsMatchWithAnotherBonusAndPiece()
    {
        var board = CreateBoardForBonusToBonusMatch();
        var bonuses = new Dictionary<GridPosition, Match3.Core.GameCore.Bonuses.BonusToken>
        {
            [new GridPosition(0, 1)] = new Match3.Core.GameCore.Bonuses.LineBonus(new GridPosition(0, 1), PieceColor.Red, Match3.Core.GameCore.Bonuses.LineOrientation.Horizontal),
            [new GridPosition(1, 2)] = new Match3.Core.GameCore.Bonuses.LineBonus(new GridPosition(1, 2), PieceColor.Red, Match3.Core.GameCore.Bonuses.LineOrientation.Vertical)
        };

        var result = CreateProcessor().ProcessTurnPipelineWithEvents(
            board,
            new Move(new GridPosition(1, 2), new GridPosition(0, 2)),
            new GameSession(),
            new GameplayStateMachine(),
            bonuses: bonuses);

        Assert.True(result.IsSwapApplied);
        Assert.Contains(result.Events, e => e is DestroyerSpawned);
    }

    [Fact]
    public void TurnProcessor_UsesBonusColorForMatchDetection_WhenBoardCellDrifted()
    {
        var board = CreateBoardForBonusToBonusMatch();
        board.SetCell(new GridPosition(0, 1), PieceType.Green);
        board.SetCell(new GridPosition(1, 2), PieceType.Blue);
        var bonuses = new Dictionary<GridPosition, Match3.Core.GameCore.Bonuses.BonusToken>
        {
            [new GridPosition(0, 1)] = new Match3.Core.GameCore.Bonuses.LineBonus(new GridPosition(0, 1), PieceColor.Red, Match3.Core.GameCore.Bonuses.LineOrientation.Horizontal),
            [new GridPosition(1, 2)] = new Match3.Core.GameCore.Bonuses.LineBonus(new GridPosition(1, 2), PieceColor.Red, Match3.Core.GameCore.Bonuses.LineOrientation.Vertical)
        };

        var result = CreateProcessor().ProcessTurnPipelineWithEvents(
            board,
            new Move(new GridPosition(1, 2), new GridPosition(0, 2)),
            new GameSession(),
            new GameplayStateMachine(),
            bonuses: bonuses);

        Assert.True(result.IsSwapApplied);
    }

    private static TurnPipelineResult ExecutePipeline(BoardState board, Move move, GameSession session)
    {
        return CreateProcessor().ProcessTurnPipelineWithEvents(board, move, session, new GameplayStateMachine());
    }

    private static TurnProcessor CreateProcessor()
    {
        return new TurnProcessor(
            matchFinder: new MatchFinder(),
            gravityResolver: new GravityResolver(),
            refillResolver: new RefillResolver(new SequenceRandomSource(0, 1, 2, 3, 4)));
    }

    private static Move CreateMatchMove()
    {
        return new Move(new GridPosition(0, 2), new GridPosition(1, 2));
    }

    private static BoardState CreateBoardForSwapWithMatch()
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

        board.SetCell(new GridPosition(0, 0), PieceType.Red);
        board.SetCell(new GridPosition(0, 1), PieceType.Red);
        board.SetCell(new GridPosition(0, 2), PieceType.Blue);
        board.SetCell(new GridPosition(1, 2), PieceType.Red);
        return board;
    }

    private static BoardState CreateBoardForSwapWithoutMatch()
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

        board.SetCell(new GridPosition(0, 0), PieceType.Red);
        board.SetCell(new GridPosition(0, 1), PieceType.Green);
        return board;
    }

    private static BoardState CreateBoardForBonusToBonusMatch()
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

        board.SetCell(new GridPosition(0, 0), PieceType.Red);
        board.SetCell(new GridPosition(0, 1), PieceType.Red);
        board.SetCell(new GridPosition(0, 2), PieceType.Blue);
        board.SetCell(new GridPosition(1, 2), PieceType.Red);
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
