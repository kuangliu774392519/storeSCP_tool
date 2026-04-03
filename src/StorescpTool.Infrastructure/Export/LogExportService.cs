using System.Text.Json;
using StorescpTool.Application.Contracts;
using StorescpTool.Core.Models;

namespace StorescpTool.Infrastructure.Export;

public sealed class LogExportService : ILogExportService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true
    };

    private readonly string _baseDirectory;
    private readonly ILogService _logService;
    private readonly IReceiveRecordService _receiveRecordService;
    private readonly IReceiveSessionService _receiveSessionService;

    public LogExportService(
        string baseDirectory,
        ILogService logService,
        IReceiveRecordService receiveRecordService,
        IReceiveSessionService receiveSessionService)
    {
        _baseDirectory = baseDirectory;
        _logService = logService;
        _receiveRecordService = receiveRecordService;
        _receiveSessionService = receiveSessionService;
    }

    public async Task<string> ExportDiagnosticBundleAsync(AppConfig config, CancellationToken cancellationToken = default)
    {
        var exportRoot = Path.Combine(_baseDirectory, "data", "export");
        Directory.CreateDirectory(exportRoot);

        var bundlePath = Path.Combine(exportRoot, $"diagnostic_bundle_{DateTime.Now:yyyyMMdd_HHmmss}");
        Directory.CreateDirectory(bundlePath);

        var currentLogPath = _logService.CurrentLogFilePath;
        if (File.Exists(currentLogPath))
        {
            File.Copy(currentLogPath, Path.Combine(bundlePath, Path.GetFileName(currentLogPath)), true);
        }

        await File.WriteAllTextAsync(
            Path.Combine(bundlePath, "config_snapshot.json"),
            JsonSerializer.Serialize(config, JsonOptions),
            cancellationToken);

        await File.WriteAllTextAsync(
            Path.Combine(bundlePath, "receive_records.json"),
            JsonSerializer.Serialize(_receiveRecordService.GetRecords(), JsonOptions),
            cancellationToken);

        await File.WriteAllTextAsync(
            Path.Combine(bundlePath, "receive_sessions.json"),
            JsonSerializer.Serialize(_receiveSessionService.GetSessions(), JsonOptions),
            cancellationToken);

        return bundlePath;
    }
}
