using Match3.Presentation.Runtime;
using Match3.Presentation.Rendering;
using Match3.Presentation.Screens;

namespace Match3.Presentation.Composition;

public sealed class GameFlowCompositionRoot
{
    public ScreenFlowController CreateScreenFlowController()
    {
        return new ScreenFlowController();
    }

    public IGameScreenHost CreateScreenHost(ScreenFlowController flowController)
    {
        return new PresentationScreenHost(flowController, new SpriteBatchRenderer());
    }
}
