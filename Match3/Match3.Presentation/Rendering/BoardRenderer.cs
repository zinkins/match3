using Match3.Core.GameCore.Board;

namespace Match3.Presentation.Rendering;

public sealed class BoardRenderer
{
    public BoardState? LastRenderedBoard { get; private set; }

    public void Render(BoardState board)
    {
        LastRenderedBoard = board;
    }
}
