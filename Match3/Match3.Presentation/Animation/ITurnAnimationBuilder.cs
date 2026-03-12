using Match3.Presentation.Animation.Engine;

namespace Match3.Presentation.Animation;

public interface ITurnAnimationBuilder
{
    IAnimation Build(TurnAnimationContext context);
}
