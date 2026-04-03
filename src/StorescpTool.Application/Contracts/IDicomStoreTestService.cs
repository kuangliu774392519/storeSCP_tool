using StorescpTool.Core.Models;

namespace StorescpTool.Application.Contracts;

public interface IDicomStoreTestService
{
    Task<NetworkTestResult> SendSampleStoreAsync(
        string host,
        int port,
        string callingAe,
        string calledAe,
        CancellationToken cancellationToken = default);
}
