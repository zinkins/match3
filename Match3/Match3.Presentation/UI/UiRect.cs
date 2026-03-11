using System.Numerics;

namespace Match3.Presentation.UI;

public readonly record struct UiRect(float X, float Y, float Width, float Height)
{
    public bool Contains(Vector2 point)
    {
        return point.X >= X &&
               point.X <= X + Width &&
               point.Y >= Y &&
               point.Y <= Y + Height;
    }
}
