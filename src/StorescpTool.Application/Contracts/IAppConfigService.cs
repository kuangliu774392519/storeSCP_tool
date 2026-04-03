using StorescpTool.Core.Models;

namespace StorescpTool.Application.Contracts;

public interface IAppConfigService
{
    Task<AppConfig> LoadAsync(CancellationToken cancellationToken = default);
    Task SaveAsync(AppConfig config, CancellationToken cancellationToken = default);
}
