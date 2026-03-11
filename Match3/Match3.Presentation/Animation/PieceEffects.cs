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
        bool hideBasePiece)
    {
        Position = position;
        Piece = piece;
        this.effect = effect;
        DurationSeconds = durationSeconds;
        DelaySeconds = delaySeconds;
        HideBasePiece = hideBasePiece;
    }

    public GridPosition? Position { get; }

    public RenderPiece Piece { get; }

    public float DurationSeconds { get; }

    public float DelaySeconds { get; }

    public bool HideBasePiece { get; }

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

public sealed class GameplayEffectsController
{
    private readonly CompositePieceEffect selectedEffect = new(
        new PulseScaleEffect(0.12f),
        new RotationEffect(0.18f));

    private readonly Dictionary<GridPosition, TimedPieceEffect> cellEffects = [];
    private readonly List<TimedPieceEffect> overlayEffects = [];
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
            if (overlayEffects.Any(effect => effect.HideBasePiece && effect.Position == piece.Position))
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

        return pieces.OrderBy(piece => piece.Layer).ToArray();
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

    public void QueueBoardSettle(BoardRenderSnapshot beforeSnapshot, BoardRenderSnapshot afterSnapshot, float cellSize)
    {
        for (var column = 0; column < 8; column++)
        {
            var usedSources = new HashSet<GridPosition>();
            var spawnCount = 0;

            foreach (var target in afterSnapshot.Pieces
                .Where(piece => piece.Position.Column == column)
                .OrderByDescending(piece => piece.Position.Row))
            {
                var source = beforeSnapshot.Pieces
                    .Where(piece => piece.Position.Column == column &&
                                    piece.Tint == target.Tint &&
                                    piece.Position.Row <= target.Position.Row &&
                                    !usedSources.Contains(piece.Position))
                    .OrderByDescending(piece => piece.Position.Row)
                    .FirstOrDefault();

                Vector2 from;
                if (source is not null && source.Position != target.Position)
                {
                    usedSources.Add(source.Position);
                    from = new Vector2(source.X, source.Y);
                }
                else if (source is not null && source.Position == target.Position)
                {
                    continue;
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
                    durationSeconds: 0.45f,
                    delaySeconds: 0.22f,
                    hideBasePiece: true));
            }
        }
    }
}
