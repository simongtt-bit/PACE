using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Pace.Engineer.App.Debugging;
using Pace.Engineer.App.Services;
using Pace.Engineer.App.ViewModels;
using Pace.Engineer.Core.Interfaces;

namespace Pace.Engineer.App;

public partial class App : Application
{
    private IHost? _host;
    private CancellationTokenSource? _telemetryCancellationTokenSource;

    protected override async void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        _host = Host.CreateDefaultBuilder()
            .ConfigureServices(services =>
            {
                services.AddSingleton<ILiveTelemetrySource, FakeTelemetrySource>();
                services.AddSingleton<ISessionSnapshotPublisher, SessionSnapshotPublisher>();

                services.AddSingleton<MainWindowViewModel>();
                services.AddSingleton<MainWindow>();
            })
            .Build();

        await _host.StartAsync();

        var mainWindow = _host.Services.GetRequiredService<MainWindow>();
        var publisher = _host.Services.GetRequiredService<ISessionSnapshotPublisher>();

        _telemetryCancellationTokenSource = new CancellationTokenSource();

        _ = Task.Run(() => publisher.StartAsync(_telemetryCancellationTokenSource.Token));

        mainWindow.Show();
    }

    protected override async void OnExit(ExitEventArgs e)
    {
        _telemetryCancellationTokenSource?.Cancel();

        if (_host is not null)
        {
            await _host.StopAsync();
            _host.Dispose();
        }

        _telemetryCancellationTokenSource?.Dispose();

        base.OnExit(e);
    }
}