using StorescpTool.Core.Models;

namespace StorescpTool.Application.Contracts;

public interface IReceiveSessionService
{
    event EventHandler<ReceiveSessionSummary>? SessionChanged;
    IReadOnlyList<ReceiveSessionSummary> GetSessions();
    void StartSession(string sessionId, string callingAe, string calledAe, string remoteIp);
    void MarkFileReceived(string sessionId, string filePath);
    void CompleteSession(string sessionId, string status, string? errorMessage = null);
}
