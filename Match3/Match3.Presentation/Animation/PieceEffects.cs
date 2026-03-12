using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Match3.Core.GameCore.Pieces;
using Match3.Core.GameCore.ValueObjects;
using Match3.Presentation.Rendering;

namespace Match3.Presentation.Animation;

public readonly record struct PieceEffectClock(float ElapsedSeconds, float DurationSeconds, bool Loop)
{
    public float Progress
    {
        get
        {
            if (DurationSeconds <= 0f)
            {
                return 1f;
            }

            var value = Loop
                ? (ElapsedSeconds % DurationSeconds) / DurationSeconds
                : MathF.Min(1f, ElapsedSeconds / DurationSeconds);
            return value;
        }
    }
}

public interface IPieceEffect
{
    RenderPiece Apply(RenderPiece piece, PieceEffectClock clock);
}

public sealed class CompositePieceEffect(params IPieceEffect[] effects) : IPieceEffect
{
    private readonly IReadOnlyList<IPieceEffect> effects = effects;

    public RenderPiece Apply(RenderPiece piece, PieceEffectClock clock)
    {
        return effects.Aggregate(piece, (current, effect) => effect.Apply(current, clock));
    }
}

public sealed class PulseScaleEffect(float amplitude) : IPieceEffect
{
    public RenderPiece Apply(RenderPiece piece, PieceEffectClock clock)
    {
        var scale = 1f + (amplitude * MathF.Sin(clock.Progress * MathF.PI));
        return ScalePiece(piece, scale);
    }

    public static RenderPiece ScalePiece(RenderPiece piece, float scale)
    {
        var width = piece.Width * scale;
        var height = piece.Height * scale;
        var x = piece.X - ((width - piece.Width) / 2f);
        var y = piece.Y - ((height - piece.Height) / 2f);
        return piece with { X = x, Y = y, Width = width, Height = height };
    }
}

public sealed class RotationEffect(float maxRadians) : IPieceEffect
{
    public RenderPiece Apply(RenderPiece piece, PieceEffectClock clock)
    {
        var rotation = maxRadians * MathF.Sin(clock.Progress * MathF.Tau);
        return piece with { Rotation = piece.Rotation + rotation };
    }
}

public sealed class SettleTransformEffect(float startScale, float startRotation) : IPieceEffect
{
    public RenderPiece Apply(RenderPiece piece, PieceEffectClock clock)
    {
        var t = 1f - Easing.SmoothStep(clock.Progress);
        var scaled = PulseScaleEffect.ScalePiece(piece, 1f + ((startScale - 1f) * t));
        return scaled with { Rotation = scaled.Rotation + (startRotation * t) };
    }
}

public sealed class MovePieceEffect(Vector2 from, Vector2 to) : IPieceEffect
{
    public RenderPiece Apply(RenderPiece piece, PieceEffectClock clock)
    {
        var offset = Vector2.Lerp(from, to, Easing.SmoothStep(clock.Progress));
        return piece with { X = offset.X, Y = offset.Y };
    }
}

public sealed class SequencePieceEffect(params (IPieceEffect effect, float durationSeconds)[] segments) : IPieceEffect
{
    private readonly (IPieceEffect effect, float durationSeconds)[] segments = segments;

    public RenderPiece Apply(RenderPiece piece, PieceEffectClock clock)
    {
        var elapsed = clock.ElapsedSeconds;
        foreach (var (effect, duration) in segments)
        {
            if (elapsed <= duration)
            {
                return effect.Apply(piece, new PieceEffectClock(elapsed, duration, Loop: false));
            }

            piece = effect.Apply(piece, new PieceEffectClock(duration, duration, Loop: false));
            elapsed -= duration;
        }

        return piece;
    }
}

public sealed class TimedPieceEffect
{
    private readonly IPieceEffect effect;

    public TimedPieceEffect(
        GridPosition? position,
        RenderPiece piece,
        IPieceEffect effect,
        float durationSeconds,
        float delaySeconds,
        bool hideBasePiece,
        bool hideBasePieceBeforeStart = false)
    {
        Position = position;
        Piece = piece;
        this.effect = effect;
        DurationSeconds = durationSeconds;
        DelaySeconds = delaySeconds;
        HideBasePiece = hideBasePiece;
        HideBasePieceBeforeStart = hideBasePieceBeforeStart;
    }

