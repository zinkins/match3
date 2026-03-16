using Match3.Core.GameCore.ValueObjects;

namespace Match3.Presentation.Animation.Engine;

public sealed class BoardViewState
{
    private readonly Dictionary<NodeId, PieceNode> nodesById = [];
    private readonly Dictionary<GridPosition, NodeId> nodeIdsByCell = [];
    private readonly Dictionary<NodeId, EffectNode> effectNodesById = [];
    private readonly Dictionary<GridPosition, int> hiddenCellCounts = [];

    public IReadOnlyCollection<PieceNode> PieceNodes => nodesById.Values;

    public IReadOnlyCollection<EffectNode> EffectNodes => effectNodesById.Values;

    /// <summary>
    /// Inserts or updates a piece node and refreshes the cell-to-node mapping for its current logical cell.
    /// </summary>
    /// <param name="node">Piece node to store.</param>
    public void AddOrUpdate(PieceNode node)
    {
        ArgumentNullException.ThrowIfNull(node);

        if (nodesById.TryGetValue(node.Id, out var existing))
        {
            foreach (var staleCell in nodeIdsByCell
                .Where(pair => pair.Value == node.Id)
                .Select(pair => pair.Key)
                .ToArray())
            {
                nodeIdsByCell.Remove(staleCell);
            }
        }

        nodesById[node.Id] = node;
        nodeIdsByCell[node.LogicalCell] = node.Id;
    }

    /// <summary>
    /// Returns the piece node currently mapped to the specified grid cell, if any.
    /// </summary>
    /// <param name="position">Logical grid cell to query.</param>
    /// <returns>The mapped piece node, or <see langword="null" /> if the cell has no node.</returns>
    public PieceNode? GetPieceNode(GridPosition position)
    {
        return nodeIdsByCell.TryGetValue(position, out var nodeId) &&
            nodesById.TryGetValue(nodeId, out var node)
            ? node
            : null;
    }

    /// <summary>
    /// Removes a piece node by id and clears its logical-cell mapping when present.
    /// </summary>
    /// <param name="id">Identifier of the node to remove.</param>
    /// <returns><see langword="true" /> if a node was removed; otherwise <see langword="false" />.</returns>
    public bool RemoveNode(NodeId id)
    {
        if (!nodesById.TryGetValue(id, out var node))
        {
            return false;
        }

        _ = nodesById.Remove(id);
        if (nodeIdsByCell.TryGetValue(node.LogicalCell, out var mappedId) && mappedId == id)
        {
            _ = nodeIdsByCell.Remove(node.LogicalCell);
        }

        return true;
    }

    /// <summary>
    /// Removes all piece nodes except the supplied retained set.
    /// </summary>
    /// <param name="retainedIds">Node ids that should remain in the view state.</param>
    public void RemoveNodesExcept(IEnumerable<NodeId> retainedIds)
    {
        ArgumentNullException.ThrowIfNull(retainedIds);

        var retained = retainedIds.ToHashSet();
        foreach (var nodeId in nodesById.Keys.Where(id => !retained.Contains(id)).ToArray())
        {
            RemoveNode(nodeId);
        }
    }

    /// <summary>
    /// Inserts or updates an effect node.
    /// </summary>
    /// <param name="node">Effect node to store.</param>
    public void AddOrUpdate(EffectNode node)
    {
        ArgumentNullException.ThrowIfNull(node);
        effectNodesById[node.Id] = node;
    }

    /// <summary>
    /// Removes an effect node by id.
    /// </summary>
    /// <param name="id">Identifier of the effect node to remove.</param>
    /// <returns><see langword="true" /> if an effect node was removed; otherwise <see langword="false" />.</returns>
    public bool RemoveEffectNode(NodeId id)
    {
        return effectNodesById.Remove(id);
    }

    /// <summary>
    /// Marks the supplied cells as temporarily hidden, supporting nested hide/show lifetimes.
    /// </summary>
    /// <param name="positions">Cells to hide.</param>
    public void HideCells(IEnumerable<GridPosition> positions)
    {
        ArgumentNullException.ThrowIfNull(positions);

        foreach (var position in positions)
        {
            hiddenCellCounts[position] = hiddenCellCounts.TryGetValue(position, out var count)
                ? count + 1
                : 1;
        }
    }

    /// <summary>
    /// Decrements the hide count for the supplied cells and reveals them when the count reaches zero.
    /// </summary>
    /// <param name="positions">Cells to reveal.</param>
    public void ShowCells(IEnumerable<GridPosition> positions)
    {
        ArgumentNullException.ThrowIfNull(positions);

        foreach (var position in positions)
        {
            if (!hiddenCellCounts.TryGetValue(position, out var count))
            {
                continue;
            }

            if (count <= 1)
            {
                hiddenCellCounts.Remove(position);
                continue;
            }

            hiddenCellCounts[position] = count - 1;
        }
    }

    /// <summary>
    /// Determines whether a cell is currently hidden by one or more active effects.
    /// </summary>
    /// <param name="position">Cell to inspect.</param>
    /// <returns><see langword="true" /> when the cell is hidden; otherwise <see langword="false" />.</returns>
    public bool IsCellHidden(GridPosition position)
    {
        return hiddenCellCounts.ContainsKey(position);
    }

    public void ClearHiddenCells()
    {
        hiddenCellCounts.Clear();
    }
}
