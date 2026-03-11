using System;
using System.Collections.Generic;
using System.Linq;
using Match3.Core.GameCore.Board;
using Match3.Core.GameCore.Bonuses;
using Match3.Core.GameCore.Pieces;
using Match3.Core.GameCore.ValueObjects;
using Match3.Core.GameFlow.Events;
using Match3.Core.GameFlow.Sessions;
using Match3.Core.GameFlow.StateMachine;

namespace Match3.Core.GameFlow.Pipeline;

public sealed class TurnProcessor
{
    private readonly MatchFinder matchFinder;
    private readonly GravityResolver gravityResolver;
    private readonly RefillResolver refillResolver;
    private readonly ScoreCalculator scoreCalculator;
    private readonly BonusFactory bonusFactory;
    private readonly BonusActivationResolver bonusActivationResolver;

    public TurnProcessor()
        : this(new MatchFinder(), new GravityResolver(), new RefillResolver(), new ScoreCalculator())
    {
    }

    public TurnProcessor(MatchFinder matchFinder, GravityResolver gravityResolver, RefillResolver refillResolver)
        : this(matchFinder, gravityResolver, refillResolver, new ScoreCalculator())
    {
    }

    public TurnProcessor(
        MatchFinder matchFinder,
        GravityResolver gravityResolver,
        RefillResolver refillResolver,
        ScoreCalculator scoreCalculator)
        : this(matchFinder, gravityResolver, refillResolver, scoreCalculator, new BonusFactory(), new BonusActivationResolver())
    {
    }

    public TurnProcessor(
        MatchFinder matchFinder,
        GravityResolver gravityResolver,
        RefillResolver refillResolver,
        ScoreCalculator scoreCalculator,
        BonusFactory bonusFactory,
        BonusActivationResolver bonusActivationResolver)
    {
        this.matchFinder = matchFinder ?? throw new ArgumentNullException(nameof(matchFinder));
        this.gravityResolver = gravityResolver ?? throw new ArgumentNullException(nameof(gravityResolver));
        this.refillResolver = refillResolver ?? throw new ArgumentNullException(nameof(refillResolver));
        this.scoreCalculator = scoreCalculator ?? throw new ArgumentNullException(nameof(scoreCalculator));
        this.bonusFactory = bonusFactory ?? throw new ArgumentNullException(nameof(bonusFactory));
        this.bonusActivationResolver = bonusActivationResolver ?? throw new ArgumentNullException(nameof(bonusActivationResolver));
    }

    public bool TryProcessMove(BoardState board, Move move)
    {
        Swap(board, new Dictionary<GridPosition, BonusToken>(), move);

        var matches = matchFinder.FindMatches(board);
        if (matches.Count > 0)
        {
            return true;
        }

        Swap(board, new Dictionary<GridPosition, BonusToken>(), move);
        return false;
    }

    public bool RecheckAfterGravityAndRefill(BoardState board)
    {
        gravityResolver.Apply(board);
        refillResolver.Refill(board);
        return matchFinder.FindMatches(board).Count > 0;
    }

    public void ExecuteAtomicResolvingStep(GameSession session, GameplayStateMachine stateMachine, Action resolveAction)
    {
        stateMachine.TransitionToResolving();
        resolveAction();
        stateMachine.AdvanceAfterPhase(session);
    }

    public bool ProcessTurnPipeline(BoardState board, Move move, GameSession session, GameplayStateMachine stateMachine)
    {
        return ProcessTurnPipelineWithEvents(board, move, session, stateMachine).IsSwapApplied;
    }

