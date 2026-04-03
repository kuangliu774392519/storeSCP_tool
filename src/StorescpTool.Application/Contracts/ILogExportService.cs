using StorescpTool.Core.Models;

namespace StorescpTool.Application.Contracts;

public interface ILogExportService
{
    Task<string> ExportDiagnosticBundleAsync(AppConfig config, CancellationToken cancellationToken = default);
}
