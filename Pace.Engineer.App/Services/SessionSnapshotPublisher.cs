using Pace.Engineer.Core.Interfaces;
using Pace.Engineer.Core.Models;

namespace Pace.Engineer.App.Services;

public sealed class SessionSnapshotPublisher(ILiveTelemetrySource telemetrySource) : ISessionSnapshotPublisher
{
    public SessionSnapshot? Current { get; private set; }

    public event EventHandler<SessionSnapshot>? SnapshotReceived;

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await foreach (var snapshot in telemetrySource.StreamAsync(cancellationToken))
        {
            Current = snapshot;
            SnapshotReceived?.Invoke(this, snapshot);
        }
    }
}