using Match3.Platform.Hosting;
using Match3.Presentation.Composition;
using Match3.Presentation.Runtime;
using Match3.Presentation.Screens;

namespace Match3.iOS;

internal static class IosCompositionRoot
{
    public static Match3Game CreateGame()
    {
        var game = new Match3Game();
        var compositionRoot = new GameFlowCompositionRoot();
        var flow = compositionRoot.CreateScreenFlowController();
        game.Services.AddService(typeof(ScreenFlowController), flow);
        game.Services.AddService(typeof(IGameScreenHost), compositionRoot.CreateScreenHost(flow));
        return game;
    }
}
