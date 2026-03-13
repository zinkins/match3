using System.Numerics;
using Match3.Core.GameCore.ValueObjects;
using Match3.Presentation.Rendering;

namespace Match3.Presentation.Animation.Engine;

public sealed class DestroyerFlightAnimation : ITimedAnimation
{
    private readonly AnimationBinding[] bindings;
    private readonly BoardViewState viewState;
    private readonly EffectNode effectNode;
    private readonly DestroyerAnimation movement;
    private readonly GridPosition[] pathCells;
    private readonly Vector2 halfSize;
    private readonly float durationSeconds;
    private float elapsedSeconds;
    private int hiddenCellIndex = -1;
    private bool started;
    private bool cleanedUp;

    public DestroyerFlightAnimation(
        BoardViewState viewState,
        EffectNode effectNode,
        IReadOnlyList<Vector2> pathCenters,
        IReadOnlyList<GridPosition> pathCells,
        float size,
        float durationSeconds)
    {
        ArgumentNullException.ThrowIfNull(viewState);
        ArgumentNullException.ThrowIfNull(effectNode);
        ArgumentNullException.ThrowIfNull(pathCenters);
        ArgumentNullException.ThrowIfNull(pathCells);

        this.viewState = viewState;
        this.effectNode = effectNode;
        movement = new DestroyerAnimation(pathCenters);
        this.pathCells = pathCells.ToArray();
        halfSize = new Vector2(size / 2f, size / 2f);
        this.durationSeconds = durationSeconds;
        bindings = [new AnimationBinding(effectNode, AnimationChannel.Position)];
    }

    public bool IsCompleted => elapsedSeconds >= durationSeconds;

    public bool BlocksInput => false;

    public IReadOnlyCollection<AnimationBinding> ActiveBindings => bindings;

    public void Update(float deltaTime)
    {
        _ = Advance(deltaTime);
    }

    public float Advance(float deltaTime)
    {
        if (deltaTime < 0f || cleanedUp)
        {
            return 0f;
        }

        if (!started)
        {
            started = true;
            HideReachedCells(0f);
        }

        var remaining = MathF.Max(0f, durationSeconds - elapsedSeconds);
        var consumed = durationSeconds <= 0f
            ? 0f
            : MathF.Min(deltaTime, remaining);
        elapsedSeconds += durationSeconds <= 0f ? 0f : consumed;
        var progress = durationSeconds <= 0f ? 1f : MathF.Min(1f, elapsedSeconds / durationSeconds);
        effectNode.Position = movement.Evaluate(progress) - halfSize;
        HideReachedCells(progress);

        if (IsCompleted)
        {
            Cleanup();
        }

        return durationSeconds <= 0f ? deltaTime : deltaTime - consumed;
    }

    private void HideReachedCells(float progress)
    {
        if (pathCells.Length == 0)
        {
            return;
        }

        var reachedIndex = pathCells.Length == 1
            ? 0
            : (int)MathF.Floor(progress * (pathCells.Length - 1));
        while (hiddenCellIndex < reachedIndex)
        {
            hiddenCellIndex++;
            viewState.HideCells([pathCells[hiddenCellIndex]]);
        }
    }

    private void Cleanup()
    {
        if (cleanedUp)
        {
            return;
        }

        cleanedUp = true;
        viewState.ShowCells(pathCells);
        viewState.RemoveEffectNode(effectNode.Id);
    }
}
