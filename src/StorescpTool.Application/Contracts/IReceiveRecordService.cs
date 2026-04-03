using StorescpTool.Core.Models;

namespace StorescpTool.Application.Contracts;

public interface IReceiveRecordService
{
    event EventHandler<ReceiveRecord>? RecordAdded;
    IReadOnlyList<ReceiveRecord> GetRecords();
    void Add(ReceiveRecord record);
}
