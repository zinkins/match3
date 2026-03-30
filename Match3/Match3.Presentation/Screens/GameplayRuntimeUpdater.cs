using System;

namespace Match3.Presentation.Screens;

public sealed class GameplayRuntimeUpdater
{
    public void Update(GameplayScreen gameplay, TimeSpan elapsed)
    {
        ArgumentNullException.ThrowIfNull(gameplay);

        var elapsedSeconds = (float)elapsed.TotalSeconds;
        gameplay.Presenter.Update(elapsed);
        gameplay.VisualState.Update(elapsedSeconds);
        gameplay.AnimationPlayer.Update(elapsedSeconds);
    }
}