    public TurnPipelineResult ProcessTurnPipelineWithEvents(
        BoardState board,
        Move move,
        GameSession session,
        GameplayStateMachine stateMachine,
        int currentScore = 0,
        IDictionary<GridPosition, BonusToken> bonuses = null,
        Action<GameplayState, GameSession> onPhaseCompleted = null)
    {
        bonuses ??= new Dictionary<GridPosition, BonusToken>();
        var events = new List<IDomainEvent>();

        stateMachine.TransitionToSelecting();
        onPhaseCompleted?.Invoke(stateMachine.State, session);

        stateMachine.TransitionToSwapping();
        onPhaseCompleted?.Invoke(stateMachine.State, session);

        Swap(board, bonuses, move);
        SyncBonusColorsToBoard(board, bonuses);
        events.Add(new PiecesSwapped(move));

        var matches = matchFinder.FindMatches(board);
        var applied = matches.Count > 0;
        if (!applied)
        {
            Swap(board, bonuses, move);
            events.Add(new SwapReverted(move));
            stateMachine.TransitionToIdle();
            return new TurnPipelineResult(false, events);
        }

        stateMachine.TransitionToResolving();
        onPhaseCompleted?.Invoke(stateMachine.State, session);

        var resolvedScore = currentScore;
        while (matches.Count > 0)
        {
            var matchedPositions = matches
                .SelectMany(group => group.Positions)
                .Distinct()
                .ToArray();
            var hasMatchedBonuses = matchedPositions.Any(position => bonuses.ContainsKey(position));
            var createdBonus = hasMatchedBonuses
                ? null
                : TryCreateBonus(matches, GetBonusAnchor(matches, move), bonuses);
            matchedPositions = matches
                .SelectMany(group => group.Positions)
                .Where(position => createdBonus is null || position != createdBonus.Position)
                .Distinct()
                .ToArray();

            var bonusActivation = ResolveMatchedBonuses(board, bonuses, matchedPositions);
            var plainDestroyed = matchedPositions
                .Where(position => !bonusActivation.ActivatedBonusPositions.Contains(position))
                .ToArray();

            ClearMatchedPieces(board, bonuses, plainDestroyed);

            var destroyedPositions = plainDestroyed
                .Concat(bonusActivation.DestroyedPositions)
                .Distinct()
                .ToArray();
            var destroyedPieces = destroyedPositions.Length;

            if (createdBonus is not null)
            {
                bonuses[createdBonus.Position] = createdBonus;
                events.Add(CreateBonusCreatedEvent(createdBonus));
            }

            foreach (var activated in bonusActivation.ActivatedBonuses)
            {
                events.Add(CreateBonusActivationEvent(board, activated));
            }

            events.Add(new MatchResolved(destroyedPieces));
            var updatedScore = scoreCalculator.AddScore(resolvedScore, destroyedPieces);
            events.Add(new ScoreAdded(updatedScore - resolvedScore));
            resolvedScore = updatedScore;

            if (session.IsGameOver)
            {
                return FinishWithGameOver(stateMachine, events, applied);
            }

            stateMachine.TransitionToApplyingGravity();
            ApplyGravityToBonuses(board, bonuses);
            gravityResolver.Apply(board);
            events.Add(new PiecesFell());
            onPhaseCompleted?.Invoke(stateMachine.State, session);

            if (session.IsGameOver)
            {
                return FinishWithGameOver(stateMachine, events, applied);
            }

            stateMachine.TransitionToRefilling();
            refillResolver.Refill(board);
            SyncBonusColorsToBoard(board, bonuses);
            events.Add(new PiecesSpawned());
            onPhaseCompleted?.Invoke(stateMachine.State, session);

            if (session.IsGameOver)
            {
                return FinishWithGameOver(stateMachine, events, applied);
            }

            matches = matchFinder.FindMatches(board);
            if (matches.Count > 0)
            {
                stateMachine.TransitionToResolving();
                onPhaseCompleted?.Invoke(stateMachine.State, session);
            }
        }

        stateMachine.TransitionToCheckingEndGame();
        if (session.IsGameOver)
        {
            return FinishWithGameOver(stateMachine, events, applied);
        }

        stateMachine.TransitionToIdle();
        return new TurnPipelineResult(applied, events);
    }

    private BonusToken TryCreateBonus(
        IReadOnlyList<MatchGroup> matches,
        GridPosition lastMovedCell,
        IDictionary<GridPosition, BonusToken> bonuses)
    {
        if (matches.Count == 0)
        {
            return null;
        }

        var canCreate = matches.Any(group => group.Positions.Count >= 4) || HasIntersection(matches);
        if (!canCreate)
        {
            return null;
        }

        var bonus = bonusFactory.Create(matches, lastMovedCell);
        bonuses.Remove(bonus.Position);
        return bonus;
    }

