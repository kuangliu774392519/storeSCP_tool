using StorescpTool.Core.Models;

namespace StorescpTool.Application.Contracts;

public interface ILogService
{
    event EventHandler<LogEntry>? EntryAdded;
    string CurrentLogFilePath { get; }
    IReadOnlyList<LogEntry> GetEntries();
    void SetLogDirectory(string directoryPath);
    void Info(string module, string message, string dimseInfo = "");
    void Warning(string module, string message, string dimseInfo = "");
    void Error(string module, string message, Exception? exception = null, string dimseInfo = "");
}
