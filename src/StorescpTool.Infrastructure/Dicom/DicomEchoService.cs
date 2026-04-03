using FellowOakDicom.Network;
using FellowOakDicom.Network.Client;
using StorescpTool.Application.Contracts;
using StorescpTool.Core.Models;

namespace StorescpTool.Infrastructure.Dicom;

public sealed class DicomEchoService : IDicomEchoService
{
    public async Task<NetworkTestResult> EchoAsync(EchoSettings settings, CancellationToken cancellationToken = default)
    {
        var watch = System.Diagnostics.Stopwatch.StartNew();

        try
        {
            DicomCEchoResponse? response = null;
            var client = DicomClientFactory.Create(
                settings.Host,
                settings.Port,
                false,
                settings.CallingAeTitle,
                settings.CalledAeTitle);

            await client.AddRequestAsync(new DicomCEchoRequest
            {
                OnResponseReceived = (_, received) => response = received
            });

            await client.SendAsync(cancellationToken);
            watch.Stop();

            return new NetworkTestResult
            {
                Success = response?.Status == DicomStatus.Success,
                TestType = "C-ECHO",
                Target = $"{settings.Host}:{settings.Port}",
                DurationMs = watch.ElapsedMilliseconds,
                Message = response is null
                    ? "No response from remote AE"
                    : $"C-ECHO response: {response.Status}"
            };
        }
        catch (Exception ex)
        {
            watch.Stop();
            return new NetworkTestResult
            {
                Success = false,
                TestType = "C-ECHO",
                Target = $"{settings.Host}:{settings.Port}",
                DurationMs = watch.ElapsedMilliseconds,
                Message = $"C-ECHO failed: {ex.Message}"
            };
        }
    }
}
