namespace Match3.Core.GameCore.Board;

public interface IRandomSource
{
    int Next(int minInclusive, int maxExclusive);
}
