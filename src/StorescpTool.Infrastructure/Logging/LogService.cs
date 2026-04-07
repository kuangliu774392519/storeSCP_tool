using System.Text;
using StorescpTool.Application.Contracts;
using StorescpTool.Core.Models;

namespace StorescpTool.Infrastructure.Logging;

public sealed class LogService : ILogService
{
    private readonly List<LogEntry> _entries = [];
    private readonly object _sync = new();
    private readonly string _baseDirectory;
    private string _logDirectory;

    public event EventHandler<LogEntry>? EntryAdded;

    public LogService(string baseDirectory)
    {
        _baseDirectory = baseDirectory;
        _logDirectory = Path.Combine(_baseDirectory, "data", "logs");
        Directory.CreateDirectory(_logDirectory);
    }

    public string CurrentLogFilePath => Path.Combine(_logDirectory, $"app_{DateTime.Now:yyyy-MM-dd}.log");

    public IReadOnlyList<LogEntry> GetEntries()
    {
        lock (_sync)
        {
            return _entries.ToList();
        }
    }

    public void SetLogDirectory(string directoryPath)
    {
        var resolvedPath = Path.IsPathRooted(directoryPath)
            ? directoryPath
            : Path.Combine(_baseDirectory, directoryPath);

        Directory.CreateDirectory(resolvedPath);

        lock (_sync)
        {
            _logDirectory = resolvedPath;
        }
    }

    public void Info(string module, string message, string dimseInfo = "") => Add("Info", module, message, dimseInfo);

    public void Warning(string module, string message, string dimseInfo = "") => Add("Warning", module, message, dimseInfo);

    public void Error(string module, string message, Exception? exception = null, string dimseInfo = "")
    {
        var finalMessage = exception is null ? message : $"{message} | {exception.Message}";
        Add("Error", module, finalMessage, dimseInfo);
    }

    private void Add(string level, string module, string message, string dimseInfo)
    {
        var entry = new LogEntry
        {
            Timestamp = DateTime.Now,
            Level = level,
            Module = module,
            DimseInfo = dimseInfo,
            Message = message
        };

        lock (_sync)
        {
            _entries.Add(entry);
            if (_entries.Count > 2000)
            {
                _entries.RemoveAt(0);
            }

            try
            {
                var dimseSegment = string.IsNullOrWhiteSpace(entry.DimseInfo)
                    ? string.Empty
                    : $" [DIMSE:{entry.DimseInfo}]";

                File.AppendAllText(
                    CurrentLogFilePath,
                    $"{entry.Timestamp:yyyy-MM-dd HH:mm:ss.fff} [{entry.Level}] [{entry.Module}]{dimseSegment} {entry.Message}{Environment.NewLine}",
                    Encoding.UTF8);
            }
            catch
            {
                // Logging must never break the main workflow.
            }
        }

        EntryAdded?.Invoke(this, entry);
    }
}
