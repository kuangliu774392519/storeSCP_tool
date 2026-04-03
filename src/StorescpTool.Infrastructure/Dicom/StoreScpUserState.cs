using StorescpTool.Application.Contracts;
using StorescpTool.Core.Models;

namespace StorescpTool.Infrastructure.Dicom;

internal sealed class StoreScpUserState
{
    public required AppConfig Config { get; init; }
    public required ILogService LogService { get; init; }
    public required IReceiveRecordService ReceiveRecordService { get; init; }
    public required IReceiveSessionService ReceiveSessionService { get; init; }
}
