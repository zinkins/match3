using System.Collections.Generic;
using Match3.Core.GameCore.Board;
using Match3.Core.GameCore.Bonuses;
using Match3.Core.GameCore.Pieces;
using Match3.Core.GameCore.ValueObjects;
using Match3.Core.GameFlow.Events;
using Match3.Core.GameFlow.Pipeline;
using Match3.Core.GameFlow.Sessions;
using Match3.Core.GameFlow.StateMachine;

namespace Match3.Tests;

public class Phase11BonusesTests
{
    [Fact]
    public void BonusFactory_CreatesLine_ForMatchOfFour()
    {
        var factory = new BonusFactory();
        var groups = new[]
        {
            Match(PieceType.Red, new GridPosition(2, 1), new GridPosition(2, 2), new GridPosition(2, 3), new GridPosition(2, 4))
        };

        var bonus = factory.Create(groups, new GridPosition(2, 4));

        Assert.IsType<LineBonus>(bonus);
    }

    [Fact]
    public void BonusFactory_CreatesLine_OnLastMovedCell()
    {
        var factory = new BonusFactory();
        var lastMoved = new GridPosition(3, 4);
        var groups = new[]
        {
            Match(PieceType.Blue, new GridPosition(3, 1), new GridPosition(3, 2), new GridPosition(3, 3), lastMoved)
        };

        var bonus = Assert.IsType<LineBonus>(factory.Create(groups, lastMoved));

        Assert.Equal(lastMoved, bonus.Position);
    }

    [Fact]
    public void LineBonus_HasSameColorAsMatchedPieces()
    {
        var factory = new BonusFactory();
        var groups = new[]
        {
            Match(PieceType.Green, new GridPosition(1, 1), new GridPosition(1, 2), new GridPosition(1, 3), new GridPosition(1, 4))
        };

        var bonus = Assert.IsType<LineBonus>(factory.Create(groups, new GridPosition(1, 4)));

        Assert.Equal(PieceColor.Green, bonus.Color);
    }

    [Fact]
    public void LineBonus_HasOrientation()
    {
        var factory = new BonusFactory();
        var horizontal = new[]
        {
            Match(PieceType.Red, new GridPosition(0, 1), new GridPosition(0, 2), new GridPosition(0, 3), new GridPosition(0, 4))
        };
        var vertical = new[]
        {
            Match(PieceType.Blue, new GridPosition(1, 6), new GridPosition(2, 6), new GridPosition(3, 6), new GridPosition(4, 6))
        };

        var horizontalBonus = Assert.IsType<LineBonus>(factory.Create(horizontal, new GridPosition(0, 4)));
        var verticalBonus = Assert.IsType<LineBonus>(factory.Create(vertical, new GridPosition(4, 6)));

        Assert.Equal(LineOrientation.Horizontal, horizontalBonus.Orientation);
        Assert.Equal(LineOrientation.Vertical, verticalBonus.Orientation);
    }

    [Fact]
    public void BonusFactory_CreatesBomb_ForMatchOfFive()
    {
        var factory = new BonusFactory();
        var groups = new[]
        {
            Match(PieceType.Yellow, new GridPosition(5, 0), new GridPosition(5, 1), new GridPosition(5, 2), new GridPosition(5, 3), new GridPosition(5, 4))
        };

        var bonus = factory.Create(groups, new GridPosition(5, 2));

        Assert.IsType<BombBonus>(bonus);
    }

    [Fact]
    public void BonusFactory_CreatesBomb_OnLastMovedCell_ForLinearMatch()
    {
        var factory = new BonusFactory();
        var lastMoved = new GridPosition(4, 5);
        var groups = new[]
        {
            Match(PieceType.Purple, new GridPosition(4, 1), new GridPosition(4, 2), new GridPosition(4, 3), new GridPosition(4, 4), lastMoved)
        };

        var bonus = Assert.IsType<BombBonus>(factory.Create(groups, lastMoved));

        Assert.Equal(lastMoved, bonus.Position);
    }

