using System.Windows;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Pace.Engineer.Analysis.Services;
using Pace.Engineer.App.Services;
using Pace.Engineer.App.ViewModels;
using Pace.Engineer.Core.Interfaces;
using Pace.Engineer.Telemetry.AssettoCorsa;

namespace Pace.Engineer.App;

public partial class App : Application
{
    private IHost? _host;
    private CancellationTokenSource? _telemetryCancellationTokenSource;

    protected override async void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        _host = Host.CreateDefaultBuilder()
            .ConfigureServices(
                (context, services) =>
                {
                    services.AddSingleton<AssettoCorsaTelemetrySource>();

                    services.AddSingleton<ILiveTelemetrySource>(sp =>
                        sp.GetRequiredService<AssettoCorsaTelemetrySource>()
                    );

                    services.AddSingleton<ITelemetryConnectionMonitor>(sp =>
                        sp.GetRequiredService<AssettoCorsaTelemetrySource>()
                    );

                    services.AddSingleton<ISessionSnapshotPublisher, SessionSnapshotPublisher>();

                    services.AddSingleton<FuelProjectionService>();
                    services.AddSingleton<TyreAnalysisService>();
                    services.AddSingleton<PaceAnalysisService>();
                    services.AddSingleton<EngineerService>();
                    services.AddSingleton<IVoiceClipService, VoiceClipService>();
                    services.AddSingleton<IEngineerClipResolver, EngineerClipResolver>();
                    services.AddSingleton<IEngineerAudioService, EngineerAudioService>();

                    services.AddSingleton<MainWindowViewModel>();
                    services.AddSingleton<MainWindow>();
                }
            )
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
