using System.Collections.ObjectModel;
using System.Net;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using StorescpTool.Application.Contracts;
using StorescpTool.Core.Models;
using StorescpTool.Core.Utilities;

namespace StorescpTool.App.ViewModels;

public partial class MainViewModel : ObservableObject
{
    private const string SelfTestCallingAe = "STORESCU_TEST";

    private readonly IAppConfigService _configService;
    private readonly ILogService _logService;
    private readonly INetworkDiagnosticsService _networkDiagnosticsService;
    private readonly IDicomEchoService _dicomEchoService;
    private readonly IDicomStoreTestService _dicomStoreTestService;
    private readonly ILogExportService _logExportService;
    private readonly IStoreScpService _storeScpService;
    private readonly IReceiveRecordService _receiveRecordService;
    private readonly IReceiveSessionService _receiveSessionService;

    [ObservableProperty]
    private string localAeTitle = "RC120";

    [ObservableProperty]
    private string listenIp = string.Empty;

    [ObservableProperty]
    private int listenPort = 5678;

    [ObservableProperty]
    private string receiveDirectory = "data\\received";

    [ObservableProperty]
    private string logDirectory = "data\\logs";

    [ObservableProperty]
    private bool validateCalledAe;

    [ObservableProperty]
    private string pingHost = "127.0.0.1";

    [ObservableProperty]
    private string tcpHost = "127.0.0.1";

    [ObservableProperty]
    private int tcpPort = 104;

    [ObservableProperty]
    private string echoHost = "127.0.0.1";

    [ObservableProperty]
    private int echoPort = 104;

    [ObservableProperty]
    private string echoCallingAeTitle = "RC120";

    [ObservableProperty]
    private string echoCalledAeTitle = "CT_AE";

    [ObservableProperty]
    private string storeScpStatus = "Stopped";

    [ObservableProperty]
    private string networkResult = "Waiting for test";

    [ObservableProperty]
    private string localAddresses = string.Empty;

    [ObservableProperty]
    private string currentLogFilePath = string.Empty;

    [ObservableProperty]
    private string diagnosticBundlePath = string.Empty;

    [ObservableProperty]
    private string operationMessage = "Ready";

    public ObservableCollection<string> AvailableListenIps { get; } = [];
    public ObservableCollection<LogEntry> Logs { get; } = [];
    public ObservableCollection<ReceiveRecord> ReceiveRecords { get; } = [];
    public ObservableCollection<ReceiveSessionSummary> ReceiveSessions { get; } = [];

    public MainViewModel(
        IAppConfigService configService,
        ILogService logService,
        INetworkDiagnosticsService networkDiagnosticsService,
        IDicomEchoService dicomEchoService,
        IDicomStoreTestService dicomStoreTestService,
        ILogExportService logExportService,
        IStoreScpService storeScpService,
        IReceiveRecordService receiveRecordService,
        IReceiveSessionService receiveSessionService)
    {
        _configService = configService;
        _logService = logService;
        _networkDiagnosticsService = networkDiagnosticsService;
        _dicomEchoService = dicomEchoService;
        _dicomStoreTestService = dicomStoreTestService;
        _logExportService = logExportService;
        _storeScpService = storeScpService;
        _receiveRecordService = receiveRecordService;
        _receiveSessionService = receiveSessionService;

        foreach (var entry in _logService.GetEntries())
        {
            Logs.Add(entry);
        }

        foreach (var record in _receiveRecordService.GetRecords())
        {
            ReceiveRecords.Add(record);
        }

        foreach (var session in _receiveSessionService.GetSessions())
        {
            ReceiveSessions.Add(session);
        }

        _logService.EntryAdded += OnLogEntryAdded;
        _receiveRecordService.RecordAdded += OnReceiveRecordAdded;
        _receiveSessionService.SessionChanged += OnReceiveSessionChanged;
        RefreshLocalAddresses();
        CurrentLogFilePath = _logService.CurrentLogFilePath;
    }

    public async Task InitializeAsync()
    {
        var config = await _configService.LoadAsync();
        var availableIps = _networkDiagnosticsService.GetLocalIPv4Addresses();
        UpdateAvailableListenIps(availableIps);
        LocalAeTitle = config.LocalAeTitle;
        ListenIp = ResolveListenIp(config.ListenIp, availableIps);
        ListenPort = config.ListenPort;
        ReceiveDirectory = config.ReceiveDirectory;
        LogDirectory = config.LogDirectory;
        ValidateCalledAe = config.ValidateCalledAe;
        EchoHost = config.Echo.Host;
        EchoPort = config.Echo.Port;
        EchoCallingAeTitle = config.Echo.CallingAeTitle;
        EchoCalledAeTitle = config.Echo.CalledAeTitle;
        PingHost = config.Echo.Host;
        TcpHost = config.Echo.Host;
        TcpPort = config.Echo.Port;
        _logService.SetLogDirectory(LogDirectory);
        CurrentLogFilePath = _logService.CurrentLogFilePath;
        StoreScpStatus = _storeScpService.GetStatus().StatusText;
        _logService.Info("App", "Configuration loaded");
        OperationMessage = "Configuration loaded";
    }

