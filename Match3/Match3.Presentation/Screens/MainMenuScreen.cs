using System;
using Match3.Presentation.UI;

namespace Match3.Presentation.Screens;

public sealed class MainMenuScreen : IScreen
{
    public MainMenuScreen()
    {
        PlayButton = new UiButton("Play", () => PlayRequested?.Invoke());
    }

    public string Name => "MainMenu";

    public UiButton PlayButton { get; }

    public event Action? PlayRequested;
}
