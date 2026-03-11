using System;
using System.Collections.Generic;
using System.Text;

namespace Match3.Presentation.Rendering;

public sealed class HudRenderer
{
    private readonly StringBuilder builder;

    public HudRenderer()
    {
        builder = new StringBuilder(16);
    }

    public string FormatScore(int score)
    {
        builder.Clear();
        builder.Append("Score: ");
        builder.Append(score);
        return builder.ToString();
    }

    public string FormatRemainingTime(TimeSpan remainingTime)
    {
        var seconds = Math.Max(0, (int)Math.Ceiling(remainingTime.TotalSeconds));
        builder.Clear();
        builder.Append("Time: ");
        builder.Append(seconds);
        return builder.ToString();
    }

    public HudRenderSnapshot BuildSnapshot(int score, TimeSpan remainingTime, float viewportWidth)
    {
        return new HudRenderSnapshot(
        [
            new RenderText(FormatScore(score), 24f, 20f, PieceVisualConstants.TintWhite),
            new RenderText(FormatRemainingTime(remainingTime), Math.Max(24f, viewportWidth - 150f), 20f, PieceVisualConstants.TintWhite)
        ]);
    }
}
