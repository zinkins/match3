using Match3.Presentation.UI;

namespace Match3.Presentation.Screens;

public static class ScreenLayoutMetrics
{
    private const float ButtonWidth = 220f;
    private const float ButtonHeight = 70f;
    private const float TitleWidth = 220f;

    public static UiRect GetMainMenuPlayButtonBounds(float viewportWidth)
    {
        return CreateCenteredButton(viewportWidth, 130f);
    }

    public static UiRect GetGameOverOkButtonBounds(float viewportWidth)
    {
        return CreateCenteredButton(viewportWidth, 160f);
    }

    public static float GetCenteredTitleX(float viewportWidth)
    {
        return (viewportWidth - TitleWidth) / 2f;
    }

    private static UiRect CreateCenteredButton(float viewportWidth, float y)
    {
        var x = (viewportWidth - ButtonWidth) / 2f;
        return new UiRect(x, y, ButtonWidth, ButtonHeight);
    }
}