    [Fact]
    public void BonusFactory_CreatesBomb_ForCrossMatch()
    {
        var factory = new BonusFactory();
        var intersection = new GridPosition(3, 3);
        var groups = new[]
        {
            Match(PieceType.Red, new GridPosition(3, 1), new GridPosition(3, 2), intersection, new GridPosition(3, 4), new GridPosition(3, 5)),
            Match(PieceType.Red, new GridPosition(1, 3), new GridPosition(2, 3), intersection, new GridPosition(4, 3), new GridPosition(5, 3))
        };

        var bonus = Assert.IsType<BombBonus>(factory.Create(groups, new GridPosition(3, 5)));

        Assert.Equal(intersection, bonus.Position);
    }

    [Fact]
    public void BombBonus_HasSameColorAsMatchedPieces()
    {
        var factory = new BonusFactory();
        var groups = new[]
        {
            Match(PieceType.Purple, new GridPosition(6, 1), new GridPosition(6, 2), new GridPosition(6, 3), new GridPosition(6, 4), new GridPosition(6, 5))
        };

        var bonus = Assert.IsType<BombBonus>(factory.Create(groups, new GridPosition(6, 5)));

        Assert.Equal(PieceColor.Purple, bonus.Color);
    }

    [Fact]
    public void LineBonus_ActivatesAndProducesDestroyers()
    {
        var board = CreateFilledBoard();
        var behavior = new LineBonusBehavior();
        var bonus = new LineBonus(new GridPosition(2, 3), PieceColor.Red, LineOrientation.Horizontal);

        var destroyer = behavior.Activate(bonus, board);

        Assert.NotEmpty(destroyer.Path);
        Assert.NotEmpty(destroyer.DestroyedPositions);
    }

    [Fact]
    public void Destroyer_DestroysPiecesOnPath()
    {
        var board = CreateFilledBoard();
        var behavior = new LineBonusBehavior();
        var bonus = new LineBonus(new GridPosition(1, 4), PieceColor.Blue, LineOrientation.Vertical);

        var destroyer = behavior.Activate(bonus, board);

        foreach (var position in destroyer.Path)
        {
            Assert.Null(board.GetPiece(position));
        }
    }

    [Fact]
    public void Destroyer_ActivatesOtherBonusesOnPath()
    {
        var board = CreateFilledBoard();
        var behavior = new LineBonusBehavior();
        var line = new LineBonus(new GridPosition(4, 2), PieceColor.Red, LineOrientation.Horizontal);
        var bombOnPath = new BombBonus(new GridPosition(4, 5), PieceColor.Green);
        board.SetBonus(line.Position, line);
        board.SetBonus(bombOnPath.Position, bombOnPath);

        var destroyer = behavior.Activate(line, board);

        Assert.Contains(destroyer.ActivatedBonuses, bonus => bonus.Position == bombOnPath.Position && bonus.Kind == bombOnPath.Kind);
    }

    [Fact]
    public void BombBonus_ActivatesAndExplodesArea()
    {
        var board = CreateFilledBoard();
        var behavior = new BombBonusBehavior();
        var bomb = new BombBonus(new GridPosition(3, 3), PieceColor.Yellow);

        var result = behavior.Activate(bomb, board);

        Assert.Equal(9, result.AffectedArea.Count);
        Assert.All(result.AffectedArea, position => Assert.Null(board.GetPiece(position)));
    }

    [Fact]
    public void BombBonus_ActivatesOtherBonusesInsideExplosionArea()
    {
        var board = CreateFilledBoard();
        var behavior = new BombBonusBehavior();
        var bomb = new BombBonus(new GridPosition(3, 3), PieceColor.Red);
        var lineInArea = new LineBonus(new GridPosition(3, 4), PieceColor.Blue, LineOrientation.Horizontal);
        board.SetBonus(bomb.Position, bomb);
        board.SetBonus(lineInArea.Position, lineInArea);

        var result = behavior.Activate(bomb, board);

        Assert.Contains(result.ActivatedBonuses, bonus => bonus.Position == lineInArea.Position && bonus.Kind == lineInArea.Kind);
    }

