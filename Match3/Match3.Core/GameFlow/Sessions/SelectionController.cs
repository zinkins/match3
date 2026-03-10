using Match3.Core.GameCore.ValueObjects;

namespace Match3.Core.GameFlow.Sessions;

public sealed class SelectionController
{
    public GridPosition? SelectedCell { get; private set; }

    public Move? Select(GridPosition position)
    {
        if (SelectedCell is null)
        {
            SelectedCell = position;
            return null;
        }

        var first = SelectedCell.Value;
        SelectedCell = null;

        if (!first.IsAdjacentTo(position))
        {
            return null;
        }

        return new Move(first, position);
    }
}
