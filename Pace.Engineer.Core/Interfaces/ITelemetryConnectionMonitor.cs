using Pace.Engineer.Core.Models;

namespace Pace.Engineer.Core.Interfaces;

public interface ITelemetryConnectionMonitor
{
    TelemetryConnectionState Current { get; }
    event EventHandler<TelemetryConnectionState>? ConnectionStateChanged;
}