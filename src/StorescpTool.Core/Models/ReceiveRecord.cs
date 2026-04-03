namespace StorescpTool.Core.Models;

public sealed class ReceiveRecord
{
    public DateTime ReceivedAt { get; init; } = DateTime.Now;
    public string CallingAe { get; init; } = string.Empty;
    public string CalledAe { get; init; } = string.Empty;
    public string RemoteIp { get; init; } = string.Empty;
    public string PatientName { get; init; } = string.Empty;
    public string StudyInstanceUid { get; init; } = string.Empty;
    public string SopInstanceUid { get; init; } = string.Empty;
    public string FilePath { get; init; } = string.Empty;
}
