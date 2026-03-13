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
        board.SetBonus(new GridPosition(0, 1), new Match3.Core.GameCore.Bonuses.LineBonus(new GridPosition(0, 1), PieceColor.Red, Match3.Core.GameCore.Bonuses.LineOrientation.Horizontal));

        var result = CreateProcessor().ProcessTurnPipelineWithEvents(
            board,
            CreateMatchMove(),
            new GameSession(),
            new GameplayStateMachine());

        Assert.Contains(result.Events, e => e is DestroyerSpawned);
    }

    [Fact]
    public void TurnProcessor_ReturnsDestroyerSpawned_WhenMovedBonusCompletesMatch()
    {
        var board = CreateBoardForSwapWithMatch();
        board.SetBonus(new GridPosition(1, 2), new Match3.Core.GameCore.Bonuses.LineBonus(new GridPosition(1, 2), PieceColor.Red, Match3.Core.GameCore.Bonuses.LineOrientation.Horizontal));

        var result = CreateProcessor().ProcessTurnPipelineWithEvents(
            board,
            new Move(new GridPosition(1, 2), new GridPosition(0, 2)),
            new GameSession(),
            new GameplayStateMachine());

        Assert.Contains(result.Events, e => e is DestroyerSpawned);
    }

    [Fact]
    public void TurnProcessor_BuildsSeparateCascadeSteps_WhenGravityCreatesFollowUpMatch()
    {
        var result = CreateProcessor().ProcessTurnPipelineWithEvents(
            CreateBoardForCascadeAfterGravity(),
            new Move(new GridPosition(3, 1), new GridPosition(3, 2)),
            new GameSession(),
            new GameplayStateMachine());

        Assert.True(result.IsSwapApplied);
        Assert.Equal(2, result.CascadeSteps.Count);
        Assert.Contains(result.CascadeSteps[0].Events, e => e is MatchResolved);
        Assert.Contains(result.CascadeSteps[0].Events, e => e is PiecesFell);
        Assert.Contains(result.CascadeSteps[0].Events, e => e is PiecesSpawned);
        Assert.Contains(result.CascadeSteps[1].Events, e => e is MatchResolved);
        Assert.Contains(result.CascadeSteps[1].Events, e => e is PiecesFell);
        Assert.Contains(result.CascadeSteps[1].Events, e => e is PiecesSpawned);
    }

    [Fact]
    public void ChainReactionScenario_PlaysBonusEffectsInDeterministicOrder()
    {
        var board = CreateBoardForBonusChainReactionOrder();
        board.SetBonus(new GridPosition(3, 1), new Match3.Core.GameCore.Bonuses.LineBonus(new GridPosition(3, 1), PieceColor.Red, Match3.Core.GameCore.Bonuses.LineOrientation.Horizontal));
        board.SetBonus(new GridPosition(3, 3), new Match3.Core.GameCore.Bonuses.BombBonus(new GridPosition(3, 3), PieceColor.Blue));
        board.SetBonus(new GridPosition(2, 3), new Match3.Core.GameCore.Bonuses.LineBonus(new GridPosition(2, 3), PieceColor.Green, Match3.Core.GameCore.Bonuses.LineOrientation.Vertical));

        var result = CreateProcessor().ProcessTurnPipelineWithEvents(
            board,
            new Move(new GridPosition(4, 2), new GridPosition(3, 2)),
            new GameSession(),
            new GameplayStateMachine());

        var orderedBonusEvents = result.Events
            .Where(e => e is DestroyerSpawned or BombExploded)
            .Select(e => e switch
            {
                DestroyerSpawned destroyer => $"D:{destroyer.Position.Row},{destroyer.Position.Column}",
                BombExploded bomb => $"B:{bomb.Position.Row},{bomb.Position.Column}",
                _ => throw new InvalidOperationException()
            })
            .ToArray();

        Assert.Equal(
            [
                "D:3,1",
                "B:3,3",
                "D:2,3"
            ],
            orderedBonusEvents);
    }

    [Fact]
    public void TurnProcessor_UsesCascadeMatchLocation_ForBonusAnchor_WhenLastSwapIsOutsideMatch()
    {
        var method = typeof(TurnProcessor).GetMethod("GetBonusAnchor", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
        Assert.NotNull(method);
        var matches = new[]
        {
            new MatchGroup(PieceType.Red,
                [new GridPosition(0, 0), new GridPosition(0, 1), new GridPosition(0, 2), new GridPosition(0, 3)])
        };
        var move = new Move(new GridPosition(5, 5), new GridPosition(5, 6));

        var anchor = Assert.IsType<GridPosition>(method!.Invoke(null, [matches, move]));

        Assert.Equal(0, anchor.Row);
        Assert.InRange(anchor.Column, 1, 2);
        Assert.NotEqual(move.To, anchor);
    }

    [Fact]
    public void TurnProcessor_AppliesSwap_WhenMovedBonusFormsMatchWithAnotherBonusAndPiece()
    {
        var board = CreateBoardForBonusToBonusMatch();
        board.SetBonus(new GridPosition(0, 1), new Match3.Core.GameCore.Bonuses.LineBonus(new GridPosition(0, 1), PieceColor.Red, Match3.Core.GameCore.Bonuses.LineOrientation.Horizontal));
        board.SetBonus(new GridPosition(1, 2), new Match3.Core.GameCore.Bonuses.LineBonus(new GridPosition(1, 2), PieceColor.Red, Match3.Core.GameCore.Bonuses.LineOrientation.Vertical));

        var result = CreateProcessor().ProcessTurnPipelineWithEvents(
            board,
            new Move(new GridPosition(1, 2), new GridPosition(0, 2)),
            new GameSession(),
            new GameplayStateMachine());

        Assert.True(result.IsSwapApplied);
        Assert.Contains(result.Events, e => e is DestroyerSpawned);
    }

    [Fact]
    public void TurnProcessor_UsesBonusColorForMatchDetection_WhenBoardCellDrifted()
    {
        var board = CreateBoardForBonusToBonusMatch();
        board.SetContent(
            new GridPosition(0, 1),
            new Match3.Core.GameCore.Board.CellContent(
                PieceType.Red,
                new Match3.Core.GameCore.Bonuses.LineBonus(new GridPosition(0, 1), PieceColor.Red, Match3.Core.GameCore.Bonuses.LineOrientation.Horizontal)));
        board.SetContent(
            new GridPosition(1, 2),
            new Match3.Core.GameCore.Board.CellContent(
                PieceType.Red,
                new Match3.Core.GameCore.Bonuses.LineBonus(new GridPosition(1, 2), PieceColor.Red, Match3.Core.GameCore.Bonuses.LineOrientation.Vertical)));

        var result = CreateProcessor().ProcessTurnPipelineWithEvents(
            board,
            new Move(new GridPosition(1, 2), new GridPosition(0, 2)),
            new GameSession(),
            new GameplayStateMachine());

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
                board.SetPiece(new GridPosition(row, column), types[(row + column) % types.Count]);
            }
        }

        board.SetPiece(new GridPosition(0, 0), PieceType.Red);
        board.SetPiece(new GridPosition(0, 1), PieceType.Red);
        board.SetPiece(new GridPosition(0, 2), PieceType.Blue);
        board.SetPiece(new GridPosition(1, 2), PieceType.Red);
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
                board.SetPiece(new GridPosition(row, column), types[(row + column) % types.Count]);
            }
        }

        board.SetPiece(new GridPosition(0, 0), PieceType.Red);
        board.SetPiece(new GridPosition(0, 1), PieceType.Green);
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
                board.SetPiece(new GridPosition(row, column), types[(row + column) % types.Count]);
            }
        }

        board.SetPiece(new GridPosition(0, 0), PieceType.Red);
        board.SetPiece(new GridPosition(0, 1), PieceType.Red);
        board.SetPiece(new GridPosition(0, 2), PieceType.Blue);
        board.SetPiece(new GridPosition(1, 2), PieceType.Red);
        return board;
    }

    private static BoardState CreateBoardForCascadeAfterGravity()
    {
        var board = new BoardState();
        var types = PieceCatalog.All;
        for (var row = 0; row < board.Height; row++)
        {
            for (var column = 0; column < board.Width; column++)
            {
                board.SetPiece(new GridPosition(row, column), types[(row + column) % types.Count]);
            }
        }

        board.SetPiece(new GridPosition(1, 1), PieceType.Red);
        board.SetPiece(new GridPosition(2, 1), PieceType.Blue);
        board.SetPiece(new GridPosition(3, 1), PieceType.Green);
        board.SetPiece(new GridPosition(4, 1), PieceType.Blue);
        board.SetPiece(new GridPosition(5, 1), PieceType.Red);
        board.SetPiece(new GridPosition(6, 1), PieceType.Red);
        board.SetPiece(new GridPosition(3, 2), PieceType.Blue);
        return board;
    }

    private static BoardState CreateBoardForBonusChainReactionOrder()
    {
        var board = new BoardState();
        var types = PieceCatalog.All;
        for (var row = 0; row < board.Height; row++)
        {
            for (var column = 0; column < board.Width; column++)
            {
                board.SetPiece(new GridPosition(row, column), types[(row + column) % types.Count]);
            }
        }

        board.SetPiece(new GridPosition(3, 0), PieceType.Red);
        board.SetPiece(new GridPosition(3, 1), PieceType.Red);
        board.SetPiece(new GridPosition(3, 2), PieceType.Blue);
        board.SetPiece(new GridPosition(4, 2), PieceType.Red);
        board.SetPiece(new GridPosition(3, 3), PieceType.Blue);
        board.SetPiece(new GridPosition(2, 3), PieceType.Green);
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
