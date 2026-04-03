using StorescpTool.Infrastructure.Network;

namespace StorescpTool.Tests;

public sealed class NetworkDiagnosticsServiceTests
{
    [Fact]
    public void GetLocalIPv4Addresses_ReturnsCollection()
    {
        var service = new NetworkDiagnosticsService();
        var addresses = service.GetLocalIPv4Addresses();
        Assert.NotNull(addresses);
    }
}
