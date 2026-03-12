using Match3.Presentation.Animation;
using Match3.Presentation.Animation.Engine;
using System.Numerics;

namespace Match3.Tests;

public sealed class Phase16AnimationEngineTests
{
    [Fact]
    public void AnimationPlayer_StartsWithoutActiveAnimations()
    {
        var player = new AnimationPlayer();

        Assert.False(player.HasActiveAnimations);
        Assert.False(player.HasBlockingAnimations);
    }

    [Fact]
    public void SequenceAnimation_RunsAppendedAnimationsInOrder()
    {
        var calls = new List<string>();
        var sequence = new SequenceAnimation()
            .Append(new DelayAnimation(0.1f))
            .Append(new CallbackAnimation(() => calls.Add("A")))
            .Append(new CallbackAnimation(() => calls.Add("B")));

        sequence.Update(0.05f);
        Assert.Empty(calls);

        sequence.Update(0.05f);

        Assert.Equal(["A", "B"], calls);
        Assert.True(sequence.IsCompleted);
    }

    [Fact]
    public void ParallelAnimation_CompletesAfterLongestChild()
    {
        var animation = new ParallelAnimation(
            new DelayAnimation(0.1f),
            new DelayAnimation(0.2f));

        animation.Update(0.1f);

        Assert.False(animation.IsCompleted);

        animation.Update(0.1f);

        Assert.True(animation.IsCompleted);
    }

    [Fact]
    public void DelayAnimation_CompletesOnlyAfterConfiguredDuration()
    {
        var animation = new DelayAnimation(0.2f);

        animation.Update(0.19f);
        Assert.False(animation.IsCompleted);

        animation.Update(0.01f);
        Assert.True(animation.IsCompleted);
    }

    [Fact]
    public void CallbackAnimation_InvokesActionOnlyOnce()
    {
        var calls = 0;
        var animation = new CallbackAnimation(() => calls++);

        animation.Update(0.1f);
        animation.Update(0.1f);

        Assert.Equal(1, calls);
        Assert.True(animation.IsCompleted);
    }

    [Fact]
    public void PropertyTween_InterpolatesFloatValue()
    {
        var target = new object();
        var current = 0f;
        var tween = new PropertyTween<float>(
            target,
            AnimationChannel.Position,
            () => current,
            value => current = value,
            0f,
            10f,
            1f,
            static (from, to, progress) => from + ((to - from) * progress));

        tween.Update(0.5f);

        Assert.Equal(5f, current, 3);
        Assert.False(tween.IsCompleted);

        tween.Update(0.5f);

        Assert.Equal(10f, current, 3);
        Assert.True(tween.IsCompleted);
    }

    [Fact]
    public void AnimationPlayer_RejectsConflictingTweens_OnSameNodeAndChannel()
    {
        var player = new AnimationPlayer();
        var target = new object();
        var firstValue = 0f;
        var secondValue = 0f;
        var first = CreateTween(target, AnimationChannel.Position, () => firstValue, value => firstValue = value, 10f);
        var second = CreateTween(target, AnimationChannel.Position, () => secondValue, value => secondValue = value, 20f);

        var firstHandle = player.Play(first);
        var secondHandle = player.Play(second);

        Assert.True(firstHandle.IsAccepted);
        Assert.False(secondHandle.IsAccepted);
        Assert.Single(player.ActiveAnimations);
    }

    [Fact]
    public void SequenceAnimation_JoinRunsAnimationsInParallelWithinSingleStep()
    {
        var calls = new List<string>();
        var sequence = new SequenceAnimation()
            .Append(new DelayAnimation(0.1f))
            .Join(new DelayAnimation(0.2f))
            .Append(new CallbackAnimation(() => calls.Add("Done")));

        sequence.Update(0.1f);
        Assert.Empty(calls);
        Assert.False(sequence.IsCompleted);

        sequence.Update(0.1f);

        Assert.Equal(["Done"], calls);
        Assert.True(sequence.IsCompleted);
    }

