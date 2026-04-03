using StorescpTool.Core.Models;
using StorescpTool.Infrastructure.Export;
using StorescpTool.Infrastructure.Logging;
using StorescpTool.Infrastructure.Records;
using StorescpTool.Infrastructure.Sessions;

namespace StorescpTool.Tests;

public sealed class LogExportServiceTests
{
    [Fact]
    public async Task ExportDiagnosticBundleAsync_WritesExpectedFiles()
    {
        var baseDirectory = Path.Combine(Path.GetTempPath(), "storescp_tool_tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(baseDirectory);

        var logService = new LogService(baseDirectory);
        var recordService = new ReceiveRecordService(baseDirectory);
        var sessionService = new ReceiveSessionService(baseDirectory);

        logService.Info("Test", "hello");
        recordService.Add(new ReceiveRecord { SopInstanceUid = "1.2.3", FilePath = "test.dcm" });
        sessionService.StartSession("session-1", "CALLING", "CALLED", "127.0.0.1");
        sessionService.MarkFileReceived("session-1", "test.dcm");
        sessionService.CompleteSession("session-1", "Completed");

        var service = new LogExportService(baseDirectory, logService, recordService, sessionService);
        var config = new AppConfig { LocalAeTitle = "TEST_AE" };

        var bundlePath = await service.ExportDiagnosticBundleAsync(config);

        Assert.True(Directory.Exists(bundlePath));
        Assert.True(File.Exists(Path.Combine(bundlePath, "config_snapshot.json")));
        Assert.True(File.Exists(Path.Combine(bundlePath, "receive_records.json")));
        Assert.True(File.Exists(Path.Combine(bundlePath, "receive_sessions.json")));
    }
}