    private static GridPosition GetBonusAnchor(IReadOnlyList<MatchGroup> matches, Move move)
    {
        var matchedPositions = matches.SelectMany(group => group.Positions).Distinct().ToArray();
        var containsFrom = matchedPositions.Contains(move.From);
        var containsTo = matchedPositions.Contains(move.To);

        return (containsFrom, containsTo) switch
        {
            (true, false) => move.From,
            (false, true) => move.To,
            _ => move.To
        };
    }

    private static bool HasIntersection(IReadOnlyList<MatchGroup> matches)
    {
        for (var i = 0; i < matches.Count; i++)
        {
            for (var j = i + 1; j < matches.Count; j++)
            {
                if (matches[i].Positions.Any(matches[j].Positions.Contains))
                {
                    return true;
                }
            }
        }

        return false;
    }

    private static void ClearMatchedPieces(BoardState board, IDictionary<GridPosition, BonusToken> bonuses, IReadOnlyList<GridPosition> positions)
    {
        foreach (var position in positions)
        {
            board.SetCell(position, null);
            bonuses.Remove(position);
        }
    }

    private BonusActivationSummary ResolveMatchedBonuses(
        BoardState board,
        IDictionary<GridPosition, BonusToken> bonuses,
        IReadOnlyList<GridPosition> matchedPositions)
    {
        var rootBonuses = matchedPositions
            .Where(position => bonuses.ContainsKey(position))
            .Select(position => bonuses[position])
            .GroupBy(bonus => bonus.Position)
            .Select(group => group.First())
            .ToArray();

        if (rootBonuses.Length == 0)
        {
            return BonusActivationSummary.Empty;
        }

        var destroyedPositions = new HashSet<GridPosition>();
        var activatedBonuses = new List<BonusToken>();
        var activatedBonusPositions = new HashSet<GridPosition>();
        var bonusesOnBoard = bonuses as IReadOnlyDictionary<GridPosition, BonusToken>
            ?? new Dictionary<GridPosition, BonusToken>(bonuses);

        foreach (var rootBonus in rootBonuses)
        {
            var result = bonusActivationResolver.Resolve(board, bonusesOnBoard, rootBonus);
            foreach (var activated in result.ActivatedBonuses)
            {
                activatedBonuses.Add(activated);
                activatedBonusPositions.Add(activated.Position);
                bonuses.Remove(activated.Position);
            }

            foreach (var destroyed in result.DestroyedPositions)
            {
                destroyedPositions.Add(destroyed);
                bonuses.Remove(destroyed);
            }
        }

        return new BonusActivationSummary([.. destroyedPositions], activatedBonuses, activatedBonusPositions);
    }

    private static void ApplyGravityToBonuses(BoardState board, IDictionary<GridPosition, BonusToken> bonuses)
    {
        if (bonuses.Count == 0)
        {
            return;
        }

        var moved = new Dictionary<GridPosition, BonusToken>();
        foreach (var entry in bonuses)
        {
            var position = entry.Key;
            var targetRow = position.Row;
            for (var row = position.Row + 1; row < board.Height; row++)
            {
                if (board.GetCell(new GridPosition(row, position.Column)) is null)
                {
                    targetRow++;
                }
            }

            var target = new GridPosition(targetRow, position.Column);
            moved[target] = entry.Value with { Position = target };
        }

        bonuses.Clear();
        foreach (var entry in moved)
        {
            bonuses[entry.Key] = entry.Value;
        }
    }

    private static void SyncBonusColorsToBoard(BoardState board, IDictionary<GridPosition, BonusToken> bonuses)
    {
        foreach (var entry in bonuses)
        {
            board.SetCell(entry.Key, ToPieceType(entry.Value.Color));
        }
    }

