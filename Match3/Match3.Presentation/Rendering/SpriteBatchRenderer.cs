using Match3.Core.Runtime;
using Match3.Presentation.Screens;
using Match3.Presentation.UI;

namespace Match3.Presentation.Rendering;

public sealed class SpriteBatchRenderer
{
    public void Draw(IGameCanvas canvas, IScreen screen)
    {
        switch (screen)
        {
            case MainMenuScreen mainMenu:
                DrawMainMenu(canvas, mainMenu);
                break;
            case GameplayScreen gameplay:
                DrawGameplay(canvas, gameplay);
                break;
            case GameOverScreen gameOver:
                DrawGameOver(canvas, gameOver);
                break;
        }
    }

    private static void DrawMainMenu(IGameCanvas canvas, MainMenuScreen screen)
    {
        canvas.DrawText("Match3", ScreenLayoutMetrics.GetCenteredTitleX(canvas.ViewportWidth), 50f, PieceVisualConstants.TintWhite);
        DrawButton(canvas, ScreenLayoutMetrics.GetMainMenuPlayButtonBounds(canvas.ViewportWidth), screen.PlayButton.Label);
    }

    private static void DrawButton(IGameCanvas canvas, UiRect bounds, string label)
    {
        canvas.DrawFilledRectangle(bounds.X, bounds.Y, bounds.Width, bounds.Height, PieceVisualConstants.TintLightGray);
        var textX = bounds.X + ((bounds.Width - EstimateTextWidth(label)) / 2f);
        var textY = bounds.Y + ((bounds.Height - 26f) / 2f);
        canvas.DrawText(label, textX, textY, PieceVisualConstants.TintBlack);
    }

    private static void DrawSelectionHighlight(IGameCanvas canvas, GameplayScreen screen)
    {
        if (screen.SelectedCell is not { } selectedCell)
        {
            return;
        }

        var world = screen.BoardTransform.GridToWorld(selectedCell);
        canvas.DrawFilledRectangle(
            world.X + 4f,
            world.Y + 4f,
            screen.BoardTransform.CellSize - 8f,
            screen.BoardTransform.CellSize - 8f,
            PieceVisualConstants.TintOrange);
    }

    private static void DrawPieces(IGameCanvas canvas, BoardRenderSnapshot boardSnapshot)
    {
        foreach (var piece in boardSnapshot.Pieces)
        {
            canvas.DrawFilledRectangle(piece.X, piece.Y, piece.Width, piece.Height, piece.Tint);
        }
    }

    private static void DrawCells(IGameCanvas canvas, BoardRenderSnapshot boardSnapshot)
    {
        foreach (var cell in boardSnapshot.Cells)
        {
            canvas.DrawFilledRectangle(cell.X, cell.Y, cell.Width, cell.Height, cell.Tint);
        }
    }

    private static void DrawHud(IGameCanvas canvas, GameplayScreen screen)
    {
        var hudSnapshot = screen.HudRenderer.BuildSnapshot(screen.Presenter.Score, screen.Presenter.RemainingTime, canvas.ViewportWidth);
        foreach (var label in hudSnapshot.Labels)
        {
            canvas.DrawText(label.Text, label.X, label.Y, label.Tint);
        }
    }

    private static void DrawGameOverOverlay(IGameCanvas canvas)
    {
        canvas.DrawFilledRectangle(20f, 220f, canvas.ViewportWidth - 40f, 110f, PieceVisualConstants.TintOrange);
        canvas.DrawText("Game Over", 40f, 250f, PieceVisualConstants.TintBlack);
    }

    private static void DrawGameplay(IGameCanvas canvas, GameplayScreen screen)
    {
        var boardSnapshot = screen.BoardRenderer.BuildSnapshot(screen.Board, screen.BoardTransform);
        DrawCells(canvas, boardSnapshot);
        DrawSelectionHighlight(canvas, screen);
        DrawPieces(canvas, boardSnapshot);
        DrawHud(canvas, screen);

        if (screen.ShouldShowGameOverOverlay)
        {
            DrawGameOverOverlay(canvas);
        }
    }

    private static void DrawGameOver(IGameCanvas canvas, GameOverScreen screen)
    {
        canvas.DrawText(screen.Message, ScreenLayoutMetrics.GetCenteredTitleX(canvas.ViewportWidth), 80f, PieceVisualConstants.TintWhite);
        DrawButton(canvas, ScreenLayoutMetrics.GetGameOverOkButtonBounds(canvas.ViewportWidth), screen.OkButton.Label);
    }

    private static float EstimateTextWidth(string text)
    {
        return text.Length * 16f;
    }
}
