using Match3.Core.GameCore.Board;
using Match3.Core.GameCore.Pieces;
using Match3.Core.GameCore.ValueObjects;
using Match3.Core.GameFlow.Pipeline;
using Match3.Core.GameFlow.Sessions;
using Match3.Core.GameFlow.StateMachine;

namespace Match3.Tests;

public class Phase9StateMachineAndPipelineTests
{
    [Fact]
    public void GameplayStateMachine_StartsInIdle()
    {
        var machine = new GameplayStateMachine();
        Assert.Equal(GameplayState.Idle, machine.State);
    }

    [Fact]
    public void GameplayStateMachine_TransitionsIdleToSelecting()
    {
        var machine = new GameplayStateMachine();
        machine.TransitionToSelecting();
        Assert.Equal(GameplayState.Selecting, machine.State);
    }

    [Fact]
    public void GameplayStateMachine_TransitionsSelectingToSwapping()
    {
        var machine = new GameplayStateMachine();
        machine.TransitionToSelecting();
        machine.TransitionToSwapping();
        Assert.Equal(GameplayState.Swapping, machine.State);
    }

    [Fact]
    public void GameplayStateMachine_TransitionsSwappingToResolving()
    {
        var machine = new GameplayStateMachine();
        machine.TransitionToSwapping();
        machine.TransitionToResolving();
        Assert.Equal(GameplayState.Resolving, machine.State);
    }

    [Fact]
    public void GameplayStateMachine_TransitionsResolvingToApplyingGravity()
    {
        var machine = new GameplayStateMachine();
        machine.TransitionToResolving();
        machine.TransitionToApplyingGravity();
        Assert.Equal(GameplayState.ApplyingGravity, machine.State);
    }

    [Fact]
    public void GameplayStateMachine_TransitionsApplyingGravityToRefilling()
    {
        var machine = new GameplayStateMachine();
        machine.TransitionToApplyingGravity();
        machine.TransitionToRefilling();
        Assert.Equal(GameplayState.Refilling, machine.State);
    }

    [Fact]
    public void GameplayStateMachine_TransitionsRefillingToCheckingEndGame()
    {
        var machine = new GameplayStateMachine();
        machine.TransitionToRefilling();
        machine.TransitionToCheckingEndGame();
        Assert.Equal(GameplayState.CheckingEndGame, machine.State);
    }

    [Fact]
    public void GameplayStateMachine_TransitionsCheckingEndGameToGameOver()
    {
        var machine = new GameplayStateMachine();
        machine.TransitionToCheckingEndGame();
        machine.TransitionToGameOver();
        Assert.Equal(GameplayState.GameOver, machine.State);
    }

    [Fact]
    public void GameplayStateMachine_TransitionsToCheckingEndGame_WhenTimerExpiresDuringSwapping()
    {
        var machine = new GameplayStateMachine();
        var session = new GameSession();
        machine.TransitionToSwapping();
        session.UpdateTimer(TimeSpan.FromSeconds(60));

        machine.AdvanceAfterPhase(session);

        Assert.Equal(GameplayState.CheckingEndGame, machine.State);
    }

    [Fact]
    public void GameplayStateMachine_TransitionsToCheckingEndGame_WhenTimerExpiresDuringResolving()
    {
        var machine = new GameplayStateMachine();
        var session = new GameSession();
        machine.TransitionToResolving();
        session.UpdateTimer(TimeSpan.FromSeconds(60));

        machine.AdvanceAfterPhase(session);

        Assert.Equal(GameplayState.CheckingEndGame, machine.State);
    }

    [Fact]
    public void GameplayStateMachine_TransitionsToCheckingEndGame_WhenTimerExpiresDuringApplyingGravity()
    {
        var machine = new GameplayStateMachine();
        var session = new GameSession();
        machine.TransitionToApplyingGravity();
        session.UpdateTimer(TimeSpan.FromSeconds(60));

        machine.AdvanceAfterPhase(session);

        Assert.Equal(GameplayState.CheckingEndGame, machine.State);
    }

    [Fact]
    public void GameplayStateMachine_TransitionsToCheckingEndGame_WhenTimerExpiresDuringRefilling()
    {
        var machine = new GameplayStateMachine();
        var session = new GameSession();
        machine.TransitionToRefilling();
        session.UpdateTimer(TimeSpan.FromSeconds(60));

        machine.AdvanceAfterPhase(session);

        Assert.Equal(GameplayState.CheckingEndGame, machine.State);
    }

    [Fact]
    public void TurnProcessor_FinishesCurrentAtomicResolution_BeforeGameOver()
    {
        var machine = new GameplayStateMachine();
        var session = new GameSession();
        var processor = new TurnProcessor();
        var resolved = false;

        processor.ExecuteAtomicResolvingStep(session, machine, () =>
        {
            resolved = true;
            session.UpdateTimer(TimeSpan.FromSeconds(60));
        });

        Assert.True(resolved);
        Assert.Equal(GameplayState.CheckingEndGame, machine.State);
    }

    [Fact]
    public void TurnProcessor_ProcessesSingleTurnPipeline()
    {
        var board = CreateBoardForSwapWithMatch();
        var move = new Move(new GridPosition(0, 2), new GridPosition(1, 2));
        var session = new GameSession();
        var machine = new GameplayStateMachine();
        var processor = new TurnProcessor(
            matchFinder: new MatchFinder(),
            gravityResolver: new GravityResolver(),
            refillResolver: new RefillResolver(new SequenceRandomSource(0, 1, 2, 3, 4)));

        var applied = processor.ProcessTurnPipeline(board, move, session, machine);

        Assert.True(applied);
        Assert.Equal(GameplayState.Idle, machine.State);
    }

    [Fact]
    public void TurnProcessor_ClearsMatchedPieces_BeforeGravityAndRefill()
    {
        var board = CreateBoardForSwapWithMatch();
        var move = new Move(new GridPosition(0, 2), new GridPosition(1, 2));
        var processor = new TurnProcessor(
            matchFinder: new MatchFinder(),
            gravityResolver: new GravityResolver(),
            refillResolver: new RefillResolver(new SequenceRandomSource(1)));
        var session = new GameSession();
        var machine = new GameplayStateMachine();

        processor.ProcessTurnPipeline(board, move, session, machine);

        Assert.Equal(PieceType.Green, board.GetCell(new GridPosition(0, 0)));
        Assert.Equal(PieceType.Green, board.GetCell(new GridPosition(0, 1)));
        Assert.Equal(PieceType.Green, board.GetCell(new GridPosition(0, 2)));
    }

    private static BoardState CreateBoardForSwapWithMatch()
    {
        var board = new BoardState();
        var types = PieceCatalog.All;
        for (var row = 0; row < board.Height; row++)
        {
            for (var column = 0; column < board.Width; column++)
            {
                board.SetCell(new GridPosition(row, column), types[(row + column) % types.Count]);
            }
        }

        board.SetCell(new GridPosition(0, 0), PieceType.Red);
        board.SetCell(new GridPosition(0, 1), PieceType.Red);
        board.SetCell(new GridPosition(0, 2), PieceType.Blue);
        board.SetCell(new GridPosition(1, 2), PieceType.Red);
        return board;
    }

    private sealed class SequenceRandomSource(params int[] values) : IRandomSource
    {
        private readonly int[] values = values.Length == 0 ? [0] : values;
        private int index;

        public int Next(int minInclusive, int maxExclusive)
        {
            var value = values[index % values.Length];
            index++;
            var range = maxExclusive - minInclusive;
            return minInclusive + (value % range);
        }
    }
}
