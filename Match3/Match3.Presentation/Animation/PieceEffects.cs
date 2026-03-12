using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Match3.Core.GameCore.Pieces;
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
    private readonly Dictionary<GridPosition, TimedPieceEffect> cellEffects = [];
    private readonly List<TimedPieceEffect> overlayEffects = [];
    private GridPosition? lastSelectedCell;
    private GridPosition? suppressedSelectedCell;

    public bool HasActiveBlockingEffects => overlayEffects.Count > 0;

    public void Update(TimeSpan elapsed)
    {
        var delta = (float)elapsed.TotalSeconds;

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

    public void SyncSelection(GridPosition? selectedCell, BoardRenderSnapshot snapshot, BoardViewState? viewState, AnimationPlayer? animationPlayer)
    {
        var effectiveSelectedCell = NormalizeSelectedCell(selectedCell);
        if (lastSelectedCell == effectiveSelectedCell || viewState is null || animationPlayer is null)
        {
            lastSelectedCell = effectiveSelectedCell;
            return;
        }

        if (lastSelectedCell is { } previous)
        {
            if (viewState.GetPieceNode(previous) is { } previousNode)
            {
                animationPlayer.Play(
                    Anim.Parallel(
                        Anim.ScaleTo(previousNode, new Vector2(1f, 1f), 0.12f),
                        Anim.RotateTo(previousNode, 0f, 0.12f)),
                    ChannelConflictPolicy.Replace);
            }
            else if (snapshot.Pieces.FirstOrDefault(piece => piece.Position == previous) is { } piece)
            {
                cellEffects[previous] = new TimedPieceEffect(
                    previous,
                    piece,
                    new SettleTransformEffect(1.12f, 0.18f),
                    durationSeconds: 0.22f,
                    delaySeconds: 0f,
                    hideBasePiece: false);
            }
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
        var pieces = new List<RenderPiece>(snapshot.Pieces.Count + overlayEffects.Count + effectNodeCount);
        foreach (var piece in snapshot.Pieces)
        {
            if (overlayEffects.Any(effect => effect.HideBasePiece && effect.Position == piece.Position && effect.IsStarted))
            {
                continue;
            }

            if (viewState?.IsCellHidden(piece.Position) == true)
            {
                continue;
            }

            var current = piece;
            if (effectiveSelectedCell == piece.Position)
            {
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

    public void QueueDestroyer(BoardViewState viewState, AnimationPlayer animationPlayer, GridPosition origin, IReadOnlyList<GridPosition> path, BoardTransform transform)
    {
        ArgumentNullException.ThrowIfNull(viewState);
        ArgumentNullException.ThrowIfNull(animationPlayer);

        if (path.Count == 0)
        {
            return;
        }

        var launchIndex = Array.IndexOf(path.ToArray(), origin);
        if (launchIndex < 0)
        {
            launchIndex = path.Count / 2;
        }

        var size = transform.CellSize * 0.42f;
        var forwardPath = path.Skip(launchIndex).ToArray();
        var backwardPath = path.Take(launchIndex + 1).Reverse().ToArray();
        QueueDestroyerVisual(viewState, animationPlayer, forwardPath, transform, size);
        QueueDestroyerVisual(viewState, animationPlayer, backwardPath, transform, size);
    }

    public void QueueExplosion(BoardViewState viewState, AnimationPlayer animationPlayer, IReadOnlyList<GridPosition> area, BoardTransform transform)
    {
        ArgumentNullException.ThrowIfNull(viewState);
        ArgumentNullException.ThrowIfNull(animationPlayer);

        if (area.Count == 0)
        {
            return;
        }

        var centerRow = (float)area.Average(position => position.Row);
        var centerColumn = (float)area.Average(position => position.Column);
        var centerWorld = transform.GridToWorld(new GridPosition((int)MathF.Round(centerRow), (int)MathF.Round(centerColumn)));
        var center = new Vector2(centerWorld.X + (transform.CellSize / 2f), centerWorld.Y + (transform.CellSize / 2f));
        var maxSize = transform.CellSize * 1.9f;
        var initialPosition = new Vector2(center.X - (maxSize / 2f), center.Y - (maxSize / 2f));
        var effectNode = new EffectNode(
            NodeId.New(),
            new GridPosition(-1, -1),
            initialPosition,
            new Vector2(0.1f, 0.1f),
            rotation: 0f,
            opacity: 1f,
            tint: PieceVisualConstants.TintOrange,
            glow: 0f,
            isVisible: true,
            shape: PieceVisualConstants.ShapeCircle,
            width: maxSize,
            height: maxSize,
            layer: 25f);
        viewState.AddOrUpdate(effectNode);

        var areaCells = area.ToArray();
        var animation = Anim.Sequence()
            .Append(new CallbackAnimation(() => viewState.HideCells(areaCells)))
            .Append(Anim.Parallel(
                Anim.ScaleTo(effectNode, new Vector2(1f, 1f), 0.45f),
                Anim.FadeTo(effectNode, 0f, 0.45f)))
            .Append(new CallbackAnimation(() =>
            {
                viewState.ShowCells(areaCells);
                viewState.RemoveEffectNode(effectNode.Id);
            }));

        animationPlayer.Play(animation, ChannelConflictPolicy.Replace);
    }

    private void QueueDestroyerVisual(BoardViewState viewState, AnimationPlayer animationPlayer, IReadOnlyList<GridPosition> path, BoardTransform transform, float size)
    {
        if (path.Count <= 1)
        {
            return;
        }

        var centers = BuildWorldPath(path, transform);
        var initialPosition = new Vector2(centers[0].X - (size / 2f), centers[0].Y - (size / 2f));
        var effectNode = new EffectNode(
            NodeId.New(),
            path[0],
            initialPosition,
            new Vector2(1f, 1f),
            rotation: 0f,
            opacity: 1f,
            tint: PieceVisualConstants.TintWhite,
            glow: 0f,
            isVisible: true,
            shape: PieceVisualConstants.ShapeDiamond,
            width: size,
            height: size,
            layer: 30f);
        viewState.AddOrUpdate(effectNode);

        var segmentDuration = 0.8f / (path.Count - 1);
        var pathCells = path.ToArray();
        var animation = Anim.Sequence()
            .Append(new CallbackAnimation(() => viewState.HideCells([pathCells[0]])));

        for (var i = 1; i < pathCells.Length; i++)
        {
            var targetCenter = centers[i];
            var targetPosition = new Vector2(targetCenter.X - (size / 2f), targetCenter.Y - (size / 2f));
            var cell = pathCells[i];
            animation
                .Append(Anim.MoveTo(effectNode, targetPosition, segmentDuration))
                .Append(new CallbackAnimation(() => viewState.HideCells([cell])));
        }

        animation.Append(new CallbackAnimation(() =>
        {
            viewState.ShowCells(pathCells);
            viewState.RemoveEffectNode(effectNode.Id);
        }));

        animationPlayer.Play(animation, ChannelConflictPolicy.Replace);
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

    public void QueueSwap(BoardViewState viewState, AnimationPlayer animationPlayer, Move move, BoardTransform transform, bool rollback)
    {
        ArgumentNullException.ThrowIfNull(viewState);
        ArgumentNullException.ThrowIfNull(animationPlayer);

        var fromNode = viewState.GetPieceNode(move.From);
        var toNode = viewState.GetPieceNode(move.To);
        if (fromNode is null || toNode is null)
        {
            return;
        }

        var fromPosition = fromNode.Position;
        var toPosition = toNode.Position;
        if (rollback)
        {
            var rollbackSequence = Anim.Sequence()
                .Append(Anim.Parallel(
                    Anim.MoveTo(fromNode, toPosition, 0.18f, blocksInput: true),
                    Anim.MoveTo(toNode, fromPosition, 0.18f, blocksInput: true)))
                .Append(Anim.Parallel(
                    Anim.MoveTo(fromNode, fromPosition, 0.18f, blocksInput: true),
                    Anim.MoveTo(toNode, toPosition, 0.18f, blocksInput: true)));
            animationPlayer.Play(rollbackSequence, ChannelConflictPolicy.Replace);
            return;
        }

        fromNode.LogicalCell = move.To;
        toNode.LogicalCell = move.From;
        animationPlayer.Play(
            Anim.Parallel(
                Anim.MoveTo(fromNode, toPosition, 0.22f, blocksInput: true),
                Anim.MoveTo(toNode, fromPosition, 0.22f, blocksInput: true)),
            ChannelConflictPolicy.Replace);
    }

    public void QueueBoardSettle(BoardViewState viewState, AnimationPlayer animationPlayer, BoardRenderSnapshot beforeSnapshot, BoardRenderSnapshot afterSnapshot, float cellSize, float initialDelaySeconds = 0f, IReadOnlyList<GridPosition>? excludedTargets = null)
    {
        ArgumentNullException.ThrowIfNull(viewState);
        ArgumentNullException.ThrowIfNull(animationPlayer);

        var selectedNodeIdBeforeSettle = lastSelectedCell is { } selectedCellBeforeSettle && viewState.GetPieceNode(selectedCellBeforeSettle) is { } selectedNodeBeforeSettle
            ? selectedNodeBeforeSettle.Id
            : (NodeId?)null;
        var retainedNodeIds = new HashSet<NodeId>();
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
                var targetPosition = new Vector2(target.X, target.Y);
                var isSurvivor = survivorMap.TryGetValue(target.Position, out var source);
                if (isSurvivor && source!.Position == target.Position)
                {
                    var stationaryNode = viewState.GetPieceNode(target.Position);
                    if (stationaryNode is null)
                    {
                        stationaryNode = CreatePieceNode(target, targetPosition);
                        viewState.AddOrUpdate(stationaryNode);
                    }

                    stationaryNode.Position = targetPosition;
                    stationaryNode.Tint = target.Tint;
                    retainedNodeIds.Add(stationaryNode.Id);
                    continue;
                }

                PieceNode node;
                Vector2 from;
                float durationSeconds;
                float delaySeconds;

                if (isSurvivor)
                {
                    node = viewState.GetPieceNode(source!.Position) ?? CreatePieceNode(source, new Vector2(source.X, source.Y));
                    from = node.Position;
                    durationSeconds = 0.65f;
                    delaySeconds = initialDelaySeconds;
                }
                else
                {
                    if (excludedTargets?.Contains(target.Position) == true)
                    {
                        if (viewState.GetPieceNode(target.Position) is { } existingExcludedNode)
                        {
                            existingExcludedNode.Position = targetPosition;
                            existingExcludedNode.Tint = target.Tint;
                            retainedNodeIds.Add(existingExcludedNode.Id);
                        }

                        continue;
                    }

                    spawnCount++;
                    from = new Vector2(target.X, target.Y - (cellSize * (spawnCount + 1)));
                    node = CreatePieceNode(target, from);
                    durationSeconds = 0.75f;
                    delaySeconds = initialDelaySeconds + 0.4f;
                }

                node.LogicalCell = target.Position;
                node.Tint = target.Tint;
                node.Position = from;
                node.IsVisible = true;
                viewState.AddOrUpdate(node);
                retainedNodeIds.Add(node.Id);

                var animation = Anim.Sequence();
                if (delaySeconds > 0f)
                {
                    animation.Append(new DelayAnimation(delaySeconds, blocksInput: true));
                }

                animation.Append(Anim.MoveTo(node, targetPosition, durationSeconds, blocksInput: true));
                animationPlayer.Play(animation, ChannelConflictPolicy.Replace);
            }
        }

        if (lastSelectedCell is { } selectedCell && selectedNodeIdBeforeSettle is { } selectedNodeId && !retainedNodeIds.Contains(selectedNodeId))
        {
            suppressedSelectedCell = selectedCell;
            lastSelectedCell = null;
        }

        viewState.RemoveNodesExcept(retainedNodeIds);
    }

    public void QueueCreatedBonuses(BoardViewState viewState, AnimationPlayer animationPlayer, BoardRenderSnapshot afterSnapshot, float cellSize, float initialDelaySeconds = 0f, IReadOnlyList<GridPosition>? createdBonusOrigins = null)
    {
        ArgumentNullException.ThrowIfNull(viewState);
        ArgumentNullException.ThrowIfNull(animationPlayer);

        if (createdBonusOrigins is null || createdBonusOrigins.Count == 0)
        {
            return;
        }

        foreach (var target in afterSnapshot.Pieces.Where(IsBonusPiece))
        {
            var createdBonusOrigin = createdBonusOrigins
                .Where(position => position.Column == target.Position.Column && position.Row <= target.Position.Row)
                .OrderByDescending(position => position.Row)
                .FirstOrDefault();
            if (createdBonusOrigin == default && !createdBonusOrigins.Contains(createdBonusOrigin))
            {
                continue;
            }

            var from = new Vector2(target.X, target.Y - (cellSize * (target.Position.Row - createdBonusOrigin.Row)));
            var targetPosition = new Vector2(target.X, target.Y);
            var node = viewState.GetPieceNode(target.Position) ?? CreatePieceNode(target, from);
            node.LogicalCell = target.Position;
            node.Position = from;
            node.Tint = target.Tint;
            node.IsVisible = true;
            viewState.AddOrUpdate(node);

            var animation = Anim.Sequence();
            if (initialDelaySeconds > 0f)
            {
                animation.Append(new DelayAnimation(initialDelaySeconds, blocksInput: true));
            }

            animation.Append(Anim.MoveTo(node, targetPosition, 0.75f, blocksInput: true));
            animationPlayer.Play(animation, ChannelConflictPolicy.Replace);
        }
    }
    private static bool IsBonusPiece(RenderPiece piece)
    {
        return piece.Shape == PieceVisualConstants.ShapeDiamond ||
            piece.Shape == PieceVisualConstants.ShapeCircle;
    }
    private static PieceNode CreatePieceNode(RenderPiece piece, Vector2 position)
    {
        return new PieceNode(
            NodeId.New(),
            piece.Position,
            position,
            new Vector2(1f, 1f),
            piece.Rotation,
            opacity: 1f,
            piece.Tint,
            glow: 0f,
            isVisible: true);
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
