using System.Windows;
using FellowOakDicom;
using Microsoft.Extensions.DependencyInjection;
using StorescpTool.App.ViewModels;
using StorescpTool.Infrastructure.Bootstrapper;

namespace StorescpTool.App;

public partial class App : global::System.Windows.Application
{
    private ServiceProvider? _serviceProvider;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        var services = new ServiceCollection();
        services.AddStorescpToolInfrastructure(AppContext.BaseDirectory);
        services.AddSingleton<MainViewModel>();
        services.AddSingleton<MainWindow>();

        _serviceProvider = services.BuildServiceProvider();
        DicomSetupBuilder.UseServiceProvider(_serviceProvider);

        var window = _serviceProvider.GetRequiredService<MainWindow>();
        window.Show();
    }

    protected override void OnExit(ExitEventArgs e)
    {
        _serviceProvider?.Dispose();
        base.OnExit(e);
    }
}
