using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Match3.Core.GameCore.ValueObjects;
using Match3.Presentation.Animation.Engine;
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

public sealed class GameplayVisualState
{
    private GridPosition? lastSelectedCell;
    private GridPosition? suppressedSelectedCell;

    public NodeId? GetSelectedNodeId(BoardViewState viewState)
    {
        ArgumentNullException.ThrowIfNull(viewState);

        return lastSelectedCell is { } selectedCell && viewState.GetPieceNode(selectedCell) is { } selectedNode
            ? selectedNode.Id
            : null;
    }

    public void SuppressSelectionIfNeeded(NodeId? selectedNodeIdBeforeSettle, IReadOnlySet<NodeId> retainedNodeIds)
    {
        ArgumentNullException.ThrowIfNull(retainedNodeIds);

        if (lastSelectedCell is { } selectedCell &&
            selectedNodeIdBeforeSettle is { } selectedNodeId &&
            !retainedNodeIds.Contains(selectedNodeId))
        {
            suppressedSelectedCell = selectedCell;
            lastSelectedCell = null;
        }
    }

    public void SyncSelection(GridPosition? selectedCell, BoardRenderSnapshot snapshot, BoardViewState? viewState, AnimationPlayer? animationPlayer)
    {
        var effectiveSelectedCell = NormalizeSelectedCell(selectedCell);
        if (lastSelectedCell == effectiveSelectedCell || viewState is null || animationPlayer is null)
        {
            lastSelectedCell = effectiveSelectedCell;
            return;
        }

        if (lastSelectedCell is { } previous && viewState.GetPieceNode(previous) is { } previousNode)
        {
            animationPlayer.Play(
                Anim.Parallel(
                    Anim.ScaleTo(previousNode, new Vector2(1f, 1f), 0.12f),
                    Anim.RotateTo(previousNode, 0f, 0.12f)),
                ChannelConflictPolicy.Replace);
        }

        if (effectiveSelectedCell is { } current && viewState.GetPieceNode(current) is { } selectedNode)
        {
            animationPlayer.Play(
                Anim.Parallel(
                    Anim.ScaleTo(selectedNode, new Vector2(1.12f, 1.12f), 0.12f),
                    Anim.RotateTo(selectedNode, 0.18f, 0.12f)),
                ChannelConflictPolicy.Replace);
        }

        lastSelectedCell = effectiveSelectedCell;
    }

    public IReadOnlyList<RenderPiece> BuildPieces(BoardRenderSnapshot snapshot, GridPosition? selectedCell, BoardViewState? viewState = null, AnimationPlayer? animationPlayer = null)
    {
        var effectiveSelectedCell = NormalizeSelectedCell(selectedCell);
        SyncSelection(effectiveSelectedCell, snapshot, viewState, animationPlayer);

        var effectNodeCount = viewState?.EffectNodes.Count ?? 0;
        var pieces = new List<RenderPiece>(snapshot.Pieces.Count + effectNodeCount);
        foreach (var piece in snapshot.Pieces)
        {
            if (viewState?.IsCellHidden(piece.Position) == true)
            {
                continue;
            }

            var current = effectiveSelectedCell == piece.Position
                ? piece with { Layer = 10f }
                : piece;
            pieces.Add(current);
        }

        if (viewState is not null)
        {
            foreach (var effectNode in viewState.EffectNodes.Where(node => node.IsVisible))
            {
                pieces.Add(BuildEffectPiece(effectNode));
            }
        }

        return pieces.OrderBy(piece => piece.Layer).ToArray();
    }

    private GridPosition? NormalizeSelectedCell(GridPosition? selectedCell)
    {
        if (suppressedSelectedCell is { } suppressed && selectedCell != suppressed)
        {
            suppressedSelectedCell = null;
        }

        return selectedCell == suppressedSelectedCell
            ? null
            : selectedCell;
    }

    private static RenderPiece BuildEffectPiece(EffectNode effectNode)
    {
        var width = effectNode.Width * effectNode.Scale.X;
        var height = effectNode.Height * effectNode.Scale.Y;
        var x = effectNode.Position.X - ((width - effectNode.Width) / 2f);
        var y = effectNode.Position.Y - ((height - effectNode.Height) / 2f);
        return new RenderPiece(
            effectNode.LogicalCell,
            effectNode.Shape,
            effectNode.Tint,
            x,
            y,
            width,
            height,
            effectNode.Rotation,
            effectNode.Layer);
    }
}
