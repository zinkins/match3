using System.Collections.Generic;
using Match3.Core.GameFlow.Events;

namespace Match3.Presentation.Animation;

public sealed class AnimationQueue
{
    private readonly Queue<AnimationStep> pending = new();
    private AnimationStep? currentStep;
    private float currentElapsedSeconds;

    public bool HasRunningAnimations => currentStep is not null || pending.Count > 0;

    public AnimationStep? CurrentStep => currentStep;

    public void Enqueue(IReadOnlyList<IDomainEvent> events)
    {
        foreach (var domainEvent in events)
        {
            pending.Enqueue(CreateStep(domainEvent));
        }

        TryStartNextStep();
    }

    public void Update(float elapsedSeconds)
    {
        if (elapsedSeconds < 0f)
        {
            return;
        }

        TryStartNextStep();
        while (currentStep is not null)
        {
            currentElapsedSeconds += elapsedSeconds;
            if (currentElapsedSeconds < currentStep.DurationSeconds)
            {
                return;
            }

            elapsedSeconds = currentElapsedSeconds - currentStep.DurationSeconds;
            currentStep = null;
            currentElapsedSeconds = 0f;
            TryStartNextStep();
        }
    }

    private void TryStartNextStep()
    {
        if (currentStep is null && pending.Count > 0)
        {
            currentStep = pending.Dequeue();
            currentElapsedSeconds = 0f;
        }
    }

    private static AnimationStep CreateStep(IDomainEvent domainEvent)
    {
        return domainEvent switch
        {
            PiecesSwapped => new AnimationStep(nameof(PiecesSwapped), 0.22f),
            SwapReverted => new AnimationStep(nameof(SwapReverted), 0.36f),
            MatchResolved => new AnimationStep(nameof(MatchResolved), 0.2f),
            LineBonusCreated => new AnimationStep(nameof(LineBonusCreated), 0.15f),
            BombBonusCreated => new AnimationStep(nameof(BombBonusCreated), 0.15f),
            DestroyerSpawned => new AnimationStep(nameof(DestroyerSpawned), 0.8f),
            BombExploded => new AnimationStep(nameof(BombExploded), 0.45f),
            PiecesFell => new AnimationStep(nameof(PiecesFell), 0.65f),
            PiecesSpawned => new AnimationStep(nameof(PiecesSpawned), 1.15f),
            GameEnded => new AnimationStep(nameof(GameEnded), 0.01f),
            _ => new AnimationStep(domainEvent.GetType().Name, 0.01f)
        };
    }
}
