using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using StorescpTool.Application.Contracts;
using StorescpTool.Core.Models;
using StorescpTool.Core.Utilities;

namespace StorescpTool.Infrastructure.Network;

public sealed class NetworkDiagnosticsService : INetworkDiagnosticsService
{
    private static readonly string[] VirtualAdapterKeywords =
    [
        "virtual",
        "vmware",
        "virtualbox",
        "hyper-v",
        "vethernet",
        "docker",
        "wsl",
        "wireguard",
        "vpn",
        "tap",
        "tun",
        "loopback",
        "bluetooth"
    ];

    public IReadOnlyList<string> GetLocalIPv4Addresses()
    {
        return NetworkInterface
            .GetAllNetworkInterfaces()
            .Where(n => n.OperationalStatus == OperationalStatus.Up)
            .SelectMany(CreateCandidates)
            .Let(ListenIpSelector.OrderCandidates);
    }

    public async Task<NetworkTestResult> PingAsync(string host, CancellationToken cancellationToken = default)
    {
        var watch = System.Diagnostics.Stopwatch.StartNew();
        using var ping = new Ping();

        try
        {
            var reply = await ping.SendPingAsync(host, 2000);
            watch.Stop();
            return new NetworkTestResult
            {
                Success = reply.Status == IPStatus.Success,
                TestType = "Ping",
                Target = host,
                DurationMs = watch.ElapsedMilliseconds,
                Message = reply.Status == IPStatus.Success
                    ? $"Ping success, RTT={reply.RoundtripTime} ms"
                    : $"Ping failed: {reply.Status}"
            };
        }
        catch (Exception ex)
        {
            watch.Stop();
            return new NetworkTestResult
            {
                Success = false,
                TestType = "Ping",
                Target = host,
                DurationMs = watch.ElapsedMilliseconds,
                Message = $"Ping failed: {ex.Message}"
            };
        }
    }

    public async Task<NetworkTestResult> CheckTcpPortAsync(string host, int port, CancellationToken cancellationToken = default)
    {
        var watch = System.Diagnostics.Stopwatch.StartNew();
        using var client = new TcpClient();
        using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        linkedCts.CancelAfter(TimeSpan.FromSeconds(2));

        try
        {
            await client.ConnectAsync(host, port, linkedCts.Token);
            watch.Stop();
            return new NetworkTestResult
            {
                Success = true,
                TestType = "TCP",
                Target = $"{host}:{port}",
                DurationMs = watch.ElapsedMilliseconds,
                Message = "TCP port is reachable"
            };
        }
        catch (Exception ex)
        {
            watch.Stop();
            return new NetworkTestResult
            {
                Success = false,
                TestType = "TCP",
                Target = $"{host}:{port}",
                DurationMs = watch.ElapsedMilliseconds,
                Message = $"TCP check failed: {ex.Message}"
            };
        }
    }

    private static IEnumerable<ListenIpCandidate> CreateCandidates(NetworkInterface networkInterface)
    {
        var properties = networkInterface.GetIPProperties();
        var hasGateway = properties.GatewayAddresses.Any(gateway =>
            gateway.Address.AddressFamily == AddressFamily.InterNetwork &&
            !IPAddress.Any.Equals(gateway.Address));
        var isPreferredAdapterType = IsPreferredAdapterType(networkInterface);
        var isVirtualAdapter = IsVirtualAdapter(networkInterface);
        var interfaceName = string.IsNullOrWhiteSpace(networkInterface.Name)
            ? networkInterface.Description
            : networkInterface.Name;

        return properties.UnicastAddresses
            .Where(address => address.Address.AddressFamily == AddressFamily.InterNetwork)
            .Select(address => new ListenIpCandidate(
                address.Address.ToString(),
                hasGateway,
                isPreferredAdapterType,
                isVirtualAdapter,
                networkInterface.Speed,
                interfaceName));
    }

    private static bool IsPreferredAdapterType(NetworkInterface networkInterface)
    {
        return networkInterface.NetworkInterfaceType is
            NetworkInterfaceType.Ethernet or
            NetworkInterfaceType.Ethernet3Megabit or
            NetworkInterfaceType.FastEthernetFx or
            NetworkInterfaceType.FastEthernetT or
            NetworkInterfaceType.GigabitEthernet or
            NetworkInterfaceType.Wireless80211;
    }

    private static bool IsVirtualAdapter(NetworkInterface networkInterface)
    {
        if (networkInterface.NetworkInterfaceType is NetworkInterfaceType.Loopback or NetworkInterfaceType.Tunnel)
        {
            return true;
        }

        var adapterText = $"{networkInterface.Name} {networkInterface.Description}".ToLowerInvariant();
        return VirtualAdapterKeywords.Any(adapterText.Contains);
    }
}

internal static class EnumerableExtensions
{
    public static TResult Let<TSource, TResult>(this TSource source, Func<TSource, TResult> selector)
    {
        return selector(source);
    }
}
