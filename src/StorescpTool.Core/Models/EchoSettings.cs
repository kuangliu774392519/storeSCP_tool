namespace StorescpTool.Core.Models;

public sealed class EchoSettings
{
    public string Host { get; set; } = "127.0.0.1";
    public int Port { get; set; } = 104;
    public string CallingAeTitle { get; set; } = "RC120";
    public string CalledAeTitle { get; set; } = "CT_AE";
}
