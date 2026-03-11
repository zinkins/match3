using Match3.Core.Runtime;

namespace Match3.Presentation.Input;

public sealed class TouchInputRouter
{
    public bool ShouldHandleBoardSelection(InputState inputState)
    {
        return inputState.IsPrimaryClick;
    }
}
