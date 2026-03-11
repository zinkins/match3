using System.Collections.Generic;

namespace Match3.Presentation.Rendering;

public sealed record RenderQuad(float X, float Y, float Width, float Height, string Tint);

public sealed record RenderText(string Text, float X, float Y, string Tint);

public sealed class BoardRenderSnapshot
{
    public BoardRenderSnapshot(IReadOnlyList<RenderQuad> cells, IReadOnlyList<RenderQuad> pieces)
    {
        Cells = cells;
        Pieces = pieces;
    }

    public IReadOnlyList<RenderQuad> Cells { get; }

    public IReadOnlyList<RenderQuad> Pieces { get; }
}

public sealed class HudRenderSnapshot
{
    public HudRenderSnapshot(IReadOnlyList<RenderText> labels)
    {
        Labels = labels;
    }

    public IReadOnlyList<RenderText> Labels { get; }
}
