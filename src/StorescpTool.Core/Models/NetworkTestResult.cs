namespace StorescpTool.Core.Models;

public sealed class NetworkTestResult
{
    public bool Success { get; init; }
    public string TestType { get; init; } = string.Empty;
    public string Target { get; init; } = string.Empty;
    public long DurationMs { get; init; }
    public string Message { get; init; } = string.Empty;
}
