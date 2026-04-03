using FellowOakDicom;
using Microsoft.Extensions.DependencyInjection;
using StorescpTool.Application.Contracts;
using StorescpTool.Infrastructure.Config;
using StorescpTool.Infrastructure.Dicom;
using StorescpTool.Infrastructure.Logging;
using StorescpTool.Infrastructure.Network;
using StorescpTool.Infrastructure.Records;
using StorescpTool.Infrastructure.Export;
using StorescpTool.Infrastructure.Sessions;

namespace StorescpTool.Infrastructure.Bootstrapper;

public static class ServiceRegistration
{
    public static IServiceCollection AddStorescpToolInfrastructure(this IServiceCollection services, string baseDirectory)
    {
        services.AddFellowOakDicom();
        services.AddSingleton<ILogService>(_ => new LogService(baseDirectory));
        services.AddSingleton<IAppConfigService>(_ => new AppConfigService(baseDirectory));
        services.AddSingleton<INetworkDiagnosticsService, NetworkDiagnosticsService>();
        services.AddSingleton<IDicomEchoService, DicomEchoService>();
        services.AddSingleton<IReceiveRecordService>(_ => new ReceiveRecordService(baseDirectory));
        services.AddSingleton<IReceiveSessionService>(_ => new ReceiveSessionService(baseDirectory));
        services.AddSingleton<IDicomStoreTestService, DicomStoreTestService>();
        services.AddSingleton<ILogExportService>(serviceProvider =>
            new LogExportService(
                baseDirectory,
                serviceProvider.GetRequiredService<ILogService>(),
                serviceProvider.GetRequiredService<IReceiveRecordService>(),
                serviceProvider.GetRequiredService<IReceiveSessionService>()));
        services.AddSingleton<IStoreScpService>(serviceProvider =>
            new StoreScpService(
                serviceProvider.GetRequiredService<ILogService>(),
                serviceProvider.GetRequiredService<IReceiveRecordService>(),
                serviceProvider.GetRequiredService<IReceiveSessionService>(),
                baseDirectory));
        return services;
    }
}
