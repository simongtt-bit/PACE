using Pace.Engineer.Core.Interfaces;
using Pace.Engineer.Core.Models;

namespace Pace.Engineer.Telemetry.AssettoCorsa;

public sealed class AssettoCorsaTelemetrySource : ILiveTelemetrySource
{
    public async IAsyncEnumerable<SessionSnapshot> StreamAsync(CancellationToken cancellationToken)
    {
        await Task.CompletedTask;
        yield break;
    }
}