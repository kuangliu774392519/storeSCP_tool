using StorescpTool.Core.Models;
using StorescpTool.Infrastructure.Records;

namespace StorescpTool.Tests;

public sealed class ReceiveRecordServiceTests
{
    [Fact]
    public void Add_InsertsLatestRecordFirst()
    {
        var baseDirectory = Path.Combine(Path.GetTempPath(), "storescp_tool_tests", Guid.NewGuid().ToString("N"));
        var service = new ReceiveRecordService(baseDirectory);
        service.Add(new ReceiveRecord { SopInstanceUid = "1" });
        service.Add(new ReceiveRecord { SopInstanceUid = "2" });

        var records = service.GetRecords();

        Assert.Equal(2, records.Count);
        Assert.Equal("2", records[0].SopInstanceUid);
        Assert.Equal("1", records[1].SopInstanceUid);
    }

    [Fact]
    public void Records_AreLoadedFromDisk()
    {
        var baseDirectory = Path.Combine(Path.GetTempPath(), "storescp_tool_tests", Guid.NewGuid().ToString("N"));
        var writer = new ReceiveRecordService(baseDirectory);
        writer.Add(new ReceiveRecord { SopInstanceUid = "persisted" });

        var reader = new ReceiveRecordService(baseDirectory);
        var records = reader.GetRecords();

        Assert.Single(records);
        Assert.Equal("persisted", records[0].SopInstanceUid);
    }
}
