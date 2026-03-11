using System.Numerics;

namespace Match3.Presentation.Animation;

public sealed class SwapAnimation
{
    public SwapAnimation(Vector2 from, Vector2 to)
    {
        From = from;
        To = to;
    }

    public Vector2 From { get; }

    public Vector2 To { get; }

    public Vector2 Evaluate(float progress)
    {
        var eased = Easing.SmoothStep(progress);
        return Vector2.Lerp(From, To, eased);
    }
}
