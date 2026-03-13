namespace Match3.Presentation.Animation.Engine;

public interface ITimedAnimation : IAnimation
{
    float Advance(float deltaTime);
}