    [RelayCommand]
    private async Task StartStoreScpAsync()
    {
        var config = BuildConfig();
        if (!TryValidateStoreConfiguration(config))
        {
            return;
        }

        _logService.SetLogDirectory(config.LogDirectory);
        CurrentLogFilePath = _logService.CurrentLogFilePath;
        await _configService.SaveAsync(config);
        await _storeScpService.StartAsync(config);
        StoreScpStatus = _storeScpService.GetStatus().StatusText;
        OperationMessage = $"StoreSCP status: {StoreScpStatus}";
    }

    [RelayCommand]
    private async Task StopStoreScpAsync()
    {
        await _storeScpService.StopAsync();
        StoreScpStatus = _storeScpService.GetStatus().StatusText;
        OperationMessage = "StoreSCP stopped";
    }

    [RelayCommand]
    private async Task SaveConfigAsync()
    {
        var config = BuildConfig();
        if (!TryValidateStoreConfiguration(config))
        {
            return;
        }

        _logService.SetLogDirectory(config.LogDirectory);
        CurrentLogFilePath = _logService.CurrentLogFilePath;
        await _configService.SaveAsync(config);
        _logService.Info("App", "Configuration saved");
        OperationMessage = "Configuration saved";
    }

    [RelayCommand]
    private async Task PingAsync()
    {
        if (string.IsNullOrWhiteSpace(PingHost))
        {
            SetValidationFailure("Ping host cannot be empty.");
            return;
        }

        var result = await _networkDiagnosticsService.PingAsync(PingHost);
        NetworkResult = FormatResult(result);
        LogResult(result);
    }

    [RelayCommand]
    private async Task CheckPortAsync()
    {
        if (!TryValidateHost(TcpHost))
        {
            SetValidationFailure("TCP host is invalid.");
            return;
        }

        if (!TryValidatePort(TcpPort))
        {
            SetValidationFailure("TCP port must be between 1 and 65535.");
            return;
        }

        var result = await _networkDiagnosticsService.CheckTcpPortAsync(TcpHost, TcpPort);
        NetworkResult = FormatResult(result);
        LogResult(result);
    }

    [RelayCommand]
    private async Task EchoAsync()
    {
        if (!TryValidateHost(EchoHost))
        {
            SetValidationFailure("C-ECHO host is invalid.");
            return;
        }

        if (!TryValidatePort(EchoPort))
        {
            SetValidationFailure("C-ECHO port must be between 1 and 65535.");
            return;
        }

        if (!TryValidateAeTitle(EchoCallingAeTitle) || !TryValidateAeTitle(EchoCalledAeTitle))
        {
            SetValidationFailure("AE Title cannot be empty and must be 16 characters or fewer.");
            return;
        }

        var result = await _dicomEchoService.EchoAsync(new EchoSettings
        {
            Host = EchoHost,
            Port = EchoPort,
            CallingAeTitle = EchoCallingAeTitle,
            CalledAeTitle = EchoCalledAeTitle
        });

        NetworkResult = FormatResult(result);
        LogResult(result);
    }

    [RelayCommand]
    private async Task SelfTestStoreAsync()
    {
        if (!TryValidateAeTitle(LocalAeTitle))
        {
            SetValidationFailure("Local AE Title is invalid.");
            return;
        }

        if (!TryValidatePort(ListenPort))
        {
            SetValidationFailure("Listen port must be between 1 and 65535.");
            return;
        }

        var targetHost = string.IsNullOrWhiteSpace(ListenIp) ? "127.0.0.1" : ListenIp;
        if (!TryValidateHost(targetHost))
        {
            SetValidationFailure("Listen IP is invalid for self test.");
            return;
        }

        var result = await _dicomStoreTestService.SendSampleStoreAsync(
            targetHost,
            ListenPort,
            SelfTestCallingAe,
            LocalAeTitle);

        NetworkResult = FormatResult(result);
        LogResult(result);
    }

    [RelayCommand]
    private async Task ExportDiagnosticsAsync()
    {
        var config = BuildConfig();
        if (!TryValidateStoreConfiguration(config))
        {
            return;
        }

        _logService.SetLogDirectory(config.LogDirectory);
        CurrentLogFilePath = _logService.CurrentLogFilePath;
        await _configService.SaveAsync(config);
        DiagnosticBundlePath = await _logExportService.ExportDiagnosticBundleAsync(config);
        _logService.Info("Export", $"Diagnostic bundle exported: {DiagnosticBundlePath}");
        OperationMessage = $"Diagnostic bundle exported: {DiagnosticBundlePath}";
    }

