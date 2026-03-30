namespace Match3.Presentation.Rendering;

public static class BoardSnapshotAnalysis
{
    public static IReadOnlyList<int> GetTrackedColumns(params BoardRenderSnapshot[] snapshots)
    {
        return snapshots
            .Where(snapshot => snapshot is not null)
            .SelectMany(snapshot => snapshot.Pieces.Select(piece => piece.Position.Column))
            .Distinct()
            .OrderBy(column => column)
            .ToArray();
    }
}
