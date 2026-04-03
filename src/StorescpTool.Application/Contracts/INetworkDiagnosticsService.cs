using StorescpTool.Core.Models;

namespace StorescpTool.Application.Contracts;

public interface INetworkDiagnosticsService
{
    Task<NetworkTestResult> PingAsync(string host, CancellationToken cancellationToken = default);
    Task<NetworkTestResult> CheckTcpPortAsync(string host, int port, CancellationToken cancellationToken = default);
    IReadOnlyList<string> GetLocalIPv4Addresses();
}