    [Fact]
    public void PieceNode_KeepsStableId_WhenLogicalCellChanges()
    {
        var node = new PieceNode(
            NodeId.New(),
            new Match3.Core.GameCore.ValueObjects.GridPosition(0, 0),
            new System.Numerics.Vector2(10f, 20f),
            new System.Numerics.Vector2(1f, 1f),
            rotation: 0f,
            opacity: 1f,
            tint: Match3.Presentation.Rendering.PieceVisualConstants.TintRed,
            glow: 0f,
            isVisible: true);
        var id = node.Id;

        node.LogicalCell = new Match3.Core.GameCore.ValueObjects.GridPosition(1, 0);

        Assert.Equal(id, node.Id);
    }

    [Fact]
    public void BoardViewState_CanResolvePieceNodeByGridPosition()
    {
        var state = new BoardViewState();
        var node = new PieceNode(
            NodeId.New(),
            new Match3.Core.GameCore.ValueObjects.GridPosition(2, 3),
            new System.Numerics.Vector2(20f, 30f),
            new System.Numerics.Vector2(1f, 1f),
            rotation: 0f,
            opacity: 1f,
            tint: Match3.Presentation.Rendering.PieceVisualConstants.TintBlue,
            glow: 0f,
            isVisible: true);

        state.AddOrUpdate(node);

        var resolved = state.GetPieceNode(node.LogicalCell);

        Assert.Same(node, resolved);
    }

    [Fact]
    public void Anim_MoveTo_ProducesPositionTween()
    {
        var node = CreateNode();
        var tween = Anim.MoveTo(node, new Vector2(30f, 45f), 1f);

        tween.Update(0.5f);

        Assert.Equal(new Vector2(20f, 32.5f), node.Position);
        Assert.False(tween.IsCompleted);
        Assert.Contains(tween.ActiveBindings, binding => ReferenceEquals(binding.Target, node) && binding.Channel == AnimationChannel.Position);
    }

    [Fact]
    public void Anim_ScaleTo_ProducesScaleTween()
    {
        var node = CreateNode();
        var tween = Anim.ScaleTo(node, new Vector2(2f, 3f), 1f);

        tween.Update(1f);

        Assert.Equal(new Vector2(2f, 3f), node.Scale);
        Assert.True(tween.IsCompleted);
    }

    [Fact]
    public void Anim_FadeTo_ProducesOpacityTween()
    {
        var node = CreateNode();
        var tween = Anim.FadeTo(node, 0.25f, 1f);

        tween.Update(1f);

        Assert.Equal(0.25f, node.Opacity, 3);
        Assert.True(tween.IsCompleted);
    }

    [Fact]
    public void Anim_Sequence_ComposesAppendAndJoinCalls()
    {
        var calls = new List<string>();
        var sequence = Anim.Sequence()
            .Append(new DelayAnimation(0.1f))
            .Join(new DelayAnimation(0.2f))
            .Append(new CallbackAnimation(() => calls.Add("Done")));

        sequence.Update(0.2f);

        Assert.Equal(["Done"], calls);
        Assert.True(sequence.IsCompleted);
    }

    [Fact]
    public void Anim_Parallel_ComposesProvidedAnimations()
    {
        var calls = new List<string>();
        var parallel = Anim.Parallel(
            new DelayAnimation(0.1f),
            new CallbackAnimation(() => calls.Add("Now")));

        parallel.Update(0f);
        Assert.Equal(["Now"], calls);
        Assert.False(parallel.IsCompleted);

        parallel.Update(0.1f);
        Assert.True(parallel.IsCompleted);
    }

    [Fact]
    public void TurnAnimationBuilder_BuildsRollbackSequence_ForRejectedSwap()
    {
        var calls = new List<string>();
        var builder = new TurnAnimationBuilder();
        var animation = builder.Build(new TurnAnimationContext
        {
            IsSwapApplied = false,
            QueueVisualEffects = () => calls.Add("visual"),
            QueueSwapAnimation = () => calls.Add("swap"),
            QueueBoardSettleAnimation = () => calls.Add("settle"),
            SwapDurationSeconds = 0.36f
        });

        animation.Update(0f);

        Assert.Equal(["visual", "swap"], calls);
        Assert.False(animation.IsCompleted);

        animation.Update(0.36f);

        Assert.True(animation.IsCompleted);
        Assert.DoesNotContain("settle", calls);
    }

