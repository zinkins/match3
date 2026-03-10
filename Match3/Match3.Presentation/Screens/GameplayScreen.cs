using Match3.Presentation.Animation;
using Match3.Presentation.Rendering;

namespace Match3.Presentation.Screens;

public sealed class GameplayScreen : IScreen
{
    public GameplayScreen(GameplayPresenter presenter, BoardRenderer boardRenderer, HudRenderer hudRenderer)
    {
        Presenter = presenter;
        BoardRenderer = boardRenderer;
        HudRenderer = hudRenderer;
    }

    public string Name => "Gameplay";

    public GameplayPresenter Presenter { get; }

    public BoardRenderer BoardRenderer { get; }

    public HudRenderer HudRenderer { get; }

    public bool ShouldShowGameOverOverlay => Presenter.ShouldShowGameOverOverlay;

    public AnimationQueue AnimationQueue => Presenter.AnimationQueue;
}
