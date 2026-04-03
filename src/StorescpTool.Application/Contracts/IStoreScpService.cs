using StorescpTool.Core.Models;

namespace StorescpTool.Application.Contracts;

public interface IStoreScpService
{
    StoreScpStatus GetStatus();
    Task StartAsync(AppConfig config, CancellationToken cancellationToken = default);
    Task StopAsync();
}