    [Fact]
    public void TurnAnimationBuilder_BuildsSwapThenSettleSequence_ForAppliedSwap()
    {
        var calls = new List<string>();
        var builder = new TurnAnimationBuilder();
        var animation = builder.Build(new TurnAnimationContext
        {
            IsSwapApplied = true,
            QueueVisualEffects = () => calls.Add("visual"),
            QueueSwapAnimation = () => calls.Add("swap"),
            QueueBoardSettleAnimation = () => calls.Add("settle"),
            SwapDurationSeconds = 0.22f,
            SettleDelaySeconds = 0.8f,
            SettleDurationSeconds = 1.15f
        });

        animation.Update(0f);
        Assert.Equal(["visual", "swap"], calls);

        animation.Update(0.21f);
        Assert.DoesNotContain("settle", calls);

        animation.Update(0.01f);
        Assert.Equal(["visual", "swap", "settle"], calls);
        Assert.False(animation.IsCompleted);

        animation.Update(1.95f);
        Assert.True(animation.IsCompleted);
    }

    [Fact]
    public void AnimationPlayer_BlocksInput_WhileBlockingScenarioIsRunning()
    {
        var player = new AnimationPlayer();
        var animation = new TurnAnimationBuilder().Build(new TurnAnimationContext
        {
            IsSwapApplied = false,
            QueueVisualEffects = static () => { },
            QueueSwapAnimation = static () => { },
            QueueBoardSettleAnimation = static () => { },
            SwapDurationSeconds = 0.36f
        });

        player.Play(animation);
        player.Update(0f);

        Assert.True(player.HasBlockingAnimations);

        player.Update(0.36f);

        Assert.False(player.HasBlockingAnimations);
    }

    [Fact]
    public void SwapAnimationScenario_MovesBothPiecesToTargetCells()
    {
        var viewState = new BoardViewState();
        var player = new AnimationPlayer();
        var controller = new GameplayEffectsController();
        var from = new Match3.Core.GameCore.ValueObjects.GridPosition(0, 0);
        var to = new Match3.Core.GameCore.ValueObjects.GridPosition(0, 1);
        var fromNode = new PieceNode(NodeId.New(), from, new Vector2(20f, 20f), new Vector2(1f, 1f), 0f, 1f, Match3.Presentation.Rendering.PieceVisualConstants.TintRed, 0f, true);
        var toNode = new PieceNode(NodeId.New(), to, new Vector2(68f, 20f), new Vector2(1f, 1f), 0f, 1f, Match3.Presentation.Rendering.PieceVisualConstants.TintBlue, 0f, true);
        viewState.AddOrUpdate(fromNode);
        viewState.AddOrUpdate(toNode);

        controller.QueueSwap(viewState, player, new Match3.Core.GameCore.ValueObjects.Move(from, to), new Match3.Presentation.Rendering.BoardTransform(48f, new Vector2(20f, 20f)), rollback: false);

        Assert.Equal(to, fromNode.LogicalCell);
        Assert.Equal(from, toNode.LogicalCell);

        player.Update(0.22f);

        Assert.Equal(new Vector2(68f, 20f), fromNode.Position);
        Assert.Equal(new Vector2(20f, 20f), toNode.Position);
    }

