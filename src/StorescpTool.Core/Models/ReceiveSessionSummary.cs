namespace StorescpTool.Core.Models;

public sealed class ReceiveSessionSummary
{
    public string SessionId { get; init; } = string.Empty;
    public DateTime StartTime { get; init; } = DateTime.Now;
    public DateTime? EndTime { get; init; }
    public string CallingAe { get; init; } = string.Empty;
    public string CalledAe { get; init; } = string.Empty;
    public string RemoteIp { get; init; } = string.Empty;
    public int ReceivedCount { get; init; }
    public string Status { get; init; } = "Started";
    public string LastFilePath { get; init; } = string.Empty;
    public string ErrorMessage { get; init; } = string.Empty;
}
