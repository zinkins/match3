using System;
using Match3.Core.GameCore.Board;
using Match3.Core.GameFlow.Sessions;
using Match3.Core.GameFlow.StateMachine;
using Match3.Core.GameFlow.Pipeline;
using Match3.Presentation.Animation;
using Match3.Presentation.Animation.Engine;
using Match3.Presentation.Input;
using Match3.Presentation.Rendering;
using Match3.Presentation.UI;

namespace Match3.Presentation.Screens;

public sealed class GameplayScreenFactory
{
    private readonly BoardGenerator boardGenerator;
    private readonly Func<ITurnAnimationBuilder> turnAnimationBuilderFactory;

    public GameplayScreenFactory(BoardGenerator? boardGenerator = null, Func<ITurnAnimationBuilder>? turnAnimationBuilderFactory = null)
    {
        this.boardGenerator = boardGenerator ?? new BoardGenerator();
        this.turnAnimationBuilderFactory = turnAnimationBuilderFactory ?? (() => new TurnAnimationBuilder());
    }

    public GameplayScreen Create(GameSession session, Action onOk)
    {
        ArgumentNullException.ThrowIfNull(session);
        ArgumentNullException.ThrowIfNull(onOk);

        var board = boardGenerator.Generate();
        var boardTransform = new BoardTransform(
            LayoutMetrics.InitialBoardCellSize,
            new System.Numerics.Vector2(LayoutMetrics.InitialBoardOriginX, LayoutMetrics.InitialBoardOriginY),
            board.Height,
            board.Width);
        var presenter = new GameplayPresenter(new TurnProcessor(), new GameplayStateMachine(), session);

        return new GameplayScreen(
            presenter,
            board,
            new BoardInputHandler(boardTransform, new SelectionController()),
            new AnimationPlayer(),
            turnAnimationBuilderFactory(),
            new BoardRenderer(),
            new HudRenderer(),
            boardTransform,
            onOk);
    }
}
