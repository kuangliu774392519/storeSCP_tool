using System.Net;
using System.Net.Sockets;
using FellowOakDicom;
using Microsoft.Extensions.DependencyInjection;
using StorescpTool.Core.Models;
using StorescpTool.Infrastructure.Dicom;
using StorescpTool.Infrastructure.Logging;
using StorescpTool.Infrastructure.Records;
using StorescpTool.Infrastructure.Sessions;

namespace StorescpTool.Tests;

public sealed class StoreScpIntegrationTests
{
    [Fact]
    public async Task LocalStoreSelfTest_RoundTripsThroughStoreScp()
    {
        var services = new ServiceCollection();
        services.AddFellowOakDicom();
        using var serviceProvider = services.BuildServiceProvider();
        DicomSetupBuilder.UseServiceProvider(serviceProvider);

        var baseDirectory = Path.Combine(Path.GetTempPath(), "storescp_tool_integration", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(baseDirectory);

        var logService = new LogService(baseDirectory);
        var recordService = new ReceiveRecordService(baseDirectory);
        var sessionService = new ReceiveSessionService(baseDirectory);
        var storeScpService = new StoreScpService(logService, recordService, sessionService, baseDirectory);
        var storeTestService = new DicomStoreTestService();
        var port = GetFreePort();

        try
        {
            await storeScpService.StartAsync(new AppConfig
            {
                LocalAeTitle = "LOCAL_SCP",
                ListenPort = port,
                ListenIp = string.Empty,
                ReceiveDirectory = "received",
                LogDirectory = "logs"
            });

            var result = await storeTestService.SendSampleStoreAsync("127.0.0.1", port, "LOCAL_SCU", "LOCAL_SCP");

            for (var i = 0; i < 20 && recordService.GetRecords().Count == 0; i++)
            {
                await Task.Delay(100);
            }

            Assert.True(result.Success, result.Message);
            Assert.Single(recordService.GetRecords());
            Assert.Single(sessionService.GetSessions());
            Assert.True(File.Exists(recordService.GetRecords()[0].FilePath));
        }
        finally
        {
            await storeScpService.StopAsync();
        }
    }

    private static int GetFreePort()
    {
        var listener = new TcpListener(IPAddress.Loopback, 0);
        listener.Start();
        var port = ((IPEndPoint)listener.LocalEndpoint).Port;
        listener.Stop();
        return port;
    }
}
