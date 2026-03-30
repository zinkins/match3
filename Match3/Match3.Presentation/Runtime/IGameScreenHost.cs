using System;

namespace Match3.Presentation.Runtime;

public interface IGameScreenHost
{
    void Update(TimeSpan elapsed, InputState inputState);

    void Draw(IGameCanvas canvas);
}
