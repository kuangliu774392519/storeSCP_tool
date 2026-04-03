namespace StorescpTool.Core.Models;

public sealed class StoreScpStatus
{
    public bool IsRunning { get; init; }
    public string StatusText { get; init; } = "Stopped";
    public string? LastError { get; init; }
}