    [RelayCommand]
    private void RefreshLocalAddresses()
    {
        var addresses = _networkDiagnosticsService.GetLocalIPv4Addresses();
        UpdateAvailableListenIps(addresses);
        LocalAddresses = string.Join(", ", addresses);
        ListenIp = ResolveListenIp(ListenIp, addresses);
        OperationMessage = "Local IPv4 list refreshed";
    }

    private AppConfig BuildConfig()
    {
        return new AppConfig
        {
            LocalAeTitle = LocalAeTitle,
            ListenIp = ListenIp,
            ListenPort = ListenPort,
            ReceiveDirectory = ReceiveDirectory,
            LogDirectory = LogDirectory,
            ValidateCalledAe = ValidateCalledAe,
            Echo = new EchoSettings
            {
                Host = EchoHost,
                Port = EchoPort,
                CallingAeTitle = EchoCallingAeTitle,
                CalledAeTitle = EchoCalledAeTitle
            }
        };
    }

    private void OnLogEntryAdded(object? sender, LogEntry entry)
    {
        global::System.Windows.Application.Current.Dispatcher.Invoke(() =>
        {
            Logs.Insert(0, entry);
            if (Logs.Count > 500)
            {
                Logs.RemoveAt(Logs.Count - 1);
            }

            CurrentLogFilePath = _logService.CurrentLogFilePath;
        });
    }

    private void OnReceiveRecordAdded(object? sender, ReceiveRecord record)
    {
        global::System.Windows.Application.Current.Dispatcher.Invoke(() =>
        {
            ReceiveRecords.Insert(0, record);
            if (ReceiveRecords.Count > 200)
            {
                ReceiveRecords.RemoveAt(ReceiveRecords.Count - 1);
            }
        });
    }

    private void OnReceiveSessionChanged(object? sender, ReceiveSessionSummary session)
    {
        global::System.Windows.Application.Current.Dispatcher.Invoke(() =>
        {
            var existingIndex = ReceiveSessions
                .Select((item, index) => new { item, index })
                .FirstOrDefault(x => x.item.SessionId == session.SessionId)?.index ?? -1;

            if (existingIndex >= 0)
            {
                ReceiveSessions[existingIndex] = session;
            }
            else
            {
                ReceiveSessions.Insert(0, session);
            }

            if (ReceiveSessions.Count > 200)
            {
                ReceiveSessions.RemoveAt(ReceiveSessions.Count - 1);
            }
        });
    }

    private void LogResult(NetworkTestResult result)
    {
        var message = $"{result.TestType} {result.Target}: {result.Message}";
        if (result.Success)
        {
            _logService.Info("Network", message);
        }
        else
        {
            _logService.Warning("Network", message);
        }

        OperationMessage = message;
    }

    private void UpdateAvailableListenIps(IReadOnlyList<string> addresses)
    {
        AvailableListenIps.Clear();
        foreach (var address in addresses)
        {
            AvailableListenIps.Add(address);
        }

        LocalAddresses = string.Join(", ", addresses);
    }

    private static string ResolveListenIp(string configuredIp, IReadOnlyList<string> availableIps)
    {
        if (!string.IsNullOrWhiteSpace(configuredIp) &&
            availableIps.Contains(configuredIp, StringComparer.OrdinalIgnoreCase))
        {
            return configuredIp;
        }

        return ListenIpSelector.Select(string.Empty, availableIps);
    }

    private static string FormatResult(NetworkTestResult result)
    {
        return $"{result.TestType} | {(result.Success ? "Success" : "Failed")} | {result.Target} | {result.Message} | {result.DurationMs} ms";
    }

    private bool TryValidateStoreConfiguration(AppConfig config)
    {
        if (!TryValidateAeTitle(config.LocalAeTitle))
        {
            SetValidationFailure("Local AE Title cannot be empty and must be 16 characters or fewer.");
            return false;
        }

        if (!string.IsNullOrWhiteSpace(config.ListenIp) && !TryValidateHost(config.ListenIp))
        {
            SetValidationFailure("Listen IP is invalid.");
            return false;
        }

        if (!TryValidatePort(config.ListenPort))
        {
            SetValidationFailure("Listen port must be between 1 and 65535.");
            return false;
        }

        if (string.IsNullOrWhiteSpace(config.ReceiveDirectory))
        {
            SetValidationFailure("Receive directory cannot be empty.");
            return false;
        }

        if (string.IsNullOrWhiteSpace(config.LogDirectory))
        {
            SetValidationFailure("Log directory cannot be empty.");
            return false;
        }

        return true;
    }

    private void SetValidationFailure(string message)
    {
        OperationMessage = message;
        _logService.Warning("Validation", message);
    }

    private static bool TryValidateAeTitle(string value)
    {
        return !string.IsNullOrWhiteSpace(value) && value.Trim().Length <= 16;
    }

    private static bool TryValidatePort(int port)
    {
        return port is >= 1 and <= 65535;
    }

    private static bool TryValidateHost(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        return IPAddress.TryParse(value, out _) ||
               Uri.CheckHostName(value) is UriHostNameType.Dns;
    }
}
