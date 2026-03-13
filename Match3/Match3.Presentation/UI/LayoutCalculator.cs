using System;
using System.Numerics;
using Match3.Presentation.Rendering;

namespace Match3.Presentation.UI;

public sealed class LayoutCalculator
{
    public UiRect GetSafeBounds(float viewportWidth, float viewportHeight)
    {
        var horizontalPadding = MathF.Max(LayoutMetrics.MinSafePadding, viewportWidth * LayoutMetrics.SafePaddingFactor);
        var verticalPadding = MathF.Max(LayoutMetrics.MinSafePadding, viewportHeight * LayoutMetrics.SafePaddingFactor);
        return new UiRect(
            horizontalPadding,
            verticalPadding,
            MathF.Max(0f, viewportWidth - (horizontalPadding * 2f)),
            MathF.Max(0f, viewportHeight - (verticalPadding * 2f)));
    }

    public GameplayLayout CalculateGameplayLayout(float viewportWidth, float viewportHeight, int rows = 8, int columns = 8)
    {
        var safeBounds = GetSafeBounds(viewportWidth, viewportHeight);
        var availableHeight = MathF.Max(0f, safeBounds.Height - LayoutMetrics.HudHeight - LayoutMetrics.HudSpacing);
        var cellSize = MathF.Floor(MathF.Min(safeBounds.Width / columns, availableHeight / rows));
        if (cellSize <= 0f)
        {
            cellSize = 1f;
        }

        var boardWidth = cellSize * columns;
        var boardHeight = cellSize * rows;
        var origin = new Vector2(
            safeBounds.X + ((safeBounds.Width - boardWidth) / 2f),
            safeBounds.Y + LayoutMetrics.HudHeight + LayoutMetrics.HudSpacing + ((availableHeight - boardHeight) / 2f));

        return new GameplayLayout(
            new BoardTransform(cellSize, origin, rows, columns),
            new UiRect(safeBounds.X, safeBounds.Y, safeBounds.Width, LayoutMetrics.HudHeight),
            safeBounds);
    }

    public MenuLayout CalculateMainMenuLayout(float viewportWidth, float viewportHeight)
    {
        var safeBounds = GetSafeBounds(viewportWidth, viewportHeight);
        var buttonY = safeBounds.Y + (safeBounds.Height * LayoutMetrics.MainMenuButtonYFactor);
        return new MenuLayout(
            (safeBounds.X + ((safeBounds.Width - LayoutMetrics.TitleWidth) / 2f), safeBounds.Y + LayoutMetrics.MainMenuTitleTopOffset),
            CreateCenteredButton(safeBounds, buttonY),
            safeBounds,
            safeBounds);
    }

    public MenuLayout CalculateGameOverLayout(float viewportWidth, float viewportHeight)
    {
        var safeBounds = GetSafeBounds(viewportWidth, viewportHeight);
        var popupWidth = MathF.Min(safeBounds.Width * LayoutMetrics.GameOverPopupWidthFactor, LayoutMetrics.GameOverPopupMaxWidth);
        var popupHeight = MathF.Min(safeBounds.Height * LayoutMetrics.GameOverPopupHeightFactor, LayoutMetrics.GameOverPopupMaxHeight);
        var popupX = safeBounds.X + ((safeBounds.Width - popupWidth) / 2f);
        var popupY = safeBounds.Y + ((safeBounds.Height - popupHeight) / 2f);
        var popupBounds = new UiRect(popupX, popupY, popupWidth, popupHeight);
        var buttonWidth = MathF.Min(LayoutMetrics.ButtonWidth, popupBounds.Width - LayoutMetrics.GameOverPopupHorizontalPadding);
        var buttonBounds = new UiRect(
            popupBounds.X + ((popupBounds.Width - buttonWidth) / 2f),
            popupBounds.Y + popupBounds.Height - LayoutMetrics.ButtonHeight - LayoutMetrics.GameOverButtonBottomOffset,
            buttonWidth,
            LayoutMetrics.ButtonHeight);
        var titlePosition = (
            popupBounds.X + ((popupBounds.Width - LayoutMetrics.TitleWidth) / 2f),
            popupBounds.Y + LayoutMetrics.GameOverTitleTopOffset);

        return new MenuLayout(titlePosition, buttonBounds, safeBounds, popupBounds);
    }

    private static UiRect CreateCenteredButton(UiRect safeBounds, float y)
    {
        var width = MathF.Min(LayoutMetrics.ButtonWidth, safeBounds.Width);
        var x = safeBounds.X + ((safeBounds.Width - width) / 2f);
        return new UiRect(x, y, width, LayoutMetrics.ButtonHeight);
    }
}

public sealed record GameplayLayout(BoardTransform BoardTransform, UiRect HudBounds, UiRect SafeBounds);

public sealed record MenuLayout((float X, float Y) TitlePosition, UiRect ButtonBounds, UiRect SafeBounds, UiRect PanelBounds);
