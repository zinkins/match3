namespace Match3.Presentation.Animation;

public sealed class AnimationStep(string name, float durationSeconds)
{
    public string Name { get; } = name;

    public float DurationSeconds { get; } = durationSeconds;
}
