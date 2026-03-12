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

    public PieceNode? GetPieceNode(GridPosition position)
    {
        return nodeIdsByCell.TryGetValue(position, out var nodeId) &&
            nodesById.TryGetValue(nodeId, out var node)
            ? node
            : null;
    }

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

    public void RemoveNodesExcept(IEnumerable<NodeId> retainedIds)
    {
        ArgumentNullException.ThrowIfNull(retainedIds);

        var retained = retainedIds.ToHashSet();
        foreach (var nodeId in nodesById.Keys.Where(id => !retained.Contains(id)).ToArray())
        {
            RemoveNode(nodeId);
        }
    }

    public void AddOrUpdate(EffectNode node)
    {
        ArgumentNullException.ThrowIfNull(node);
        effectNodesById[node.Id] = node;
    }

    public bool RemoveEffectNode(NodeId id)
    {
        return effectNodesById.Remove(id);
    }

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

    public bool IsCellHidden(GridPosition position)
    {
        return hiddenCellCounts.ContainsKey(position);
    }
}
