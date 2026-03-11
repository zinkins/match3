using System.Collections.Generic;
using Match3.Core.GameCore.ValueObjects;

namespace Match3.Presentation.Rendering;

public sealed record RenderQuad(float X, float Y, float Width, float Height, string Tint);

public sealed record RenderText(string Text, float X, float Y, string Tint);

public sealed record RenderPiece(
    GridPosition Position,
    string Shape,
    string Tint,
    float X,
    float Y,
    float Width,
    float Height,
    float Rotation = 0f,
    float Layer = 0f);

public sealed class BoardRenderSnapshot
{
    public BoardRenderSnapshot(IReadOnlyList<RenderQuad> cells, IReadOnlyList<RenderPiece> pieces)
    {
        Cells = cells;
        Pieces = pieces;
    }

    public IReadOnlyList<RenderQuad> Cells { get; }

    public IReadOnlyList<RenderPiece> Pieces { get; }
}

public sealed class HudRenderSnapshot
{
    public HudRenderSnapshot(IReadOnlyList<RenderText> labels)
    {
        Labels = labels;
    }

    public IReadOnlyList<RenderText> Labels { get; }
}
