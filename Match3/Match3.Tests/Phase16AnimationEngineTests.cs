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
            QueueCreatedBonusAnimation = () => calls.Add("bonus"),
            QueueBoardSettleAnimation = () => calls.Add("settle"),
            SwapDurationSeconds = 0.36f
        });

        animation.Update(0f);

        Assert.Equal(["visual", "swap"], calls);
        Assert.False(animation.IsCompleted);

        animation.Update(0.36f);

        Assert.True(animation.IsCompleted);
        Assert.DoesNotContain("settle", calls);
        Assert.DoesNotContain("bonus", calls);
    }

    [Fact]
    public void TurnAnimationBuilder_BuildsSwapThenCreatedBonusThenSettleSequence_ForAppliedSwap()
    {
        var calls = new List<string>();
        var builder = new TurnAnimationBuilder();
        var animation = builder.Build(new TurnAnimationContext
        {
            IsSwapApplied = true,
            QueueVisualEffects = () => calls.Add("visual"),
            QueueSwapAnimation = () => calls.Add("swap"),
            QueueCreatedBonusAnimation = () => calls.Add("bonus"),
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
        Assert.Equal(["visual", "swap", "bonus", "settle"], calls);
        Assert.False(animation.IsCompleted);

        animation.Update(1.95f);
        Assert.True(animation.IsCompleted);
    }

    [Fact]
    public void CreatedBonusScenario_StartsFromCreationCell_InsteadOfSpawnLane()
    {
        var controller = new GameplayEffectsController();
        var player = new AnimationPlayer();
        var viewState = new BoardViewState();
        var renderer = new Match3.Presentation.Rendering.PieceNodeRenderer();
        var afterSnapshot = new Match3.Presentation.Rendering.BoardRenderSnapshot(
            [],
            [
                new Match3.Presentation.Rendering.RenderPiece(new Match3.Core.GameCore.ValueObjects.GridPosition(3, 2), Match3.Presentation.Rendering.PieceVisualConstants.ShapeDiamond, Match3.Presentation.Rendering.PieceVisualConstants.TintRed, 116f, 164f, 32f, 32f)
            ]);

        controller.QueueCreatedBonuses(viewState, player, afterSnapshot, 48f, createdBonusOrigins: [new Match3.Core.GameCore.ValueObjects.GridPosition(1, 2)]);
        player.Update(0.05f);

        var nodeSnapshot = renderer.BuildSnapshot(afterSnapshot, viewState);
        var pieces = controller.BuildPieces(nodeSnapshot, null, viewState, player);
        var bonus = Assert.Single(pieces, piece => piece.Shape == Match3.Presentation.Rendering.PieceVisualConstants.ShapeDiamond);

        Assert.True(bonus.Y >= 68f);
        Assert.True(bonus.Y < 164f);
    }
    [Fact]
    public void TurnAnimationBuilder_DoesNotScheduleSpawnForCreatedBonusCell()
    {
        var builder = new TurnAnimationBuilder();
        var controller = new GameplayEffectsController();
        var player = new AnimationPlayer();
        var viewState = new BoardViewState();
        var afterSnapshot = new Match3.Presentation.Rendering.BoardRenderSnapshot(
            [],
            [
                new Match3.Presentation.Rendering.RenderPiece(new Match3.Core.GameCore.ValueObjects.GridPosition(2, 0), Match3.Presentation.Rendering.PieceVisualConstants.ShapeDiamond, Match3.Presentation.Rendering.PieceVisualConstants.TintRed, 20f, 116f, 32f, 32f)
            ]);
        var createdBonusOrigins = new[] { new Match3.Core.GameCore.ValueObjects.GridPosition(1, 0) };
        var createdBonusTargets = new[] { new Match3.Core.GameCore.ValueObjects.GridPosition(2, 0) };
        var animation = builder.Build(new TurnAnimationContext
        {
            IsSwapApplied = true,
            QueueVisualEffects = static () => { },
            QueueSwapAnimation = static () => { },
            QueueCreatedBonusAnimation = () => controller.QueueCreatedBonuses(viewState, player, afterSnapshot, 48f, createdBonusOrigins: createdBonusOrigins),
            QueueBoardSettleAnimation = () => controller.QueueBoardSettle(viewState, player, new Match3.Presentation.Rendering.BoardRenderSnapshot([], []), afterSnapshot, 48f, excludedTargets: createdBonusTargets),
            SwapDurationSeconds = 0.22f,
            SettleDurationSeconds = 1.15f
        });

        animation.Update(0f);
        animation.Update(0.22f);

        Assert.Single(viewState.PieceNodes);
        Assert.NotNull(viewState.GetPieceNode(new Match3.Core.GameCore.ValueObjects.GridPosition(2, 0)));
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
            QueueCreatedBonusAnimation = static () => { },
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
    public void DestroyerScenario_SpawnsTransientEffectNode_AndClearsPathOverTime()
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
        Assert.DoesNotContain(earlyPieces, piece => piece.Position == origin && piece.Shape == Match3.Presentation.Rendering.PieceVisualConstants.ShapeSquare);
        Assert.Contains(earlyPieces, piece => piece.Position == new Match3.Core.GameCore.ValueObjects.GridPosition(0, 2));

        AdvanceRuntime(controller, player, 0.35f);
        var midPieces = controller.BuildPieces(snapshot, null, viewState);
        Assert.DoesNotContain(midPieces, piece => piece.Position == new Match3.Core.GameCore.ValueObjects.GridPosition(0, 2) && piece.Shape == Match3.Presentation.Rendering.PieceVisualConstants.ShapeSquare);
        Assert.Contains(midPieces, piece => piece.Position == tail);

        AdvanceRuntime(controller, player, 0.40f);
        Assert.Empty(viewState.EffectNodes);
        Assert.False(viewState.IsCellHidden(origin));
        Assert.False(viewState.IsCellHidden(new Match3.Core.GameCore.ValueObjects.GridPosition(0, 2)));
    }

    [Fact]
    public void GameplayScreen_ShowsGameOverOnlyAfterBlockingScenarioCompletes()
    {
        var flow = new Match3.Presentation.Screens.ScreenFlowController(CreateExpiredSession);
        flow.MainMenu.PlayButton.Click();
        flow.Tick();
        var gameplay = flow.Gameplay;

        gameplay.AnimationPlayer.Play(new DelayAnimation(0.5f, blocksInput: true));

        Assert.False(gameplay.ShouldShowGameOverOverlay);

        gameplay.AnimationPlayer.Update(0.5f);

        Assert.True(gameplay.ShouldShowGameOverOverlay);
    }
    [Fact]
    public void SelectionEffect_CanStackWithMovementWithoutOverwritingPositionChannel()
    {
        var controller = new GameplayEffectsController();
        var player = new AnimationPlayer();
        var viewState = new BoardViewState();
        var cell = new Match3.Core.GameCore.ValueObjects.GridPosition(0, 0);
        var snapshot = new Match3.Presentation.Rendering.BoardRenderSnapshot(
            [],
            [
                new Match3.Presentation.Rendering.RenderPiece(cell, Match3.Presentation.Rendering.PieceVisualConstants.ShapeSquare, Match3.Presentation.Rendering.PieceVisualConstants.TintRed, 20f, 20f, 32f, 32f)
            ]);
        var node = new PieceNode(NodeId.New(), cell, new Vector2(20f, 20f), new Vector2(1f, 1f), 0f, 1f, Match3.Presentation.Rendering.PieceVisualConstants.TintRed, 0f, true);
        viewState.AddOrUpdate(node);

        controller.BuildPieces(snapshot, cell, viewState, player);
        player.Update(0.12f);

        Assert.True(node.Scale.X > 1f);
        Assert.True(node.Rotation > 0f);

        player.Play(Anim.MoveTo(node, new Vector2(68f, 20f), 0.22f), ChannelConflictPolicy.Replace);
        player.Update(0.22f);

        Assert.Equal(new Vector2(68f, 20f), node.Position);
        Assert.True(node.Scale.X > 1f);
        Assert.True(node.Rotation > 0f);
    }
    [Fact]
    public void GravityScenario_ReusesExistingPieceNodes_ForFallingPieces()
    {
        var controller = new GameplayEffectsController();
        var player = new AnimationPlayer();
        var viewState = new BoardViewState();
        var beforeSnapshot = new Match3.Presentation.Rendering.BoardRenderSnapshot(
            [],
            [
                new Match3.Presentation.Rendering.RenderPiece(new Match3.Core.GameCore.ValueObjects.GridPosition(0, 0), Match3.Presentation.Rendering.PieceVisualConstants.ShapeSquare, Match3.Presentation.Rendering.PieceVisualConstants.TintRed, 20f, 20f, 32f, 32f),
                new Match3.Presentation.Rendering.RenderPiece(new Match3.Core.GameCore.ValueObjects.GridPosition(1, 0), Match3.Presentation.Rendering.PieceVisualConstants.ShapeSquare, Match3.Presentation.Rendering.PieceVisualConstants.TintBlue, 20f, 68f, 32f, 32f),
                new Match3.Presentation.Rendering.RenderPiece(new Match3.Core.GameCore.ValueObjects.GridPosition(2, 0), Match3.Presentation.Rendering.PieceVisualConstants.ShapeSquare, Match3.Presentation.Rendering.PieceVisualConstants.TintRed, 20f, 116f, 32f, 32f)
            ]);
        var afterSnapshot = new Match3.Presentation.Rendering.BoardRenderSnapshot(
            [],
            [
                new Match3.Presentation.Rendering.RenderPiece(new Match3.Core.GameCore.ValueObjects.GridPosition(1, 0), Match3.Presentation.Rendering.PieceVisualConstants.ShapeSquare, Match3.Presentation.Rendering.PieceVisualConstants.TintRed, 20f, 68f, 32f, 32f),
                new Match3.Presentation.Rendering.RenderPiece(new Match3.Core.GameCore.ValueObjects.GridPosition(2, 0), Match3.Presentation.Rendering.PieceVisualConstants.ShapeSquare, Match3.Presentation.Rendering.PieceVisualConstants.TintBlue, 20f, 116f, 32f, 32f)
            ]);
        var movingNode = new PieceNode(
            NodeId.New(),
            new Match3.Core.GameCore.ValueObjects.GridPosition(1, 0),
            new Vector2(20f, 68f),
            new Vector2(1f, 1f),
            0f,
            1f,
            Match3.Presentation.Rendering.PieceVisualConstants.TintBlue,
            0f,
            true);
        viewState.AddOrUpdate(movingNode);

        controller.QueueBoardSettle(viewState, player, beforeSnapshot, afterSnapshot, 48f);

        var stationaryNode = viewState.GetPieceNode(new Match3.Core.GameCore.ValueObjects.GridPosition(1, 0));
        Assert.Same(movingNode, viewState.GetPieceNode(new Match3.Core.GameCore.ValueObjects.GridPosition(2, 0)));
        Assert.NotNull(stationaryNode);
        Assert.NotSame(movingNode, stationaryNode);
        Assert.Equal(2, viewState.PieceNodes.Count);

        player.Update(0.65f);

        Assert.Equal(new Vector2(20f, 116f), movingNode.Position);
    }

    [Fact]
    public void SpawnScenario_CreatesNewPieceNodes_AboveBoard_AndMovesThemDown()
    {
        var controller = new GameplayEffectsController();
        var player = new AnimationPlayer();
        var viewState = new BoardViewState();
        var beforeSnapshot = new Match3.Presentation.Rendering.BoardRenderSnapshot([], []);
        var afterSnapshot = new Match3.Presentation.Rendering.BoardRenderSnapshot(
            [],
            [
                new Match3.Presentation.Rendering.RenderPiece(new Match3.Core.GameCore.ValueObjects.GridPosition(0, 0), Match3.Presentation.Rendering.PieceVisualConstants.ShapeDiamond, Match3.Presentation.Rendering.PieceVisualConstants.TintRed, 20f, 20f, 32f, 32f)
            ]);

        controller.QueueBoardSettle(viewState, player, beforeSnapshot, afterSnapshot, 48f);

        var spawnedNode = viewState.GetPieceNode(new Match3.Core.GameCore.ValueObjects.GridPosition(0, 0));
        Assert.NotNull(spawnedNode);
        Assert.True(spawnedNode!.Position.Y < 20f);

        player.Update(0.4f);
        Assert.True(spawnedNode.Position.Y < 20f);

        player.Update(0.35f);
        Assert.Equal(new Vector2(20f, 20f), spawnedNode.Position);
    }
    [Fact]
    public void BoardViewState_RemovesConsumedPieceNodes_AfterResolvePhase()
    {
        var controller = new GameplayEffectsController();
        var player = new AnimationPlayer();
        var viewState = new BoardViewState();
        var consumedNode = new PieceNode(NodeId.New(), new Match3.Core.GameCore.ValueObjects.GridPosition(0, 0), new Vector2(20f, 20f), new Vector2(1f, 1f), 0f, 1f, Match3.Presentation.Rendering.PieceVisualConstants.TintRed, 0f, true);
        var fallingNode = new PieceNode(NodeId.New(), new Match3.Core.GameCore.ValueObjects.GridPosition(1, 0), new Vector2(20f, 68f), new Vector2(1f, 1f), 0f, 1f, Match3.Presentation.Rendering.PieceVisualConstants.TintBlue, 0f, true);
        var bottomNode = new PieceNode(NodeId.New(), new Match3.Core.GameCore.ValueObjects.GridPosition(2, 0), new Vector2(20f, 116f), new Vector2(1f, 1f), 0f, 1f, Match3.Presentation.Rendering.PieceVisualConstants.TintGreen, 0f, true);
        viewState.AddOrUpdate(consumedNode);
        viewState.AddOrUpdate(fallingNode);
        viewState.AddOrUpdate(bottomNode);

        var beforeSnapshot = new Match3.Presentation.Rendering.BoardRenderSnapshot(
            [],
            [
                new Match3.Presentation.Rendering.RenderPiece(new Match3.Core.GameCore.ValueObjects.GridPosition(0, 0), Match3.Presentation.Rendering.PieceVisualConstants.ShapeSquare, Match3.Presentation.Rendering.PieceVisualConstants.TintRed, 20f, 20f, 32f, 32f),
                new Match3.Presentation.Rendering.RenderPiece(new Match3.Core.GameCore.ValueObjects.GridPosition(1, 0), Match3.Presentation.Rendering.PieceVisualConstants.ShapeSquare, Match3.Presentation.Rendering.PieceVisualConstants.TintBlue, 20f, 68f, 32f, 32f),
                new Match3.Presentation.Rendering.RenderPiece(new Match3.Core.GameCore.ValueObjects.GridPosition(2, 0), Match3.Presentation.Rendering.PieceVisualConstants.ShapeSquare, Match3.Presentation.Rendering.PieceVisualConstants.TintGreen, 20f, 116f, 32f, 32f)
            ]);
        var afterSnapshot = new Match3.Presentation.Rendering.BoardRenderSnapshot(
            [],
            [
                new Match3.Presentation.Rendering.RenderPiece(new Match3.Core.GameCore.ValueObjects.GridPosition(1, 0), Match3.Presentation.Rendering.PieceVisualConstants.ShapeSquare, Match3.Presentation.Rendering.PieceVisualConstants.TintBlue, 20f, 68f, 32f, 32f),
                new Match3.Presentation.Rendering.RenderPiece(new Match3.Core.GameCore.ValueObjects.GridPosition(2, 0), Match3.Presentation.Rendering.PieceVisualConstants.ShapeSquare, Match3.Presentation.Rendering.PieceVisualConstants.TintGreen, 20f, 116f, 32f, 32f)
            ]);

        controller.QueueBoardSettle(viewState, player, beforeSnapshot, afterSnapshot, 48f);

        Assert.Equal(2, viewState.PieceNodes.Count);
        Assert.Null(viewState.GetPieceNode(new Match3.Core.GameCore.ValueObjects.GridPosition(0, 0)));
        Assert.DoesNotContain(viewState.PieceNodes, node => node.Id == consumedNode.Id);
        Assert.Contains(viewState.PieceNodes, node => node.Id == fallingNode.Id);
        Assert.Contains(viewState.PieceNodes, node => node.Id == bottomNode.Id);
    }

    [Fact]
    public void BoardViewState_CreatesNodesForPostCascadeBoardState_WithoutFullReset()
    {
        var controller = new GameplayEffectsController();
        var player = new AnimationPlayer();
        var viewState = new BoardViewState();
        var stableNode = new PieceNode(NodeId.New(), new Match3.Core.GameCore.ValueObjects.GridPosition(2, 0), new Vector2(20f, 116f), new Vector2(1f, 1f), 0f, 1f, Match3.Presentation.Rendering.PieceVisualConstants.TintGreen, 0f, true);
        viewState.AddOrUpdate(stableNode);

        var firstBefore = new Match3.Presentation.Rendering.BoardRenderSnapshot(
            [],
            [
                new Match3.Presentation.Rendering.RenderPiece(new Match3.Core.GameCore.ValueObjects.GridPosition(2, 0), Match3.Presentation.Rendering.PieceVisualConstants.ShapeSquare, Match3.Presentation.Rendering.PieceVisualConstants.TintGreen, 20f, 116f, 32f, 32f)
            ]);
        var firstAfter = new Match3.Presentation.Rendering.BoardRenderSnapshot(
            [],
            [
                new Match3.Presentation.Rendering.RenderPiece(new Match3.Core.GameCore.ValueObjects.GridPosition(1, 0), Match3.Presentation.Rendering.PieceVisualConstants.ShapeSquare, Match3.Presentation.Rendering.PieceVisualConstants.TintRed, 20f, 68f, 32f, 32f),
                new Match3.Presentation.Rendering.RenderPiece(new Match3.Core.GameCore.ValueObjects.GridPosition(2, 0), Match3.Presentation.Rendering.PieceVisualConstants.ShapeSquare, Match3.Presentation.Rendering.PieceVisualConstants.TintGreen, 20f, 116f, 32f, 32f)
            ]);

        controller.QueueBoardSettle(viewState, player, firstBefore, firstAfter, 48f);
        var preservedNode = viewState.GetPieceNode(new Match3.Core.GameCore.ValueObjects.GridPosition(2, 0));
        Assert.Same(stableNode, preservedNode);

        var secondAfter = new Match3.Presentation.Rendering.BoardRenderSnapshot(
            [],
            [
                new Match3.Presentation.Rendering.RenderPiece(new Match3.Core.GameCore.ValueObjects.GridPosition(0, 0), Match3.Presentation.Rendering.PieceVisualConstants.ShapeSquare, Match3.Presentation.Rendering.PieceVisualConstants.TintBlue, 20f, 20f, 32f, 32f),
                new Match3.Presentation.Rendering.RenderPiece(new Match3.Core.GameCore.ValueObjects.GridPosition(1, 0), Match3.Presentation.Rendering.PieceVisualConstants.ShapeSquare, Match3.Presentation.Rendering.PieceVisualConstants.TintRed, 20f, 68f, 32f, 32f),
                new Match3.Presentation.Rendering.RenderPiece(new Match3.Core.GameCore.ValueObjects.GridPosition(2, 0), Match3.Presentation.Rendering.PieceVisualConstants.ShapeSquare, Match3.Presentation.Rendering.PieceVisualConstants.TintGreen, 20f, 116f, 32f, 32f)
            ]);

        controller.QueueBoardSettle(viewState, player, firstAfter, secondAfter, 48f);

        Assert.Equal(3, viewState.PieceNodes.Count);
        Assert.Same(stableNode, viewState.GetPieceNode(new Match3.Core.GameCore.ValueObjects.GridPosition(2, 0)));
        Assert.NotNull(viewState.GetPieceNode(new Match3.Core.GameCore.ValueObjects.GridPosition(1, 0)));
        Assert.NotNull(viewState.GetPieceNode(new Match3.Core.GameCore.ValueObjects.GridPosition(0, 0)));
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
    [Fact]
    public void SelectionEffect_IsSuppressed_WhenPieceNodeIsConsumedByResolvePhase()
    {
        var controller = new GameplayEffectsController();
        var player = new AnimationPlayer();
        var viewState = new BoardViewState();
        var selectedCell = new Match3.Core.GameCore.ValueObjects.GridPosition(0, 0);
        var survivorCell = new Match3.Core.GameCore.ValueObjects.GridPosition(1, 0);
        var consumedNode = new PieceNode(NodeId.New(), selectedCell, new Vector2(20f, 20f), new Vector2(1f, 1f), 0f, 1f, Match3.Presentation.Rendering.PieceVisualConstants.TintRed, 0f, true);
        var survivorNode = new PieceNode(NodeId.New(), survivorCell, new Vector2(20f, 68f), new Vector2(1f, 1f), 0f, 1f, Match3.Presentation.Rendering.PieceVisualConstants.TintBlue, 0f, true);
        viewState.AddOrUpdate(consumedNode);
        viewState.AddOrUpdate(survivorNode);

        var beforeSnapshot = new Match3.Presentation.Rendering.BoardRenderSnapshot(
            [],
            [
                new Match3.Presentation.Rendering.RenderPiece(selectedCell, Match3.Presentation.Rendering.PieceVisualConstants.ShapeSquare, Match3.Presentation.Rendering.PieceVisualConstants.TintRed, 20f, 20f, 32f, 32f),
                new Match3.Presentation.Rendering.RenderPiece(survivorCell, Match3.Presentation.Rendering.PieceVisualConstants.ShapeSquare, Match3.Presentation.Rendering.PieceVisualConstants.TintBlue, 20f, 68f, 32f, 32f)
            ]);
        controller.BuildPieces(beforeSnapshot, selectedCell, viewState, player);
        player.Update(0.12f);
        Assert.True(consumedNode.Scale.X > 1f);

        var afterSnapshot = new Match3.Presentation.Rendering.BoardRenderSnapshot(
            [],
            [
                new Match3.Presentation.Rendering.RenderPiece(selectedCell, Match3.Presentation.Rendering.PieceVisualConstants.ShapeSquare, Match3.Presentation.Rendering.PieceVisualConstants.TintGreen, 20f, 20f, 32f, 32f),
                new Match3.Presentation.Rendering.RenderPiece(survivorCell, Match3.Presentation.Rendering.PieceVisualConstants.ShapeSquare, Match3.Presentation.Rendering.PieceVisualConstants.TintBlue, 20f, 68f, 32f, 32f)
            ]);

        controller.QueueBoardSettle(viewState, player, beforeSnapshot, afterSnapshot, 48f);
        var spawnedNode = viewState.GetPieceNode(selectedCell);
        Assert.NotNull(spawnedNode);
        Assert.NotEqual(consumedNode.Id, spawnedNode!.Id);
        var pieces = controller.BuildPieces(afterSnapshot, selectedCell, viewState, player);
        player.Update(0.12f);

        Assert.Equal(1f, spawnedNode!.Scale.X);
        Assert.Equal(0f, spawnedNode.Rotation);
        Assert.DoesNotContain(pieces, piece => piece.Position == selectedCell && piece.Layer == 10f);
    }

    [Fact]
    public void AnimationPlayer_DoesNotLeakCompletedTransientNodes()
    {
        var controller = new GameplayEffectsController();
        var player = new AnimationPlayer();
        var viewState = new BoardViewState();
        var transform = new Match3.Presentation.Rendering.BoardTransform(48f, new Vector2(20f, 20f));

        controller.QueueExplosion(viewState, player, [new Match3.Core.GameCore.ValueObjects.GridPosition(1, 1)], transform);

        Assert.Single(viewState.EffectNodes);

        player.Update(0.45f);

        Assert.Empty(viewState.EffectNodes);
        Assert.False(player.HasActiveAnimations);
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
    private static Match3.Core.GameFlow.Sessions.GameSession CreateExpiredSession()
    {
        var session = new Match3.Core.GameFlow.Sessions.GameSession();
        session.UpdateTimer(TimeSpan.FromSeconds(60));
        return session;
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
