namespace Pace.Engineer.Core.Models;

public sealed class TelemetryConnectionState
{
    public bool IsConnected { get; init; }
    public string SourceName { get; init; } = string.Empty;
    public string StatusMessage { get; init; } = string.Empty;
    public DateTimeOffset TimestampUtc { get; init; }
}