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
        var titlePosition = ScreenLayoutMetrics.GetMainMenuTitlePosition(canvas.ViewportWidth, canvas.ViewportHeight);
        canvas.DrawText("Match3", titlePosition.X, titlePosition.Y, PieceVisualConstants.TintWhite);
        DrawButton(canvas, ScreenLayoutMetrics.GetMainMenuPlayButtonBounds(canvas.ViewportWidth, canvas.ViewportHeight), screen.PlayButton.Label);
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
        // Selection is communicated by the piece transform itself to avoid a duplicate-looking base shape.
    }

    private static void DrawPieces(IGameCanvas canvas, BoardRenderSnapshot boardSnapshot)
    {
        foreach (var piece in boardSnapshot.Pieces)
        {
            canvas.DrawShape(piece.Shape, piece.X, piece.Y, piece.Width, piece.Height, piece.Tint, piece.Rotation);
        }
    }

    private static void DrawPieces(IGameCanvas canvas, IReadOnlyList<RenderPiece> pieces)
    {
        foreach (var piece in pieces)
        {
            canvas.DrawShape(piece.Shape, piece.X, piece.Y, piece.Width, piece.Height, piece.Tint, piece.Rotation);
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
        var hudSnapshot = screen.HudRenderer.BuildSnapshot(screen.Presenter.Score, screen.Presenter.RemainingTime, canvas.ViewportWidth, canvas.ViewportHeight);
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
        var boardSnapshot = screen.BoardRenderer.BuildSnapshot(screen.Board, screen.BoardTransform, screen.Bonuses);
        var renderedPieces = screen.EffectsController.BuildPieces(boardSnapshot, screen.SelectedCell);
        DrawCells(canvas, boardSnapshot);
        DrawSelectionHighlight(canvas, screen);
        DrawPieces(canvas, renderedPieces);
        DrawHud(canvas, screen);

        if (screen.ShouldShowGameOverOverlay)
        {
            DrawGameOverOverlay(canvas);
        }
    }

    private static void DrawGameOver(IGameCanvas canvas, GameOverScreen screen)
    {
        var titlePosition = ScreenLayoutMetrics.GetGameOverTitlePosition(canvas.ViewportWidth, canvas.ViewportHeight);
        canvas.DrawText(screen.Message, titlePosition.X, titlePosition.Y, PieceVisualConstants.TintWhite);
        DrawButton(canvas, ScreenLayoutMetrics.GetGameOverOkButtonBounds(canvas.ViewportWidth, canvas.ViewportHeight), screen.OkButton.Label);
    }

    private static float EstimateTextWidth(string text)
    {
        return text.Length * 16f;
    }
}
