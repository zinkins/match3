using System;
using System.Numerics;
using Match3.Presentation.Rendering;

namespace Match3.Presentation.UI;

public sealed class LayoutCalculator
{
    private const float MinSafePadding = 16f;
    private const float HudHeight = 48f;
    private const float HudSpacing = 16f;
    private const float ButtonWidth = 220f;
    private const float ButtonHeight = 70f;
    private const float TitleWidth = 220f;

    public UiRect GetSafeBounds(float viewportWidth, float viewportHeight)
    {
        var horizontalPadding = MathF.Max(MinSafePadding, viewportWidth * 0.04f);
        var verticalPadding = MathF.Max(MinSafePadding, viewportHeight * 0.04f);
        return new UiRect(
            horizontalPadding,
            verticalPadding,
            MathF.Max(0f, viewportWidth - (horizontalPadding * 2f)),
            MathF.Max(0f, viewportHeight - (verticalPadding * 2f)));
    }

    public GameplayLayout CalculateGameplayLayout(float viewportWidth, float viewportHeight, int rows = 8, int columns = 8)
    {
        var safeBounds = GetSafeBounds(viewportWidth, viewportHeight);
        var availableHeight = MathF.Max(0f, safeBounds.Height - HudHeight - HudSpacing);
        var cellSize = MathF.Floor(MathF.Min(safeBounds.Width / columns, availableHeight / rows));
        if (cellSize <= 0f)
        {
            cellSize = 1f;
        }

        var boardWidth = cellSize * columns;
        var boardHeight = cellSize * rows;
        var origin = new Vector2(
            safeBounds.X + ((safeBounds.Width - boardWidth) / 2f),
            safeBounds.Y + HudHeight + HudSpacing + ((availableHeight - boardHeight) / 2f));

        return new GameplayLayout(
            new BoardTransform(cellSize, origin, rows, columns),
            new UiRect(safeBounds.X, safeBounds.Y, safeBounds.Width, HudHeight),
            safeBounds);
    }

    public MenuLayout CalculateMainMenuLayout(float viewportWidth, float viewportHeight)
    {
        var safeBounds = GetSafeBounds(viewportWidth, viewportHeight);
        var buttonY = safeBounds.Y + (safeBounds.Height * 0.28f);
        return new MenuLayout(
            (safeBounds.X + ((safeBounds.Width - TitleWidth) / 2f), safeBounds.Y + 24f),
            CreateCenteredButton(safeBounds, buttonY),
            safeBounds,
            safeBounds);
    }

    public MenuLayout CalculateGameOverLayout(float viewportWidth, float viewportHeight)
    {
        var safeBounds = GetSafeBounds(viewportWidth, viewportHeight);
        var popupWidth = MathF.Min(safeBounds.Width * 0.72f, 520f);
        var popupHeight = MathF.Min(safeBounds.Height * 0.82f, 500f);
        var popupX = safeBounds.X + ((safeBounds.Width - popupWidth) / 2f);
        var popupY = safeBounds.Y + ((safeBounds.Height - popupHeight) / 2f);
        var popupBounds = new UiRect(popupX, popupY, popupWidth, popupHeight);
        var buttonWidth = MathF.Min(ButtonWidth, popupBounds.Width - 48f);
        var buttonBounds = new UiRect(
            popupBounds.X + ((popupBounds.Width - buttonWidth) / 2f),
            popupBounds.Y + popupBounds.Height - ButtonHeight - 24f,
            buttonWidth,
            ButtonHeight);
        var titlePosition = (
            popupBounds.X + ((popupBounds.Width - TitleWidth) / 2f),
            popupBounds.Y + 28f);

        return new MenuLayout(titlePosition, buttonBounds, safeBounds, popupBounds);
    }

    private static UiRect CreateCenteredButton(UiRect safeBounds, float y)
    {
        var width = MathF.Min(ButtonWidth, safeBounds.Width);
        var x = safeBounds.X + ((safeBounds.Width - width) / 2f);
        return new UiRect(x, y, width, ButtonHeight);
    }
}

public sealed record GameplayLayout(BoardTransform BoardTransform, UiRect HudBounds, UiRect SafeBounds);

public sealed record MenuLayout((float X, float Y) TitlePosition, UiRect ButtonBounds, UiRect SafeBounds, UiRect PanelBounds);