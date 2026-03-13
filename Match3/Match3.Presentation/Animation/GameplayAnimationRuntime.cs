using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Match3.Core.GameCore.ValueObjects;
using Match3.Presentation.Animation.Engine;
using Match3.Presentation.Rendering;

namespace Match3.Presentation.Animation;

public static class GameplayAnimationRuntime
{
    /// <summary>
    /// Queues the visual effect for a line bonus by launching mirrored destroyer particles from the activation origin.
    /// </summary>
    /// <param name="viewState">Runtime visual state that owns piece and effect nodes.</param>
    /// <param name="animationPlayer">Animation runtime that schedules the generated animations.</param>
    /// <param name="origin">The cell where the destroyer is considered to start.</param>
    /// <param name="path">The full path of cells affected by the destroyer.</param>
    /// <param name="transform">Board projection used to convert grid cells into world coordinates.</param>
    /// <param name="initialDelaySeconds">Optional delay before the effect starts.</param>
    public static void QueueDestroyer(BoardViewState viewState, AnimationPlayer animationPlayer, GridPosition origin, IReadOnlyList<GridPosition> path, BoardTransform transform, float initialDelaySeconds = 0f)
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

        var size = transform.CellSize * GameplayEffectStyle.DestroyerSizeFactor;
        var forwardPath = path.Skip(launchIndex).ToArray();
        var backwardPath = path.Take(launchIndex + 1).Reverse().ToArray();
        var segmentDurationSeconds = path.Count > 1
            ? GameplayEffectTimings.DestroyerTravelSeconds / (path.Count - 1)
            : 0f;
        QueueDestroyerVisual(viewState, animationPlayer, forwardPath, transform, size, initialDelaySeconds, segmentDurationSeconds);
        QueueDestroyerVisual(viewState, animationPlayer, backwardPath, transform, size, initialDelaySeconds, segmentDurationSeconds);
    }

    /// <summary>
    /// Queues a bomb explosion effect that temporarily hides affected cells while the explosion animation plays.
    /// </summary>
    /// <param name="viewState">Runtime visual state that owns piece and effect nodes.</param>
    /// <param name="animationPlayer">Animation runtime that schedules the generated animations.</param>
    /// <param name="area">All cells covered by the explosion.</param>
    /// <param name="transform">Board projection used to place the explosion in world space.</param>
    /// <param name="initialDelaySeconds">Optional delay before the effect starts.</param>
    public static void QueueExplosion(BoardViewState viewState, AnimationPlayer animationPlayer, IReadOnlyList<GridPosition> area, BoardTransform transform, float initialDelaySeconds = 0f)
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
        var maxSize = transform.CellSize * GameplayEffectStyle.BombMaxSizeFactor;
        var initialPosition = new Vector2(center.X - (maxSize / 2f), center.Y - (maxSize / 2f));
        var effectNode = new EffectNode(
            NodeId.New(),
            new GridPosition(-1, -1),
            initialPosition,
            new Vector2(GameplayEffectStyle.BombInitialScale, GameplayEffectStyle.BombInitialScale),
            rotation: 0f,
            opacity: 1f,
            tint: PieceVisualConstants.TintOrange,
            glow: 0f,
            isVisible: true,
            shape: PieceVisualConstants.ShapeCircle,
            width: maxSize,
            height: maxSize,
            layer: GameplayEffectStyle.BombEffectLayer);
        viewState.AddOrUpdate(effectNode);

        var areaCells = area.ToArray();
        var animation = Anim.Sequence()
            .AppendDelayIfNeeded(initialDelaySeconds)
            .AppendDelayIfNeeded(GameplayEffectTimings.BombDetonationDelaySeconds)
            .Append(new CallbackAnimation(() => viewState.HideCells(areaCells)))
            .Append(Anim.Parallel(
                Anim.ScaleTo(effectNode, new Vector2(1f, 1f), GameplayEffectTimings.BombExplosionSeconds),
                Anim.FadeTo(effectNode, 0f, GameplayEffectTimings.BombExplosionSeconds)))
            .Append(new CallbackAnimation(() =>
            {
                viewState.ShowCells(areaCells);
                viewState.RemoveEffectNode(effectNode.Id);
            }));

        animationPlayer.Play(animation, ChannelConflictPolicy.Replace);
    }

    /// <summary>
    /// Queues pop animations for pieces that disappear between two board snapshots, optionally aligned to domain-event timings.
    /// </summary>
    /// <param name="viewState">Runtime visual state that owns piece and effect nodes.</param>
    /// <param name="animationPlayer">Animation runtime that schedules the generated animations.</param>
    /// <param name="beforeSnapshot">Board snapshot before the resolve step.</param>
    /// <param name="afterSnapshot">Board snapshot after the resolve step.</param>
    /// <param name="events">Optional domain events used to delay removals until related visual effects reach a cell.</param>
    /// <param name="initialDelaySeconds">Optional delay before the pop sequence starts.</param>
    public static void QueueMatchPop(
        BoardViewState viewState,
        AnimationPlayer animationPlayer,
        BoardRenderSnapshot beforeSnapshot,
        BoardRenderSnapshot afterSnapshot,
        IReadOnlyList<Match3.Core.GameFlow.Events.IDomainEvent>? events = null,
        float initialDelaySeconds = 0f)
    {
        ArgumentNullException.ThrowIfNull(viewState);
        ArgumentNullException.ThrowIfNull(animationPlayer);
        ArgumentNullException.ThrowIfNull(beforeSnapshot);
        ArgumentNullException.ThrowIfNull(afterSnapshot);

        var survivingCells = afterSnapshot.Pieces
            .Select(piece => piece.Position)
            .ToHashSet();
        var removalDelays = events is null
            ? new Dictionary<GridPosition, float>()
            : GameplayVisualEffectsTimeline.GetRemovalStartDelays(events);
        var consumedPieces = beforeSnapshot.Pieces
            .Where(piece => !survivingCells.Contains(piece.Position))
            .ToArray();

        foreach (var piece in consumedPieces)
        {
            var effectNode = new EffectNode(
                NodeId.New(),
                piece.Position,
                new Vector2(piece.X, piece.Y),
                new Vector2(1f, 1f),
                piece.Rotation,
                opacity: 1f,
                piece.Tint,
                glow: 0f,
                isVisible: true,
                piece.Shape,
                piece.Width,
                piece.Height,
                layer: GameplayEffectStyle.MatchPopEffectLayer);
            viewState.AddOrUpdate(effectNode);

            var scaleAnimation = new PropertyTween<Vector2>(
                effectNode,
                AnimationChannel.Scale,
                () => effectNode.Scale,
                value => effectNode.Scale = value,
                new Vector2(1f, 1f),
                Vector2.Zero,
                GameplayEffectTimings.MatchPopSeconds,
                static (_, _, progress) =>
                {
                    var burst = 1f + (GameplayEffectStyle.MatchPopBurstAmplitude * MathF.Sin(progress * MathF.PI));
                    var shrink = 1f - Easing.SmoothStep(progress);
                    var value = MathF.Max(0f, burst * shrink);
                    return new Vector2(value, value);
                });
            var rotationAnimation = new PropertyTween<float>(
                effectNode,
                AnimationChannel.Rotation,
                () => effectNode.Rotation,
                value => effectNode.Rotation = value,
                piece.Rotation,
                piece.Rotation + GameplayEffectStyle.MatchPopRotationDeltaRadians,
                GameplayEffectTimings.MatchPopSeconds,
                static (from, to, progress) => from + ((to - from) * Easing.SmoothStep(progress)));

            var animation = Anim.Sequence()
                .AppendDelayIfNeeded(initialDelaySeconds + (removalDelays.TryGetValue(piece.Position, out var removalDelay) ? removalDelay : 0f))
                .Append(Anim.Parallel(scaleAnimation, rotationAnimation))
                .Append(new CallbackAnimation(() => viewState.RemoveEffectNode(effectNode.Id)));
            animationPlayer.Play(animation, ChannelConflictPolicy.Replace);
        }
    }

    /// <summary>
    /// Queues the swap or rollback animation for two adjacent piece nodes.
    /// </summary>
    /// <param name="viewState">Runtime visual state used to resolve the nodes involved in the move.</param>
    /// <param name="animationPlayer">Animation runtime that schedules the generated animations.</param>
    /// <param name="move">The logical move being visualized.</param>
    /// <param name="rollback">If <see langword="true" />, animates the swap out and back to its original positions.</param>
    public static void QueueSwap(BoardViewState viewState, AnimationPlayer animationPlayer, Move move, bool rollback)
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
                    Anim.MoveTo(fromNode, toPosition, GameplayEffectTimings.SwapRollbackLegSeconds, blocksInput: true),
                    Anim.MoveTo(toNode, fromPosition, GameplayEffectTimings.SwapRollbackLegSeconds, blocksInput: true)))
                .Append(Anim.Parallel(
                    Anim.MoveTo(fromNode, fromPosition, GameplayEffectTimings.SwapRollbackLegSeconds, blocksInput: true),
                    Anim.MoveTo(toNode, toPosition, GameplayEffectTimings.SwapRollbackLegSeconds, blocksInput: true)));
            animationPlayer.Play(rollbackSequence, ChannelConflictPolicy.Replace);
            return;
        }

        fromNode.LogicalCell = move.To;
        toNode.LogicalCell = move.From;
        animationPlayer.Play(
            Anim.Parallel(
                Anim.MoveTo(fromNode, toPosition, GameplayEffectTimings.SwapAcceptedSeconds, blocksInput: true),
                Anim.MoveTo(toNode, fromPosition, GameplayEffectTimings.SwapAcceptedSeconds, blocksInput: true)),
            ChannelConflictPolicy.Replace);
    }

    /// <summary>
    /// Queues the combined settle phase by first animating gravity and then animating newly spawned pieces.
    /// </summary>
    /// <param name="viewState">Runtime visual state that owns piece nodes.</param>
    /// <param name="animationPlayer">Animation runtime that schedules the generated animations.</param>
    /// <param name="beforeSnapshot">Board snapshot before settle begins.</param>
    /// <param name="afterSnapshot">Board snapshot after settle completes.</param>
    /// <param name="cellSize">Board cell size used to position spawned pieces above the board.</param>
    /// <param name="initialDelaySeconds">Optional delay before settle starts.</param>
    /// <param name="excludedTargets">Cells that should be skipped because another visual path owns them.</param>
    /// <param name="visualState">Optional gameplay visual state used to preserve or suppress selection effects.</param>
    public static void QueueBoardSettle(
        BoardViewState viewState,
        AnimationPlayer animationPlayer,
        BoardRenderSnapshot beforeSnapshot,
        BoardRenderSnapshot afterSnapshot,
        float cellSize,
        float initialDelaySeconds = 0f,
        IReadOnlyList<GridPosition>? excludedTargets = null,
        GameplayVisualState? visualState = null)
    {
        QueueGravity(viewState, animationPlayer, beforeSnapshot, afterSnapshot, initialDelaySeconds, excludedTargets, visualState);
        QueueSpawn(viewState, animationPlayer, beforeSnapshot, afterSnapshot, cellSize, initialDelaySeconds, excludedTargets);
    }

    /// <summary>
    /// Queues falling animations for pieces that survive a resolve step and move to new cells after gravity.
    /// </summary>
    /// <param name="viewState">Runtime visual state that owns piece nodes.</param>
    /// <param name="animationPlayer">Animation runtime that schedules the generated animations.</param>
    /// <param name="beforeSnapshot">Board snapshot before gravity.</param>
    /// <param name="afterSnapshot">Board snapshot after gravity.</param>
    /// <param name="initialDelaySeconds">Optional delay before gravity starts.</param>
    /// <param name="excludedTargets">Cells that should be left untouched because another animation will populate them.</param>
    /// <param name="visualState">Optional gameplay visual state used to suppress stale selection on consumed nodes.</param>
    public static void QueueGravity(
        BoardViewState viewState,
        AnimationPlayer animationPlayer,
        BoardRenderSnapshot beforeSnapshot,
        BoardRenderSnapshot afterSnapshot,
        float initialDelaySeconds = 0f,
        IReadOnlyList<GridPosition>? excludedTargets = null,
        GameplayVisualState? visualState = null)
    {
        ArgumentNullException.ThrowIfNull(viewState);
        ArgumentNullException.ThrowIfNull(animationPlayer);

        var selectedNodeIdBeforeSettle = visualState?.GetSelectedNodeId(viewState);
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

                if (isSurvivor)
                {
                    var node = viewState.GetPieceNode(source!.Position) ?? CreatePieceNode(source, new Vector2(source.X, source.Y));
                    var from = node.Position;
                    var durationSeconds = GameplayEffectTimings.GravitySeconds;
                    var delaySeconds = initialDelaySeconds;

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
                    continue;
                }

                if (excludedTargets?.Contains(target.Position) == true &&
                    viewState.GetPieceNode(target.Position) is { } existingExcludedNode)
                {
                    existingExcludedNode.Position = targetPosition;
                    existingExcludedNode.Tint = target.Tint;
                    retainedNodeIds.Add(existingExcludedNode.Id);
                }
            }
        }

        visualState?.SuppressSelectionIfNeeded(selectedNodeIdBeforeSettle, retainedNodeIds);
        viewState.RemoveNodesExcept(retainedNodeIds);
    }

    /// <summary>
    /// Queues spawn animations for pieces that appear after refill and have no matching survivor in the previous snapshot.
    /// </summary>
    /// <param name="viewState">Runtime visual state that owns piece nodes.</param>
    /// <param name="animationPlayer">Animation runtime that schedules the generated animations.</param>
    /// <param name="beforeSnapshot">Board snapshot before refill.</param>
    /// <param name="afterSnapshot">Board snapshot after refill.</param>
    /// <param name="cellSize">Board cell size used to place new pieces above the board.</param>
    /// <param name="initialDelaySeconds">Optional delay before spawn begins.</param>
    /// <param name="excludedTargets">Cells that should be skipped because another animation will create them.</param>
    public static void QueueSpawn(
        BoardViewState viewState,
        AnimationPlayer animationPlayer,
        BoardRenderSnapshot beforeSnapshot,
        BoardRenderSnapshot afterSnapshot,
        float cellSize,
        float initialDelaySeconds = 0f,
        IReadOnlyList<GridPosition>? excludedTargets = null)
    {
        ArgumentNullException.ThrowIfNull(viewState);
        ArgumentNullException.ThrowIfNull(animationPlayer);

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
                if (survivorMap.ContainsKey(target.Position))
                {
                    continue;
                }

                if (excludedTargets?.Contains(target.Position) == true)
                {
                    continue;
                }

                spawnCount++;
                var from = new Vector2(target.X, target.Y - (cellSize * (spawnCount + 1)));
                var targetPosition = new Vector2(target.X, target.Y);
                var node = CreatePieceNode(target, from);
                node.LogicalCell = target.Position;
                node.Tint = target.Tint;
                node.Position = from;
                node.IsVisible = true;
                viewState.AddOrUpdate(node);

                var animation = Anim.Sequence();
                if (initialDelaySeconds > 0f)
                {
                    animation.Append(new DelayAnimation(initialDelaySeconds, blocksInput: true));
                }

                animation.Append(Anim.MoveTo(node, targetPosition, GameplayEffectTimings.SpawnSeconds, blocksInput: true));
                animationPlayer.Play(animation, ChannelConflictPolicy.Replace);
            }
        }
    }

    /// <summary>
    /// Queues the visual introduction of bonuses that are created inside the board rather than spawned from the refill lane.
    /// </summary>
    /// <param name="viewState">Runtime visual state that owns piece nodes.</param>
    /// <param name="animationPlayer">Animation runtime that schedules the generated animations.</param>
    /// <param name="afterSnapshot">Board snapshot containing the created bonus pieces.</param>
    /// <param name="cellSize">Board cell size used to derive the travel distance from the creation origin.</param>
    /// <param name="initialDelaySeconds">Optional delay before the created-bonus animation begins.</param>
    /// <param name="createdBonusOrigins">Cells where bonus creation started during resolve.</param>
    public static void QueueCreatedBonuses(BoardViewState viewState, AnimationPlayer animationPlayer, BoardRenderSnapshot afterSnapshot, float cellSize, float initialDelaySeconds = 0f, IReadOnlyList<GridPosition>? createdBonusOrigins = null)
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

            animation.Append(Anim.MoveTo(node, targetPosition, GameplayEffectTimings.SpawnSeconds, blocksInput: true));
            animationPlayer.Play(animation, ChannelConflictPolicy.Replace);
        }
    }

    private static void QueueDestroyerVisual(BoardViewState viewState, AnimationPlayer animationPlayer, IReadOnlyList<GridPosition> path, BoardTransform transform, float size, float initialDelaySeconds, float segmentDurationSeconds)
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
            layer: GameplayEffectStyle.DestroyerEffectLayer);
        viewState.AddOrUpdate(effectNode);

        var pathCells = path.ToArray();
        var durationSeconds = segmentDurationSeconds * (pathCells.Length - 1);
        var animation = Anim.Sequence()
            .AppendDelayIfNeeded(initialDelaySeconds)
            .Append(new DestroyerFlightAnimation(
                viewState,
                effectNode,
                centers,
                pathCells,
                size,
                durationSeconds));

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

    private static SequenceAnimation AppendDelayIfNeeded(this SequenceAnimation animation, float delaySeconds)
    {
        if (delaySeconds > 0f)
        {
            animation.Append(new DelayAnimation(delaySeconds, blocksInput: true));
        }

        return animation;
    }
}
