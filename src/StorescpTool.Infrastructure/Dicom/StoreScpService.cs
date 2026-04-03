using System.Text;
using FellowOakDicom.Network;
using StorescpTool.Application.Contracts;
using StorescpTool.Core.Models;

namespace StorescpTool.Infrastructure.Dicom;

public sealed class StoreScpService : IStoreScpService
{
    private readonly ILogService _logService;
    private readonly IReceiveRecordService _receiveRecordService;
    private readonly IReceiveSessionService _receiveSessionService;
    private readonly string _baseDirectory;
    private IDicomServer? _server;
    private StoreScpStatus _status = new() { IsRunning = false, StatusText = "Stopped" };

    public StoreScpService(
        ILogService logService,
        IReceiveRecordService receiveRecordService,
        IReceiveSessionService receiveSessionService,
        string baseDirectory)
    {
        _logService = logService;
        _receiveRecordService = receiveRecordService;
        _receiveSessionService = receiveSessionService;
        _baseDirectory = baseDirectory;
    }

    public StoreScpStatus GetStatus() => _status;

    public Task StartAsync(AppConfig config, CancellationToken cancellationToken = default)
    {
        if (_server is not null && _server.IsListening)
        {
            _status = new StoreScpStatus { IsRunning = true, StatusText = "Listening" };
            return Task.CompletedTask;
        }

        var effectiveConfig = ResolvePaths(config);
        Directory.CreateDirectory(effectiveConfig.ReceiveDirectory);
        Directory.CreateDirectory(effectiveConfig.LogDirectory);
        _logService.SetLogDirectory(effectiveConfig.LogDirectory);

        var userState = new StoreScpUserState
        {
            Config = effectiveConfig,
            LogService = _logService,
            ReceiveRecordService = _receiveRecordService,
            ReceiveSessionService = _receiveSessionService
        };

        _server = string.IsNullOrWhiteSpace(effectiveConfig.ListenIp)
            ? DicomServerFactory.Create<DicomStoreScp>(
                effectiveConfig.ListenPort,
                null,
                Encoding.UTF8,
                null,
                userState,
                null)
            : DicomServerFactory.Create<DicomStoreScp>(
                effectiveConfig.ListenIp,
                effectiveConfig.ListenPort,
                null,
                Encoding.UTF8,
                null,
                userState,
                null);

        _status = _server.Exception is null
            ? new StoreScpStatus { IsRunning = true, StatusText = "Listening" }
            : new StoreScpStatus { IsRunning = false, StatusText = "Failed", LastError = _server.Exception.Message };

        if (_server.Exception is null)
        {
            _logService.Info("StoreSCP", $"StoreSCP started on {(string.IsNullOrWhiteSpace(effectiveConfig.ListenIp) ? "0.0.0.0" : effectiveConfig.ListenIp)}:{effectiveConfig.ListenPort}, AE={effectiveConfig.LocalAeTitle}");
        }
        else
        {
            _logService.Error("StoreSCP", "StoreSCP failed to start", _server.Exception);
        }

        return Task.CompletedTask;
    }

    public Task StopAsync()
    {
        if (_server is not null)
        {
            _server.Stop();
            _server.Dispose();
            _server = null;
            _logService.Info("StoreSCP", "StoreSCP stopped");
        }

        _status = new StoreScpStatus { IsRunning = false, StatusText = "Stopped" };
        return Task.CompletedTask;
    }

    private AppConfig ResolvePaths(AppConfig config)
    {
        return new AppConfig
        {
            LocalAeTitle = config.LocalAeTitle,
            ListenPort = config.ListenPort,
            ListenIp = config.ListenIp,
            ReceiveDirectory = ResolvePath(config.ReceiveDirectory),
            LogDirectory = ResolvePath(config.LogDirectory),
            ValidateCalledAe = config.ValidateCalledAe,
            EnableDetailedDicomLog = config.EnableDetailedDicomLog,
            Echo = new EchoSettings
            {
                Host = config.Echo.Host,
                Port = config.Echo.Port,
                CallingAeTitle = config.Echo.CallingAeTitle,
                CalledAeTitle = config.Echo.CalledAeTitle
            }
        };
    }

    private string ResolvePath(string path)
    {
        return Path.IsPathRooted(path) ? path : Path.Combine(_baseDirectory, path);
    }
}