    public GridPosition? Position { get; }

    public RenderPiece Piece { get; }

    public float DurationSeconds { get; }

    public float DelaySeconds { get; }

    public bool HideBasePiece { get; }

    public bool HideBasePieceBeforeStart { get; }

    public float ElapsedSeconds { get; private set; }

    public bool IsStarted => ElapsedSeconds >= DelaySeconds;

    public bool IsCompleted => ElapsedSeconds >= DelaySeconds + DurationSeconds;

    public void Update(float deltaSeconds)
    {
        ElapsedSeconds += deltaSeconds;
    }

    public RenderPiece BuildPiece()
    {
        var localElapsed = MathF.Max(0f, ElapsedSeconds - DelaySeconds);
        return effect.Apply(Piece, new PieceEffectClock(localElapsed, DurationSeconds, Loop: false));
    }
}

public sealed class TimedVisualEffect(Func<float, RenderPiece> buildPiece, float durationSeconds)
{
    private readonly Func<float, RenderPiece> buildPiece = buildPiece;

    public float DurationSeconds { get; } = durationSeconds;

    public float ElapsedSeconds { get; private set; }

    public bool IsCompleted => ElapsedSeconds >= DurationSeconds;

    public void Update(float deltaSeconds)
    {
        ElapsedSeconds += deltaSeconds;
    }

    public RenderPiece BuildPiece()
    {
        var progress = DurationSeconds <= 0f ? 1f : MathF.Min(1f, ElapsedSeconds / DurationSeconds);
        return buildPiece(progress);
    }
}

public sealed class TimedHiddenCells(IReadOnlyCollection<GridPosition> positions, float durationSeconds, float delaySeconds = 0f)
{
    public IReadOnlyCollection<GridPosition> Positions { get; } = positions;

    public float DurationSeconds { get; } = durationSeconds;

    public float DelaySeconds { get; } = delaySeconds;

    public float ElapsedSeconds { get; private set; }

    public bool IsStarted => ElapsedSeconds >= DelaySeconds;

    public bool IsCompleted => ElapsedSeconds >= DelaySeconds + DurationSeconds;

    public bool Contains(GridPosition position)
    {
        return IsStarted && Positions.Contains(position);
    }

    public void Update(float deltaSeconds)
    {
        ElapsedSeconds += deltaSeconds;
    }
}

public sealed class TimedPathClearEffect(IReadOnlyList<GridPosition> path, float durationSeconds, float delaySeconds = 0f)
{
    public IReadOnlyList<GridPosition> Path { get; } = path;

    public float DurationSeconds { get; } = durationSeconds;

    public float DelaySeconds { get; } = delaySeconds;

    public float ElapsedSeconds { get; private set; }

    public bool IsCompleted => ElapsedSeconds >= DelaySeconds + DurationSeconds;

    public void Update(float deltaSeconds)
    {
        ElapsedSeconds += deltaSeconds;
    }

    public bool Contains(GridPosition position)
    {
        if (ElapsedSeconds < DelaySeconds || Path.Count == 0)
        {
            return false;
        }

        var pathIndex = -1;
        for (var i = 0; i < Path.Count; i++)
        {
            if (Path[i] == position)
            {
                pathIndex = i;
                break;
            }
        }

        if (pathIndex < 0)
        {
            return false;
        }

        if (Path.Count == 1)
        {
            return true;
        }

        var progress = MathF.Min(1f, (ElapsedSeconds - DelaySeconds) / DurationSeconds);
        var reachedIndex = (int)MathF.Floor(progress * (Path.Count - 1));
        return pathIndex <= reachedIndex;
    }
}

public sealed class GameplayEffectsController
{
    private readonly CompositePieceEffect selectedEffect = new(
        new PulseScaleEffect(0.12f),
        new RotationEffect(0.18f));

    private readonly Dictionary<GridPosition, TimedPieceEffect> cellEffects = [];
    private readonly List<TimedPieceEffect> overlayEffects = [];
    private readonly List<TimedVisualEffect> visualEffects = [];
    private readonly List<TimedHiddenCells> hiddenCells = [];
    private readonly List<TimedPathClearEffect> pathClearEffects = [];
    private GridPosition? lastSelectedCell;
    private float totalSeconds;

