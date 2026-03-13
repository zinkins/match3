using Match3.Presentation.UI;

namespace Match3.Presentation.Screens;

public static class ScreenLayoutMetrics
{
    private static readonly LayoutCalculator Calculator = new();

    public static UiRect GetMainMenuPlayButtonBounds(float viewportWidth, float viewportHeight)
    {
        return Calculator.CalculateMainMenuLayout(viewportWidth, viewportHeight).ButtonBounds;
    }

    public static UiRect GetGameOverOkButtonBounds(float viewportWidth, float viewportHeight)
    {
        return Calculator.CalculateGameOverLayout(viewportWidth, viewportHeight).ButtonBounds;
    }

    public static UiRect GetGameOverPopupBounds(float viewportWidth, float viewportHeight)
    {
        return Calculator.CalculateGameOverLayout(viewportWidth, viewportHeight).PanelBounds;
    }

    public static (float X, float Y) GetMainMenuTitlePosition(float viewportWidth, float viewportHeight)
    {
        return Calculator.CalculateMainMenuLayout(viewportWidth, viewportHeight).TitlePosition;
    }

}