    [Fact]
    public void DestroyerScenario_SpawnsTransientEffectNode_AndAdvancesAlongPath()
    {
        var controller = new GameplayEffectsController();
        var player = new AnimationPlayer();
        var viewState = new BoardViewState();
        var transform = new Match3.Presentation.Rendering.BoardTransform(48f, new Vector2(20f, 20f));
        var origin = new Match3.Core.GameCore.ValueObjects.GridPosition(0, 1);
        var tail = new Match3.Core.GameCore.ValueObjects.GridPosition(0, 3);
        var snapshot = new Match3.Presentation.Rendering.BoardRenderSnapshot(
            [],
            [
                new Match3.Presentation.Rendering.RenderPiece(origin, Match3.Presentation.Rendering.PieceVisualConstants.ShapeSquare, Match3.Presentation.Rendering.PieceVisualConstants.TintRed, 68f, 20f, 32f, 32f),
                new Match3.Presentation.Rendering.RenderPiece(new Match3.Core.GameCore.ValueObjects.GridPosition(0, 2), Match3.Presentation.Rendering.PieceVisualConstants.ShapeSquare, Match3.Presentation.Rendering.PieceVisualConstants.TintBlue, 116f, 20f, 32f, 32f),
                new Match3.Presentation.Rendering.RenderPiece(tail, Match3.Presentation.Rendering.PieceVisualConstants.ShapeSquare, Match3.Presentation.Rendering.PieceVisualConstants.TintGreen, 164f, 20f, 32f, 32f)
            ]);

        controller.QueueDestroyer(viewState, player, origin, [new Match3.Core.GameCore.ValueObjects.GridPosition(0, 0), origin, new Match3.Core.GameCore.ValueObjects.GridPosition(0, 2), tail], transform);

        Assert.Equal(2, viewState.EffectNodes.Count);

        controller.Update(TimeSpan.FromSeconds(0.05f));
        player.Update(0.05f);
        var earlyPieces = controller.BuildPieces(snapshot, null, viewState);
        var earlyDestroyerX = earlyPieces
            .Where(piece => piece.Shape == Match3.Presentation.Rendering.PieceVisualConstants.ShapeDiamond)
            .Max(piece => piece.X);

        AdvanceRuntime(controller, player, 0.55f);
        var latePieces = controller.BuildPieces(snapshot, null, viewState);
        var lateDestroyerX = latePieces
            .Where(piece => piece.Shape == Match3.Presentation.Rendering.PieceVisualConstants.ShapeDiamond)
            .Max(piece => piece.X);
        Assert.True(lateDestroyerX > earlyDestroyerX);

        AdvanceRuntime(controller, player, 0.40f);
        Assert.Empty(viewState.EffectNodes);
        Assert.False(viewState.IsCellHidden(origin));
    }

    [Fact]
    public void ExplosionScenario_HidesAffectedCells_OnlyWhileEffectIsActive()
    {
        var controller = new GameplayEffectsController();
        var player = new AnimationPlayer();
        var viewState = new BoardViewState();
        var transform = new Match3.Presentation.Rendering.BoardTransform(48f, new Vector2(20f, 20f));
        var affected = new Match3.Core.GameCore.ValueObjects.GridPosition(2, 2);
        var snapshot = new Match3.Presentation.Rendering.BoardRenderSnapshot(
            [],
            [
                new Match3.Presentation.Rendering.RenderPiece(new Match3.Core.GameCore.ValueObjects.GridPosition(0, 0), Match3.Presentation.Rendering.PieceVisualConstants.ShapeSquare, Match3.Presentation.Rendering.PieceVisualConstants.TintRed, 20f, 20f, 32f, 32f),
                new Match3.Presentation.Rendering.RenderPiece(affected, Match3.Presentation.Rendering.PieceVisualConstants.ShapeSquare, Match3.Presentation.Rendering.PieceVisualConstants.TintBlue, 116f, 116f, 32f, 32f)
            ]);

        controller.QueueExplosion(viewState, player, [affected], transform);

        Assert.Single(viewState.EffectNodes);

        player.Update(0.05f);
        var activePieces = controller.BuildPieces(snapshot, null, viewState);
        Assert.DoesNotContain(activePieces, piece => piece.Position == affected);

        player.Update(0.45f);
        var restoredPieces = controller.BuildPieces(snapshot, null, viewState);
        Assert.Contains(restoredPieces, piece => piece.Position == affected);
        Assert.Empty(viewState.EffectNodes);
    }
    private static void AdvanceRuntime(GameplayEffectsController controller, AnimationPlayer player, float totalSeconds, float stepSeconds = 0.05f)
    {
        var remaining = totalSeconds;
        while (remaining > 0f)
        {
            var delta = MathF.Min(stepSeconds, remaining);
            controller.Update(TimeSpan.FromSeconds(delta));
            player.Update(delta);
            remaining -= delta;
        }
    }
    private static PropertyTween<float> CreateTween(
        object target,
        AnimationChannel channel,
        Func<float> getter,
        Action<float> setter,
        float to)
    {
        return new PropertyTween<float>(
            target,
            channel,
            getter,
            setter,
            getter(),
            to,
            1f,
            static (from, targetValue, progress) => from + ((targetValue - from) * progress));
    }

    private static PieceNode CreateNode()
    {
        return new PieceNode(
            NodeId.New(),
            new Match3.Core.GameCore.ValueObjects.GridPosition(0, 0),
            new Vector2(10f, 20f),
            new Vector2(1f, 1f),
            rotation: 0f,
            opacity: 1f,
            tint: Match3.Presentation.Rendering.PieceVisualConstants.TintRed,
            glow: 0f,
            isVisible: true);
    }
}








