using System;
using Match3.Presentation.UI;

namespace Match3.Presentation.Screens;

public sealed class GameOverScreen : IScreen
{
    public GameOverScreen()
    {
        OkButton = new UiButton("Ok", () => OkRequested?.Invoke());
    }

    public string Name => "GameOver";

    public string Message => "Game Over";

    public UiButton OkButton { get; }

    public event Action? OkRequested;
}