    public bool HasActiveBlockingEffects => overlayEffects.Count > 0;

    public void Update(TimeSpan elapsed)
    {
        var delta = (float)elapsed.TotalSeconds;
        totalSeconds += delta;

        foreach (var effect in cellEffects.Values)
        {
            effect.Update(delta);
        }

        for (var i = overlayEffects.Count - 1; i >= 0; i--)
        {
            overlayEffects[i].Update(delta);
            if (overlayEffects[i].IsCompleted)
            {
                overlayEffects.RemoveAt(i);
            }
        }

        for (var i = visualEffects.Count - 1; i >= 0; i--)
        {
            visualEffects[i].Update(delta);
            if (visualEffects[i].IsCompleted)
            {
                visualEffects.RemoveAt(i);
            }
        }

        for (var i = hiddenCells.Count - 1; i >= 0; i--)
        {
            hiddenCells[i].Update(delta);
            if (hiddenCells[i].IsCompleted)
            {
                hiddenCells.RemoveAt(i);
            }
        }

        for (var i = pathClearEffects.Count - 1; i >= 0; i--)
        {
            pathClearEffects[i].Update(delta);
            if (pathClearEffects[i].IsCompleted)
            {
                pathClearEffects.RemoveAt(i);
            }
        }

        foreach (var completed in cellEffects.Where(pair => pair.Value.IsCompleted).Select(pair => pair.Key).ToArray())
        {
            cellEffects.Remove(completed);
        }
    }

    public void SyncSelection(GridPosition? selectedCell, BoardRenderSnapshot snapshot)
    {
        if (lastSelectedCell == selectedCell)
        {
            return;
        }

        if (lastSelectedCell is { } previous &&
            snapshot.Pieces.FirstOrDefault(piece => piece.Position == previous) is { } piece)
        {
            cellEffects[previous] = new TimedPieceEffect(
                previous,
                piece,
                new SettleTransformEffect(1.12f, 0.18f),
                durationSeconds: 0.22f,
                delaySeconds: 0f,
                hideBasePiece: false);
        }

        lastSelectedCell = selectedCell;
    }

    public IReadOnlyList<RenderPiece> BuildPieces(BoardRenderSnapshot snapshot, GridPosition? selectedCell)
    {
        SyncSelection(selectedCell, snapshot);

        var pieces = new List<RenderPiece>(snapshot.Pieces.Count + overlayEffects.Count);
        foreach (var piece in snapshot.Pieces)
        {
            if (overlayEffects.Any(effect => effect.HideBasePiece && effect.Position == piece.Position && (effect.IsStarted || effect.HideBasePieceBeforeStart)))
            {
                continue;
            }

            if (hiddenCells.Any(effect => effect.Contains(piece.Position)))
            {
                continue;
            }

            if (pathClearEffects.Any(effect => effect.Contains(piece.Position)))
            {
                continue;
            }

            var current = piece;
            if (selectedCell == piece.Position)
            {
                current = selectedEffect.Apply(current, new PieceEffectClock(totalSeconds, 1.5f, Loop: true));
                current = current with { Layer = 10f };
            }
            else if (cellEffects.TryGetValue(piece.Position, out var effect) && effect.IsStarted)
            {
                current = effect.BuildPiece();
            }

            pieces.Add(current);
        }

        foreach (var overlay in overlayEffects.Where(effect => effect.IsStarted))
        {
            pieces.Add(overlay.BuildPiece());
        }

        foreach (var visualEffect in visualEffects)
        {
            pieces.Add(visualEffect.BuildPiece());
        }

        return pieces.OrderBy(piece => piece.Layer).ToArray();
    }

    public void QueueDestroyer(GridPosition origin, IReadOnlyList<GridPosition> path, BoardTransform transform)
    {
        if (path.Count == 0)
        {
            return;
        }

        var launchIndex = Array.IndexOf(path.ToArray(), origin);
        if (launchIndex < 0)
        {
            launchIndex = path.Count / 2;
        }

        var forwardPath = BuildWorldPath(path.Skip(launchIndex), transform);
        var backwardPath = BuildWorldPath(path.Take(launchIndex + 1).Reverse(), transform);
        var size = transform.CellSize * 0.42f;
        pathClearEffects.Add(new TimedPathClearEffect(path.Skip(launchIndex).ToArray(), 0.8f));
        pathClearEffects.Add(new TimedPathClearEffect(path.Take(launchIndex + 1).Reverse().ToArray(), 0.8f));

        QueueDestroyerVisual(forwardPath, size);
        QueueDestroyerVisual(backwardPath, size);
    }

