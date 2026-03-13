using Match3.Core.GameCore.ValueObjects;
using Match3.Core.GameFlow.Events;
using Match3.Presentation.Animation.Engine;
using Match3.Presentation.Rendering;

namespace Match3.Presentation.Animation;

public static class GameplayVisualEffectsTimeline
{
    public static float GetTotalDuration(IReadOnlyList<IDomainEvent> events)
    {
        ArgumentNullException.ThrowIfNull(events);

        var schedules = BuildSchedules(events);
        return schedules.Count == 0
            ? 0f
            : schedules.Max(schedule => schedule.StartDelaySeconds + schedule.DurationSeconds);
    }

    public static void QueueEvents(BoardViewState viewState, AnimationPlayer animationPlayer, IReadOnlyList<IDomainEvent> events, BoardTransform transform)
    {
        ArgumentNullException.ThrowIfNull(viewState);
        ArgumentNullException.ThrowIfNull(animationPlayer);
        ArgumentNullException.ThrowIfNull(events);
        ArgumentNullException.ThrowIfNull(transform);

        foreach (var schedule in BuildSchedules(events))
        {
            switch (schedule.Event)
            {
                case DestroyerSpawned destroyer:
                    GameplayAnimationRuntime.QueueDestroyer(viewState, animationPlayer, destroyer.Position, destroyer.Path, transform, schedule.StartDelaySeconds);
                    break;
                case BombExploded explosion:
                    GameplayAnimationRuntime.QueueExplosion(viewState, animationPlayer, explosion.Area, transform, schedule.StartDelaySeconds);
                    break;
            }
        }
    }

    public static IReadOnlyDictionary<GridPosition, float> GetRemovalStartDelays(IReadOnlyList<IDomainEvent> events)
    {
        ArgumentNullException.ThrowIfNull(events);

        var delays = new Dictionary<GridPosition, float>();
        foreach (var schedule in BuildSchedules(events))
        {
            switch (schedule.Event)
            {
                case DestroyerSpawned destroyer:
                    foreach (var cellDelay in BuildDestroyerRemovalDelays(destroyer, schedule.StartDelaySeconds))
                    {
                        delays[cellDelay.Key] = delays.TryGetValue(cellDelay.Key, out var existing)
                            ? MathF.Max(existing, cellDelay.Value)
                            : cellDelay.Value;
                    }

                    break;
                case BombExploded explosion:
                    foreach (var cell in explosion.Area)
                    {
                        delays[cell] = delays.TryGetValue(cell, out var existing)
                            ? MathF.Max(existing, schedule.StartDelaySeconds)
                            : schedule.StartDelaySeconds;
                    }

                    break;
            }
        }

        return delays;
    }

    private static IReadOnlyList<VisualEffectSchedule> BuildSchedules(IReadOnlyList<IDomainEvent> events)
    {
        var schedules = new List<VisualEffectSchedule>();
        foreach (var domainEvent in events)
        {
            var startDelaySeconds = domainEvent switch
            {
                BombExploded explosion => ResolveDestroyerReachDelay(schedules, explosion.Position),
                _ => 0f
            };

            var durationSeconds = domainEvent switch
            {
                DestroyerSpawned => 0.8f,
                BombExploded => 0.45f,
                _ => 0f
            };

            if (durationSeconds <= 0f)
            {
                continue;
            }

            schedules.Add(new VisualEffectSchedule(domainEvent, startDelaySeconds, durationSeconds));
        }

        return schedules;
    }

    private static float ResolveDestroyerReachDelay(IEnumerable<VisualEffectSchedule> schedules, GridPosition target)
    {
        return schedules
            .Select(schedule => schedule.Event switch
            {
                DestroyerSpawned destroyer => TryGetDestroyerReachTime(schedule.StartDelaySeconds, destroyer, target),
                _ => null
            })
            .Where(value => value.HasValue)
            .Select(value => value!.Value)
            .DefaultIfEmpty(0f)
            .Max();
    }

    private static float? TryGetDestroyerReachTime(float initialDelaySeconds, DestroyerSpawned destroyer, GridPosition target)
    {
        var path = destroyer.Path;
        var originIndex = FindIndex(path, destroyer.Position);
        var targetIndex = FindIndex(path, target);
        if (originIndex < 0 || targetIndex < 0 || path.Count <= 1)
        {
            return null;
        }

        var segmentDuration = 0.8f / (path.Count - 1);
        return initialDelaySeconds + (Math.Abs(targetIndex - originIndex) * segmentDuration);
    }

    private static IReadOnlyDictionary<GridPosition, float> BuildDestroyerRemovalDelays(DestroyerSpawned destroyer, float initialDelaySeconds)
    {
        var delays = new Dictionary<GridPosition, float>();
        var path = destroyer.Path;
        var originIndex = FindIndex(path, destroyer.Position);
        if (originIndex < 0 || path.Count == 0)
        {
            return delays;
        }

        var segmentDuration = path.Count <= 1
            ? 0f
            : 0.8f / (path.Count - 1);
        for (var i = 0; i < path.Count; i++)
        {
            delays[path[i]] = initialDelaySeconds + (Math.Abs(i - originIndex) * segmentDuration);
        }

        return delays;
    }

    private static int FindIndex(IReadOnlyList<GridPosition> positions, GridPosition target)
    {
        for (var i = 0; i < positions.Count; i++)
        {
            if (positions[i] == target)
            {
                return i;
            }
        }

        return -1;
    }

    private sealed record VisualEffectSchedule(IDomainEvent Event, float StartDelaySeconds, float DurationSeconds);
}
