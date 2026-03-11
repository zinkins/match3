using Match3.Core;
using Match3.Presentation.Composition;
using Match3.Presentation.Screens;

namespace Match3.DesktopGL;

internal static class DesktopCompositionRoot
{
    public static Match3Game CreateGame()
    {
        var game = new Match3Game();
        var flow = new GameFlowCompositionRoot().CreateScreenFlowController();
        game.Services.AddService(typeof(ScreenFlowController), flow);
        return game;
    }
}
