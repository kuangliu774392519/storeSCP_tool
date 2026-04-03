namespace StorescpTool.Core.Models;

public sealed class AppConfig
{
    public string LocalAeTitle { get; set; } = "RC120";
    public int ListenPort { get; set; } = 5678;
    public string ListenIp { get; set; } = string.Empty;
    public string ReceiveDirectory { get; set; } = "data\\received";
    public string LogDirectory { get; set; } = "data\\logs";
    public bool ValidateCalledAe { get; set; }
    public bool EnableDetailedDicomLog { get; set; } = true;
    public EchoSettings Echo { get; set; } = new();
}
