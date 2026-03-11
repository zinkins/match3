using System.Numerics;
using Match3.Core.GameCore.ValueObjects;
using Match3.Core.GameFlow.Sessions;
using Match3.Presentation.Rendering;

namespace Match3.Presentation.Input;

public sealed class BoardInputHandler
{
    private readonly BoardTransform boardTransform;
    private readonly SelectionController selectionController;

    public BoardInputHandler(BoardTransform boardTransform, SelectionController selectionController)
    {
        this.boardTransform = boardTransform;
        this.selectionController = selectionController;
    }

    public Move? HandleClick(Vector2 worldPosition)
    {
        if (!boardTransform.TryWorldToGrid(worldPosition, out var selectedCell))
        {
            return null;
        }

        return selectionController.Select(selectedCell);
    }

    public GridPosition? SelectedCell => selectionController.SelectedCell;
}
