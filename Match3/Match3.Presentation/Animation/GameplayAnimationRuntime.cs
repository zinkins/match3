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

        var size = transform.CellSize * 0.42f;
        var forwardPath = path.Skip(launchIndex).ToArray();
        var backwardPath = path.Take(launchIndex + 1).Reverse().ToArray();
        var segmentDurationSeconds = path.Count > 1
            ? 0.8f / (path.Count - 1)
            : 0f;
        QueueDestroyerVisual(viewState, animationPlayer, forwardPath, transform, size, initialDelaySeconds, segmentDurationSeconds);
        QueueDestroyerVisual(viewState, animationPlayer, backwardPath, transform, size, initialDelaySeconds, segmentDurationSeconds);
    }

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
            .AppendDelayIfNeeded(initialDelaySeconds)
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
                    var durationSeconds = 0.65f;
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

                animation.Append(Anim.MoveTo(node, targetPosition, 0.75f, blocksInput: true));
                animationPlayer.Play(animation, ChannelConflictPolicy.Replace);
            }
        }
    }

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

            animation.Append(Anim.MoveTo(node, targetPosition, 0.75f, blocksInput: true));
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
            layer: 30f);
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