    private static PieceType ToPieceType(PieceColor color)
    {
        return color switch
        {
            PieceColor.Red => PieceType.Red,
            PieceColor.Green => PieceType.Green,
            PieceColor.Blue => PieceType.Blue,
            PieceColor.Yellow => PieceType.Yellow,
            PieceColor.Purple => PieceType.Purple,
            _ => throw new InvalidOperationException("Unsupported piece color.")
        };
    }

    private static IDomainEvent CreateBonusCreatedEvent(BonusToken bonus)
    {
        return bonus switch
        {
            LineBonus => new LineBonusCreated(bonus.Position),
            BombBonus => new BombBonusCreated(bonus.Position),
            _ => throw new InvalidOperationException("Unsupported bonus type.")
        };
    }

    private static IDomainEvent CreateBonusActivationEvent(BoardState board, BonusToken bonus)
    {
        return bonus switch
        {
            LineBonus line => new DestroyerSpawned(line.Position, BuildDestroyerPath(board, line)),
            BombBonus bomb => new BombExploded(bomb.Position, BuildExplosionArea(board, bomb)),
            _ => throw new InvalidOperationException("Unsupported bonus type.")
        };
    }

    private static IReadOnlyList<GridPosition> BuildDestroyerPath(BoardState board, LineBonus bonus)
    {
        var path = new List<GridPosition>();
        if (bonus.Orientation == LineOrientation.Horizontal)
        {
            for (var column = 0; column < board.Width; column++)
            {
                path.Add(new GridPosition(bonus.Position.Row, column));
            }
        }
        else
        {
            for (var row = 0; row < board.Height; row++)
            {
                path.Add(new GridPosition(row, bonus.Position.Column));
            }
        }

        return path;
    }

    private static IReadOnlyList<GridPosition> BuildExplosionArea(BoardState board, BombBonus bonus)
    {
        var affected = new List<GridPosition>();
        var fromRow = bonus.Position.Row - bonus.Radius;
        var toRow = bonus.Position.Row + bonus.Radius;
        var fromColumn = bonus.Position.Column - bonus.Radius;
        var toColumn = bonus.Position.Column + bonus.Radius;

        for (var row = fromRow; row <= toRow; row++)
        {
            if (row < 0 || row >= board.Height)
            {
                continue;
            }

            for (var column = fromColumn; column <= toColumn; column++)
            {
                if (column < 0 || column >= board.Width)
                {
                    continue;
                }

                affected.Add(new GridPosition(row, column));
            }
        }

        return affected;
    }

    private static TurnPipelineResult FinishWithGameOver(
        GameplayStateMachine stateMachine,
        List<IDomainEvent> events,
        bool applied)
    {
        stateMachine.TransitionToCheckingEndGame();
        stateMachine.TransitionToGameOver();
        events.Add(new GameEnded());
        return new TurnPipelineResult(applied, events);
    }

    private static void Swap(BoardState board, IDictionary<GridPosition, BonusToken> bonuses, Move move)
    {
        var from = board.GetCell(move.From);
        var to = board.GetCell(move.To);
        board.SetCell(move.From, to);
        board.SetCell(move.To, from);

        var hasFromBonus = bonuses.Remove(move.From, out var fromBonus);
        var hasToBonus = bonuses.Remove(move.To, out var toBonus);
        if (hasFromBonus && fromBonus is not null)
        {
            bonuses[move.To] = fromBonus with { Position = move.To };
        }

        if (hasToBonus && toBonus is not null)
        {
            bonuses[move.From] = toBonus with { Position = move.From };
        }
    }

    private sealed class BonusActivationSummary(
        IReadOnlyList<GridPosition> destroyedPositions,
        IReadOnlyList<BonusToken> activatedBonuses,
        IReadOnlySet<GridPosition> activatedBonusPositions)
    {
        public static BonusActivationSummary Empty { get; } = new([], [], new HashSet<GridPosition>());

        public IReadOnlyList<GridPosition> DestroyedPositions { get; } = destroyedPositions;

        public IReadOnlyList<BonusToken> ActivatedBonuses { get; } = activatedBonuses;

        public IReadOnlySet<GridPosition> ActivatedBonusPositions { get; } = activatedBonusPositions;
    }
}
