using System.Numerics;

namespace Match3.Presentation.UI;

public sealed class SelectionHighlight
{
    public float GetScale(bool isSelected)
    {
        return isSelected ? 1.12f : 1f;
    }

    public float GetOpacity(bool isSelected)
    {
        return isSelected ? 1f : 0.7f;
    }

    public Matrix3x2 CreateTransform(Vector2 cellCenter, bool isSelected)
    {
        var scale = GetScale(isSelected);
        return Matrix3x2.CreateScale(scale, cellCenter);
    }
}
