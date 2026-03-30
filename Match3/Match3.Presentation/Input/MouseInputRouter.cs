using Match3.Presentation.Runtime;

namespace Match3.Presentation.Input;

public sealed class MouseInputRouter
{
    public bool ShouldHandleBoardSelection(InputState inputState)
    {
        return inputState.IsPrimaryClick;
    }
}
