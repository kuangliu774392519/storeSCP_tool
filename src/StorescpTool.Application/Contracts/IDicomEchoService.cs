using StorescpTool.Core.Models;

namespace StorescpTool.Application.Contracts;

public interface IDicomEchoService
{
    Task<NetworkTestResult> EchoAsync(EchoSettings settings, CancellationToken cancellationToken = default);
}
