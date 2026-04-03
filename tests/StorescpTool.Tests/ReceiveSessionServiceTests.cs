using StorescpTool.Infrastructure.Sessions;

namespace StorescpTool.Tests;

public sealed class ReceiveSessionServiceTests
{
    [Fact]
    public void CompleteSession_PreservesReceivedCount()
    {
        var baseDirectory = Path.Combine(Path.GetTempPath(), "storescp_tool_tests", Guid.NewGuid().ToString("N"));
        var service = new ReceiveSessionService(baseDirectory);
        service.StartSession("session-1", "CALLING", "CALLED", "127.0.0.1");
        service.MarkFileReceived("session-1", "a.dcm");
        service.MarkFileReceived("session-1", "b.dcm");
        service.CompleteSession("session-1", "Completed");

        var session = Assert.Single(service.GetSessions());

        Assert.Equal(2, session.ReceivedCount);
        Assert.Equal("Completed", session.Status);
        Assert.NotNull(session.EndTime);
    }

    [Fact]
    public void Sessions_AreLoadedFromDisk()
    {
        var baseDirectory = Path.Combine(Path.GetTempPath(), "storescp_tool_tests", Guid.NewGuid().ToString("N"));
        var writer = new ReceiveSessionService(baseDirectory);
        writer.StartSession("session-1", "CALLING", "CALLED", "127.0.0.1");
        writer.MarkFileReceived("session-1", "a.dcm");
        writer.CompleteSession("session-1", "Completed");

        var reader = new ReceiveSessionService(baseDirectory);
        var session = Assert.Single(reader.GetSessions());

        Assert.Equal("session-1", session.SessionId);
        Assert.Equal(1, session.ReceivedCount);
        Assert.Equal("Completed", session.Status);
    }
}