    [Fact]
    public void BonusActivationResolver_ActivatesChainReactionBonuses()
    {
        var board = CreateFilledBoard();
        var resolver = new BonusActivationResolver();
        var rootBomb = new BombBonus(new GridPosition(3, 3), PieceColor.Red);
        var triggeredLine = new LineBonus(new GridPosition(3, 4), PieceColor.Blue, LineOrientation.Horizontal);
        var triggeredBomb = new BombBonus(new GridPosition(3, 6), PieceColor.Green);
        board.SetBonus(rootBomb.Position, rootBomb);
        board.SetBonus(triggeredLine.Position, triggeredLine);
        board.SetBonus(triggeredBomb.Position, triggeredBomb);

        var result = resolver.Resolve(board, rootBomb);

        Assert.Equal(3, result.ActivatedBonuses.Count);
        Assert.Contains(result.ActivatedBonuses, bonus => bonus.Position == triggeredBomb.Position && bonus.Kind == triggeredBomb.Kind);
        Assert.True(result.DestroyedPositions.Count > 9);
    }

    [Fact]
    public void BonusActivationResolver_ClearsAreaAroundChainedBomb()
    {
        var board = CreateFilledBoard();
        var resolver = new BonusActivationResolver();
        var rootBomb = new BombBonus(new GridPosition(3, 3), PieceColor.Red);
        var triggeredBomb = new BombBonus(new GridPosition(3, 4), PieceColor.Blue);
        board.SetBonus(rootBomb.Position, rootBomb);
        board.SetBonus(triggeredBomb.Position, triggeredBomb);

        resolver.Resolve(board, rootBomb);

        for (var row = 2; row <= 4; row++)
        {
            for (var column = 3; column <= 5; column++)
            {
                Assert.Null(board.GetPiece(new GridPosition(row, column)));
            }
        }
    }

    [Fact]
    public void TurnProcessor_CreatesLineBonus_OnBoard_WhenMoveFormsMatchOfFour()
    {
        var board = CreateBoardForLineBonusCreation();
        var processor = new TurnProcessor(
            new MatchFinder(),
            new GravityResolver(),
            new RefillResolver(new SequenceRandomSource(1, 2, 3, 4, 0)),
            new ScoreCalculator(),
            new BonusFactory(),
            new BonusActivationResolver());

        processor.ProcessTurnPipelineWithEvents(
            board,
            new Move(new GridPosition(0, 2), new GridPosition(1, 2)),
            new GameSession(),
            new GameplayStateMachine());

        var created = Assert.IsType<LineBonus>(board.GetBonus(new GridPosition(0, 2)));
        Assert.Equal(new GridPosition(0, 2), created.Position);
    }

    [Fact]
    public void TurnProcessor_ReturnsBonusCreatedEvent_WhenBonusAppears()
    {
        var board = CreateBoardForLineBonusCreation();
        var processor = new TurnProcessor(
            new MatchFinder(),
            new GravityResolver(),
            new RefillResolver(new SequenceRandomSource(1, 2, 3, 4, 0)),
            new ScoreCalculator(),
            new BonusFactory(),
            new BonusActivationResolver());

        var result = processor.ProcessTurnPipelineWithEvents(
            board,
            new Move(new GridPosition(0, 2), new GridPosition(1, 2)),
            new GameSession(),
            new GameplayStateMachine());

        Assert.Contains(result.Events, e => e is LineBonusCreated);
    }

    [Fact]
    public void TurnProcessor_KeepsNewlyCreatedBonus_OnBoard_WhenCascadeWouldImmediatelyMatchIt()
    {
        var board = CreateBoardForFreshBonusProtection();
        var processor = new TurnProcessor(
            new MatchFinder(),
            new GravityResolver(),
            new RefillResolver(new SequenceRandomSource(1, 2, 3, 4, 0)),
            new ScoreCalculator(),
            new BonusFactory(),
            new BonusActivationResolver());

        var result = processor.ProcessTurnPipelineWithEvents(
            board,
            new Move(new GridPosition(4, 2), new GridPosition(5, 2)),
            new GameSession(),
            new GameplayStateMachine());

        Assert.True(HasAnyBonus(board));
        Assert.DoesNotContain(result.Events, e => e is DestroyerSpawned or BombExploded);
    }

