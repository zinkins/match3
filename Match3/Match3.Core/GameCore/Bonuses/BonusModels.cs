using System.Collections.Generic;
using Match3.Core.GameCore.Pieces;
using Match3.Core.GameCore.ValueObjects;

namespace Match3.Core.GameCore.Bonuses;

public abstract record BonusToken(BonusKind Kind, GridPosition Position, PieceColor Color);

public sealed record LineBonus(GridPosition Position, PieceColor Color, LineOrientation Orientation)
    : BonusToken(BonusKind.Line, Position, Color);

public sealed record BombBonus(GridPosition Position, PieceColor Color, int Radius = 1)
    : BonusToken(BonusKind.Bomb, Position, Color);

public sealed class Destroyer
{
    public Destroyer(
        IReadOnlyList<GridPosition> path,
        IReadOnlyList<GridPosition> destroyedPositions,
        IReadOnlyList<BonusToken> activatedBonuses)
    {
        Path = path;
        DestroyedPositions = destroyedPositions;
        ActivatedBonuses = activatedBonuses;
    }

    public IReadOnlyList<GridPosition> Path { get; }

    public IReadOnlyList<GridPosition> DestroyedPositions { get; }

    public IReadOnlyList<BonusToken> ActivatedBonuses { get; }
}

public sealed class ExplosionResult
{
    public ExplosionResult(
        IReadOnlyList<GridPosition> affectedArea,
        IReadOnlyList<GridPosition> destroyedPositions,
        IReadOnlyList<BonusToken> activatedBonuses)
    {
        AffectedArea = affectedArea;
        DestroyedPositions = destroyedPositions;
        ActivatedBonuses = activatedBonuses;
    }

    public IReadOnlyList<GridPosition> AffectedArea { get; }

    public IReadOnlyList<GridPosition> DestroyedPositions { get; }

    public IReadOnlyList<BonusToken> ActivatedBonuses { get; }
}

public sealed class BonusActivationResult
{
    public BonusActivationResult(
        IReadOnlyList<BonusToken> activatedBonuses,
        IReadOnlyList<GridPosition> destroyedPositions)
    {
        ActivatedBonuses = activatedBonuses;
        DestroyedPositions = destroyedPositions;
    }

    public IReadOnlyList<BonusToken> ActivatedBonuses { get; }

    public IReadOnlyList<GridPosition> DestroyedPositions { get; }
}
