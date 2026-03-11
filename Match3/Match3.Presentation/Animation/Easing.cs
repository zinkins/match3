using System;

namespace Match3.Presentation.Animation;

public static class Easing
{
    public static float SmoothStep(float t)
    {
        var x = Math.Clamp(t, 0f, 1f);
        return x * x * (3f - (2f * x));
    }
}