    private static MatchGroup Match(PieceType piece, params GridPosition[] positions)
    {
        return new MatchGroup(piece, positions);
    }

    private static BoardState CreateFilledBoard()
    {
        var board = new BoardState();
        for (var row = 0; row < board.Height; row++)
        {
            for (var column = 0; column < board.Width; column++)
            {
                board.SetPiece(new GridPosition(row, column), PieceType.Red);
            }
        }

        return board;
    }

    private static BoardState CreateBoardForLineBonusCreation()
    {
        var board = new BoardState();
        var rows =
            new[]
            {
                new[] { PieceType.Red, PieceType.Red, PieceType.Blue, PieceType.Red, PieceType.Purple, PieceType.Green, PieceType.Blue, PieceType.Yellow },
                new[] { PieceType.Blue, PieceType.Green, PieceType.Red, PieceType.Purple, PieceType.Green, PieceType.Blue, PieceType.Yellow, PieceType.Purple },
                new[] { PieceType.Yellow, PieceType.Purple, PieceType.Green, PieceType.Green, PieceType.Blue, PieceType.Yellow, PieceType.Purple, PieceType.Green },
                new[] { PieceType.Purple, PieceType.Yellow, PieceType.Blue, PieceType.Red, PieceType.Yellow, PieceType.Purple, PieceType.Green, PieceType.Blue },
                new[] { PieceType.Green, PieceType.Blue, PieceType.Yellow, PieceType.Purple, PieceType.Green, PieceType.Blue, PieceType.Yellow, PieceType.Purple },
                new[] { PieceType.Blue, PieceType.Green, PieceType.Purple, PieceType.Yellow, PieceType.Blue, PieceType.Yellow, PieceType.Purple, PieceType.Green },
                new[] { PieceType.Yellow, PieceType.Purple, PieceType.Green, PieceType.Blue, PieceType.Yellow, PieceType.Purple, PieceType.Green, PieceType.Blue },
                new[] { PieceType.Purple, PieceType.Yellow, PieceType.Blue, PieceType.Green, PieceType.Purple, PieceType.Green, PieceType.Blue, PieceType.Yellow }
            };

        for (var row = 0; row < board.Height; row++)
        {
            for (var column = 0; column < board.Width; column++)
            {
                board.SetPiece(new GridPosition(row, column), rows[row][column]);
            }
        }

        return board;
    }

    private static BoardState CreateBoardForFreshBonusProtection()
    {
        var board = new BoardState();
        var types = PieceCatalog.All;
        for (var row = 0; row < board.Height; row++)
        {
            for (var column = 0; column < board.Width; column++)
            {
                board.SetPiece(new GridPosition(row, column), types[(row + (column * 2)) % types.Count]);
            }
        }

        board.SetPiece(new GridPosition(5, 0), PieceType.Red);
        board.SetPiece(new GridPosition(5, 1), PieceType.Red);
        board.SetPiece(new GridPosition(5, 2), PieceType.Blue);
        board.SetPiece(new GridPosition(5, 3), PieceType.Red);
        board.SetPiece(new GridPosition(4, 2), PieceType.Red);
        board.SetPiece(new GridPosition(6, 2), PieceType.Red);
        board.SetPiece(new GridPosition(7, 2), PieceType.Red);

        return board;
    }

    private static bool HasAnyBonus(BoardState board)
    {
        for (var row = 0; row < board.Height; row++)
        {
            for (var column = 0; column < board.Width; column++)
            {
                if (board.GetBonus(new GridPosition(row, column)) is not null)
                {
                    return true;
                }
            }
        }

        return false;
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
