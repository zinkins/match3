namespace Match3.Presentation.Rendering;

public sealed record PieceVisual(string Shape, string Tint);

public static class PieceVisualConstants
{
    public const string ShapeSquare = "Square";
    public const string ShapeDiamond = "Diamond";
    public const string ShapeCircle = "Circle";
    public const string ShapeUnknown = "Unknown";

    public const string TintBlack = "Black";
    public const string TintDarkGray = "DarkGray";
    public const string TintLightGray = "LightGray";
    public const string TintOrange = "Orange";
    public const string TintRed = "Red";
    public const string TintGreen = "Green";
    public const string TintBlue = "Blue";
    public const string TintYellow = "Yellow";
    public const string TintPurple = "Purple";
    public const string TintWhite = "White";
}
