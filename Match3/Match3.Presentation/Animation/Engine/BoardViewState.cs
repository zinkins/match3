using Match3.Core.GameCore.ValueObjects;

namespace Match3.Presentation.Animation.Engine;

public sealed class BoardViewState
{
    private readonly Dictionary<NodeId, PieceNode> nodesById = [];
    private readonly Dictionary<GridPosition, NodeId> nodeIdsByCell = [];

    public IReadOnlyCollection<PieceNode> PieceNodes => nodesById.Values;

    public void AddOrUpdate(PieceNode node)
    {
        ArgumentNullException.ThrowIfNull(node);

        if (nodesById.TryGetValue(node.Id, out var existing))
        {
            nodeIdsByCell.Remove(existing.LogicalCell);
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
}
