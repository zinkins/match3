using Match3.Presentation.Animation.Engine;

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
}