    public void QueueExplosion(IReadOnlyList<GridPosition> area, BoardTransform transform)
    {
        if (area.Count == 0)
        {
            return;
        }

        var centerRow = (float)area.Average(position => position.Row);
        var centerColumn = (float)area.Average(position => position.Column);
        var centerWorld = transform.GridToWorld(new GridPosition((int)MathF.Round(centerRow), (int)MathF.Round(centerColumn)));
        var center = new Vector2(centerWorld.X + (transform.CellSize / 2f), centerWorld.Y + (transform.CellSize / 2f));
        var maxSize = transform.CellSize * 1.9f;
        hiddenCells.Add(new TimedHiddenCells(area.ToArray(), 0.45f));

        visualEffects.Add(new TimedVisualEffect(
            progress =>
            {
                var size = maxSize * Easing.SmoothStep(progress);
                return new RenderPiece(
                    new GridPosition(-1, -1),
                    PieceVisualConstants.ShapeCircle,
                    PieceVisualConstants.TintOrange,
                    center.X - (size / 2f),
                    center.Y - (size / 2f),
                    size,
                    size,
                    Rotation: 0f,
                    Layer: 25f);
            },
            durationSeconds: 0.45f));
    }

    private void QueueDestroyerVisual(IReadOnlyList<Vector2> worldPath, float size)
    {
        if (worldPath.Count <= 1)
        {
            return;
        }

        var animation = new DestroyerAnimation(worldPath);
        visualEffects.Add(new TimedVisualEffect(
            progress =>
            {
                var center = animation.Evaluate(progress);
                return new RenderPiece(
                    new GridPosition(-1, -1),
                    PieceVisualConstants.ShapeDiamond,
                    PieceVisualConstants.TintWhite,
                    center.X - (size / 2f),
                    center.Y - (size / 2f),
                    size,
                    size,
                    Rotation: progress * MathF.PI * 2f,
                    Layer: 30f);
            },
            durationSeconds: 0.8f));
    }

    private static IReadOnlyList<Vector2> BuildWorldPath(IEnumerable<GridPosition> path, BoardTransform transform)
    {
        return path
            .Select(position =>
            {
                var world = transform.GridToWorld(position);
                return new Vector2(world.X + (transform.CellSize / 2f), world.Y + (transform.CellSize / 2f));
            })
            .ToArray();
    }

    public void QueueSwap(BoardRenderSnapshot snapshot, Move move, bool rollback)
    {
        var fromPiece = snapshot.Pieces.FirstOrDefault(piece => piece.Position == move.From);
        var toPiece = snapshot.Pieces.FirstOrDefault(piece => piece.Position == move.To);
        if (fromPiece is null || toPiece is null)
        {
            return;
        }

        if (rollback)
        {
            overlayEffects.Add(new TimedPieceEffect(
                move.From,
                fromPiece with { Layer = 20f },
                new SequencePieceEffect(
                    (new MovePieceEffect(new Vector2(fromPiece.X, fromPiece.Y), new Vector2(toPiece.X, toPiece.Y)), 0.18f),
                    (new MovePieceEffect(new Vector2(toPiece.X, toPiece.Y), new Vector2(fromPiece.X, fromPiece.Y)), 0.18f)),
                durationSeconds: 0.36f,
                delaySeconds: 0f,
                hideBasePiece: true));

            overlayEffects.Add(new TimedPieceEffect(
                move.To,
                toPiece,
                new SequencePieceEffect(
                    (new MovePieceEffect(new Vector2(toPiece.X, toPiece.Y), new Vector2(fromPiece.X, fromPiece.Y)), 0.18f),
                    (new MovePieceEffect(new Vector2(fromPiece.X, fromPiece.Y), new Vector2(toPiece.X, toPiece.Y)), 0.18f)),
                durationSeconds: 0.36f,
                delaySeconds: 0f,
                hideBasePiece: true));
            return;
        }

        overlayEffects.Add(new TimedPieceEffect(
            move.To,
            fromPiece with { Layer = 20f },
            new MovePieceEffect(new Vector2(fromPiece.X, fromPiece.Y), new Vector2(toPiece.X, toPiece.Y)),
            durationSeconds: 0.22f,
            delaySeconds: 0f,
            hideBasePiece: true));

        overlayEffects.Add(new TimedPieceEffect(
            move.From,
            toPiece,
            new MovePieceEffect(new Vector2(toPiece.X, toPiece.Y), new Vector2(fromPiece.X, fromPiece.Y)),
            durationSeconds: 0.22f,
            delaySeconds: 0f,
            hideBasePiece: true));
    }

