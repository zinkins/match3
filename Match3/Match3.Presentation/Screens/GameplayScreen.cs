using Match3.Presentation.Animation;
using Match3.Presentation.Input;
using Match3.Presentation.Rendering;
using Match3.Core.GameCore.Board;
using Match3.Core.GameCore.ValueObjects;

namespace Match3.Presentation.Screens;

public sealed class GameplayScreen : IScreen
{
    public GameplayScreen(
        GameplayPresenter presenter,
        BoardState board,
        BoardInputHandler boardInputHandler,
        GameplayEffectsController effectsController,
        BoardRenderer boardRenderer,
        HudRenderer hudRenderer,
        BoardTransform boardTransform)
    {
        Presenter = presenter;
        Board = board;
        BoardInputHandler = boardInputHandler;
        EffectsController = effectsController;
        BoardRenderer = boardRenderer;
        HudRenderer = hudRenderer;
        BoardTransform = boardTransform;
    }

    public string Name => "Gameplay";

    public GameplayPresenter Presenter { get; }

    public BoardState Board { get; }

    public BoardInputHandler BoardInputHandler { get; }

    public GameplayEffectsController EffectsController { get; }

    public BoardRenderer BoardRenderer { get; }

    public HudRenderer HudRenderer { get; }

    public BoardTransform BoardTransform { get; }

    public bool ShouldShowGameOverOverlay => Presenter.ShouldShowGameOverOverlay;

    public AnimationQueue AnimationQueue => Presenter.AnimationQueue;

    public GridPosition? SelectedCell => BoardInputHandler.SelectedCell;
}
