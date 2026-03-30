namespace Match3.Presentation.Runtime;

public interface IGameCanvas
{
    int ViewportWidth { get; }

    int ViewportHeight { get; }

    void Begin();

    void End();

    void DrawFilledRectangle(float x, float y, float width, float height, string tint);

    void DrawShape(string shape, float x, float y, float width, float height, string tint, float rotationRadians = 0f);

    void DrawText(string text, float x, float y, string tint);
}
