using Pace.Engineer.Core.Interfaces;
using Pace.Engineer.Core.Models;
using Pace.Engineer.Telemetry.AssettoCorsa.SharedMemory;

namespace Pace.Engineer.Telemetry.AssettoCorsa;

public sealed class AssettoCorsaTelemetrySource : ILiveTelemetrySource, ITelemetryConnectionMonitor, IDisposable
{
    private const string PhysicsMapName = @"Local\acpmf_physics";
    private const string GraphicsMapName = @"Local\acpmf_graphics";
    private const string StaticMapName = @"Local\acpmf_static";

    private readonly MemoryMappedPageReader<PhysicsPageFile> _physicsReader = new(PhysicsMapName);
    private readonly MemoryMappedPageReader<GraphicsPageFile> _graphicsReader = new(GraphicsMapName);
    private readonly MemoryMappedPageReader<StaticPageFile> _staticReader = new(StaticMapName);

    private TelemetryConnectionState _current = new()
    {
        IsConnected = false,
        SourceName = "Assetto Corsa Shared Memory",
        StatusMessage = "Waiting for Assetto Corsa...",
        TimestampUtc = DateTimeOffset.UtcNow
    };

    public TelemetryConnectionState Current => _current;

    public event EventHandler<TelemetryConnectionState>? ConnectionStateChanged;

    public async IAsyncEnumerable<SessionSnapshot> StreamAsync(
        [System.Runtime.CompilerServices.EnumeratorCancellation]
        CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            var hasPhysics = _physicsReader.TryRead(out var physics);
            var hasGraphics = _graphicsReader.TryRead(out var graphics);
            var hasStatic = _staticReader.TryRead(out var statics);

            if (hasPhysics && hasGraphics && hasStatic)
            {
                UpdateConnectionState(true, "Connected to Assetto Corsa.");
                yield return AssettoCorsaSnapshotMapper.Map(physics, graphics, statics);
            }
            else
            {
                var missing = new List<string>();

                if (!hasPhysics)
                {
                    missing.Add("physics");
                }

                if (!hasGraphics)
                {
                    missing.Add("graphics");
                }

                if (!hasStatic)
                {
                    missing.Add("static");
                }

                UpdateConnectionState(false,
                    $"Waiting for Assetto Corsa shared memory ({string.Join(", ", missing)})...");
            }

            await Task.Delay(100, cancellationToken);
        }

        yield break;
    }

    public void Dispose()
    {
        _physicsReader.Dispose();
        _graphicsReader.Dispose();
        _staticReader.Dispose();
    }

    private void UpdateConnectionState(bool isConnected, string statusMessage)
    {
        if (_current.IsConnected == isConnected && _current.StatusMessage == statusMessage)
        {
            return;
        }

        _current = new TelemetryConnectionState
        {
            IsConnected = isConnected,
            SourceName = "Assetto Corsa Shared Memory",
            StatusMessage = statusMessage,
            TimestampUtc = DateTimeOffset.UtcNow
        };

        ConnectionStateChanged?.Invoke(this, _current);
    }
}