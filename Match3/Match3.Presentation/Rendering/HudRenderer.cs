using System;

namespace Match3.Presentation.Rendering;

public sealed class HudRenderer
{
    public string FormatScore(int score)
    {
        return $"Score: {score}";
    }

    public string FormatRemainingTime(TimeSpan remainingTime)
    {
        return $"Time: {Math.Max(0, (int)Math.Ceiling(remainingTime.TotalSeconds))}";
    }
}
