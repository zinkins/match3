using System;
using Match3.Presentation.Animation;
using Match3.Presentation.Animation.Engine;
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
        AnimationPlayer animationPlayer,
        ITurnAnimationBuilder turnAnimationBuilder,
        BoardRenderer boardRenderer,
        HudRenderer hudRenderer,
        BoardTransform boardTransform,
        Action onOk)
    {
        Presenter = presenter;
        Board = board;
        VisualBoard = board.Clone();
        BoardInputHandler = boardInputHandler;
        AnimationPlayer = animationPlayer ?? throw new ArgumentNullException(nameof(animationPlayer));
        TurnAnimationBuilder = turnAnimationBuilder ?? throw new ArgumentNullException(nameof(turnAnimationBuilder));
        BoardRenderer = boardRenderer;
        HudRenderer = hudRenderer;
        BoardTransform = boardTransform;
        BoardViewState = new BoardViewState();
        PieceNodeRenderer = new PieceNodeRenderer();
        VisualState = new GameplayVisualState();
        OkButton = new UiButton("Ok", onOk ?? throw new ArgumentNullException(nameof(onOk)));
    }

    public string Name => "Gameplay";

    public string GameOverMessage => "Game Over";

    public GameplayPresenter Presenter { get; }

    public BoardState Board { get; }

    public BoardState VisualBoard { get; private set; }

    public BoardInputHandler BoardInputHandler { get; }

    public AnimationPlayer AnimationPlayer { get; }

    public ITurnAnimationBuilder TurnAnimationBuilder { get; }

    public BoardRenderer BoardRenderer { get; }

    public HudRenderer HudRenderer { get; }

    public BoardTransform BoardTransform { get; }

    public BoardViewState BoardViewState { get; }

    public PieceNodeRenderer PieceNodeRenderer { get; }

    public GameplayVisualState VisualState { get; }

    public UiButton OkButton { get; }

    public bool ShouldShowGameOverOverlay => Presenter.IsGameOver && !AnimationPlayer.HasBlockingAnimations;

    public GridPosition? SelectedCell => BoardInputHandler.SelectedCell;

    public void SetVisualBoard(BoardState board)
    {
        VisualBoard = board ?? throw new ArgumentNullException(nameof(board));
    }
}
