namespace StorescpTool.Core.Models;

public sealed class LogEntry
{
    public DateTime Timestamp { get; init; } = DateTime.Now;
    public string Level { get; init; } = "Info";
    public string Module { get; init; } = "App";
    public string Message { get; init; } = string.Empty;
}
