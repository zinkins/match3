using System.Reflection;
using Xunit;

namespace Match3.Tests;

public class SmokeTests
{
    [Fact]
    public void Tests_Project_BuildsAndRuns()
    {
        var coreAssembly = Assembly.Load("Match3.Core");
        Assert.NotNull(coreAssembly);
        Assert.Equal("Match3.Core", coreAssembly.GetName().Name);
    }
}
