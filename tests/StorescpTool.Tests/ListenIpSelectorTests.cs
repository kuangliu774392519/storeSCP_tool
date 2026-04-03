using StorescpTool.Core.Models;
using StorescpTool.Core.Utilities;

namespace StorescpTool.Tests;

public sealed class ListenIpSelectorTests
{
    [Fact]
    public void Select_ReturnsConfiguredIp_WhenPresent()
    {
        var result = ListenIpSelector.Select("10.0.0.5", ["192.168.1.10"]);

        Assert.Equal("10.0.0.5", result);
    }

    [Fact]
    public void Select_ReturnsFirstAvailableIp_WhenConfigIsEmpty()
    {
        var result = ListenIpSelector.Select(string.Empty, ["192.168.1.10", "192.168.1.11"]);

        Assert.Equal("192.168.1.10", result);
    }

    [Fact]
    public void OrderCandidates_PrefersGatewayAndPhysicalAdapter()
    {
        var result = ListenIpSelector.OrderCandidates(
        [
            new ListenIpCandidate("10.10.10.10", HasGateway: false, IsPreferredAdapterType: false, IsVirtualAdapter: true, Speed: 1_000_000, InterfaceName: "vEthernet"),
            new ListenIpCandidate("192.168.1.20", HasGateway: true, IsPreferredAdapterType: true, IsVirtualAdapter: false, Speed: 1_000_000_000, InterfaceName: "Ethernet")
        ]);

        Assert.Equal("192.168.1.20", result.First());
    }

    [Fact]
    public void OrderCandidates_DeprioritizesVirtualAdapter_WhenBothHaveGateway()
    {
        var result = ListenIpSelector.OrderCandidates(
        [
            new ListenIpCandidate("172.16.0.5", HasGateway: true, IsPreferredAdapterType: false, IsVirtualAdapter: true, Speed: 10_000_000_000, InterfaceName: "VPN"),
            new ListenIpCandidate("192.168.10.8", HasGateway: true, IsPreferredAdapterType: true, IsVirtualAdapter: false, Speed: 100_000_000, InterfaceName: "Wi-Fi")
        ]);

        Assert.Equal("192.168.10.8", result.First());
    }
}
