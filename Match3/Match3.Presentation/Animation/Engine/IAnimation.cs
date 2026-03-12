namespace Match3.Presentation.Animation.Engine;

public interface IAnimation
{
    bool IsCompleted { get; }

    bool BlocksInput { get; }

    IReadOnlyCollection<AnimationBinding> ActiveBindings { get; }

    void Update(float deltaTime);
}
