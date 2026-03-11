using Match3.Presentation.Screens;

namespace Match3.Presentation.Composition;

public sealed class GameFlowCompositionRoot
{
    public ScreenFlowController CreateScreenFlowController()
    {
        return new ScreenFlowController();
    }
}