    public void QueueBoardSettle(BoardRenderSnapshot beforeSnapshot, BoardRenderSnapshot afterSnapshot, float cellSize, float initialDelaySeconds = 0f, IReadOnlyList<GridPosition>? createdBonusOrigins = null)
    {
        for (var column = 0; column < 8; column++)
        {
            var beforeColumn = beforeSnapshot.Pieces
                .Where(piece => piece.Position.Column == column)
                .OrderBy(piece => piece.Position.Row)
                .ToArray();
            var afterColumn = afterSnapshot.Pieces
                .Where(piece => piece.Position.Column == column)
                .OrderBy(piece => piece.Position.Row)
                .ToArray();
            var survivorMap = MatchColumnSurvivors(beforeColumn, afterColumn);
            var spawnCount = 0;

            foreach (var target in afterColumn.OrderByDescending(piece => piece.Position.Row))
            {
                Vector2 from;
                if (survivorMap.TryGetValue(target.Position, out var source))
                {
                    if (source.Position == target.Position)
                    {
                        continue;
                    }

                    from = new Vector2(source.X, source.Y);
                }
                else
                {
                    spawnCount++;
                    from = new Vector2(target.X, target.Y - (cellSize * (spawnCount + 1)));
                }

                overlayEffects.Add(new TimedPieceEffect(
                    target.Position,
                    target with { X = from.X, Y = from.Y, Layer = 15f },
                    new MovePieceEffect(from, new Vector2(target.X, target.Y)),
                    durationSeconds: source is not null ? 0.65f : 0.75f,
                    delaySeconds: initialDelaySeconds + (source is not null ? 0f : 0.4f),
                    hideBasePiece: true,
                    hideBasePieceBeforeStart: source is null));
            }
        }
    }

    private static Dictionary<GridPosition, RenderPiece> MatchColumnSurvivors(
        IReadOnlyList<RenderPiece> beforeColumn,
        IReadOnlyList<RenderPiece> afterColumn)
    {
        var lengths = new int[beforeColumn.Count + 1, afterColumn.Count + 1];
        for (var beforeIndex = beforeColumn.Count - 1; beforeIndex >= 0; beforeIndex--)
        {
            for (var afterIndex = afterColumn.Count - 1; afterIndex >= 0; afterIndex--)
            {
                lengths[beforeIndex, afterIndex] = CanMatchAsFallingPiece(beforeColumn[beforeIndex], afterColumn[afterIndex])
                    ? lengths[beforeIndex + 1, afterIndex + 1] + 1
                    : Math.Max(lengths[beforeIndex + 1, afterIndex], lengths[beforeIndex, afterIndex + 1]);
            }
        }

        var result = new Dictionary<GridPosition, RenderPiece>();
        var i = 0;
        var j = 0;
        while (i < beforeColumn.Count && j < afterColumn.Count)
        {
            if (CanMatchAsFallingPiece(beforeColumn[i], afterColumn[j]))
            {
                result[afterColumn[j].Position] = beforeColumn[i];
                i++;
                j++;
                continue;
            }

            if (lengths[i + 1, j] >= lengths[i, j + 1])
            {
                i++;
            }
            else
            {
                j++;
            }
        }

        return result;
    }

    private static bool CanMatchAsFallingPiece(RenderPiece before, RenderPiece after)
    {
        return before.Shape == after.Shape &&
            before.Tint == after.Tint &&
            before.Position.Row <= after.Position.Row;
    }
}