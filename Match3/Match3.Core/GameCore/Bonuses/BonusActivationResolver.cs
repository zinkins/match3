using System.Collections.Generic;
using Match3.Core.GameCore.Board;
using Match3.Core.GameCore.ValueObjects;

namespace Match3.Core.GameCore.Bonuses;

public sealed class BonusActivationResolver
{
    private readonly LineBonusBehavior lineBehavior;
    private readonly BombBonusBehavior bombBehavior;

    public BonusActivationResolver()
        : this(new LineBonusBehavior(), new BombBonusBehavior())
    {
    }

    public BonusActivationResolver(LineBonusBehavior lineBehavior, BombBonusBehavior bombBehavior)
    {
        this.lineBehavior = lineBehavior;
        this.bombBehavior = bombBehavior;
    }

    public BonusActivationResult Resolve(BoardState board, BonusToken rootBonus)
    {
        var queue = new Queue<BonusToken>();
        var activatedPositions = new HashSet<GridPosition>();
        var destroyedPositions = new HashSet<GridPosition>();
        var activatedBonuses = new List<BonusToken>();

        queue.Enqueue(rootBonus);

        while (queue.Count > 0)
        {
            var next = queue.Dequeue();
            if (!activatedPositions.Add(next.Position))
            {
                continue;
            }

            activatedBonuses.Add(next);

            if (next is LineBonus line)
            {
                var destroyer = lineBehavior.Activate(line, board);
                AddDestroyed(destroyedPositions, destroyer.DestroyedPositions);
                Enqueue(activatedPositions, queue, destroyer.ActivatedBonuses);
                continue;
            }

            if (next is BombBonus bomb)
            {
                var explosion = bombBehavior.Activate(bomb, board);
                AddDestroyed(destroyedPositions, explosion.DestroyedPositions);
                Enqueue(activatedPositions, queue, explosion.ActivatedBonuses);
            }
        }

        return new BonusActivationResult(activatedBonuses, [.. destroyedPositions]);
    }

    private static void AddDestroyed(HashSet<GridPosition> destroyedPositions, IReadOnlyList<GridPosition> affected)
    {
        foreach (var position in affected)
        {
            destroyedPositions.Add(position);
        }
    }

    private static void Enqueue(
        HashSet<GridPosition> activatedPositions,
        Queue<BonusToken> queue,
        IReadOnlyList<BonusToken> triggered)
    {
        foreach (var bonus in triggered)
        {
            if (!activatedPositions.Contains(bonus.Position))
            {
                queue.Enqueue(bonus);
            }
        }
    }
}
