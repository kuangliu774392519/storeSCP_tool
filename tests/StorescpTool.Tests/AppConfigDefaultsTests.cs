using StorescpTool.Core.Models;

namespace StorescpTool.Tests;

public sealed class AppConfigDefaultsTests
{
    [Fact]
    public void Defaults_AreExpected()
    {
        var config = new AppConfig();

        Assert.Equal("RC120", config.LocalAeTitle);
        Assert.Equal(5678, config.ListenPort);
        Assert.Equal("RC120", config.Echo.CallingAeTitle);
    }
}
