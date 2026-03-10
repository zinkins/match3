using System;

namespace Match3.Core.GameCore.Board;

public sealed class SystemRandomSource : IRandomSource
{
    private readonly Random random;

    public SystemRandomSource()
        : this(Random.Shared)
    {
    }

    public SystemRandomSource(Random random)
    {
        this.random = random ?? throw new ArgumentNullException(nameof(random));
    }

    public int Next(int minInclusive, int maxExclusive)
    {
        return random.Next(minInclusive, maxExclusive);
    }
}
