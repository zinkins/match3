using System;
using Match3.Presentation.Animation;
using Match3.Presentation.Input;
using Match3.Presentation.Rendering;
using Match3.Presentation.UI;
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
        BoardTransform boardTransform,
        Action onOk)
    {
        Presenter = presenter;
        Board = board;
        BoardInputHandler = boardInputHandler;
        EffectsController = effectsController;
        BoardRenderer = boardRenderer;
        HudRenderer = hudRenderer;
        BoardTransform = boardTransform;
        OkButton = new UiButton("Ok", onOk ?? throw new ArgumentNullException(nameof(onOk)));
    }

    public string Name => "Gameplay";

    public string GameOverMessage => "Game Over";

    public GameplayPresenter Presenter { get; }

    public BoardState Board { get; }

    public BoardInputHandler BoardInputHandler { get; }

    public GameplayEffectsController EffectsController { get; }

    public BoardRenderer BoardRenderer { get; }

    public HudRenderer HudRenderer { get; }

    public BoardTransform BoardTransform { get; }

    public UiButton OkButton { get; }

    public bool ShouldShowGameOverOverlay => Presenter.IsGameOver && !EffectsController.HasActiveBlockingEffects;

    public AnimationQueue AnimationQueue => Presenter.AnimationQueue;

    public GridPosition? SelectedCell => BoardInputHandler.SelectedCell;
}