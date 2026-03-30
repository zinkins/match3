using System.Numerics;

namespace Match3.Presentation.Runtime;

public readonly record struct InputState(
    bool HasPointer,
    Vector2 PointerPosition,
    bool IsPrimaryDown,
    bool WasPrimaryDown,
    int ViewportWidth,
    int ViewportHeight)
{
    public bool IsPrimaryClick => HasPointer && IsPrimaryDown && !WasPrimaryDown;
}
