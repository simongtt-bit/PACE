using Pace.Engineer.Core.Models;

namespace Pace.Engineer.Core.Interfaces;

public interface ILiveTelemetrySource
{
    IAsyncEnumerable<SessionSnapshot> StreamAsync(CancellationToken cancellationToken);
}