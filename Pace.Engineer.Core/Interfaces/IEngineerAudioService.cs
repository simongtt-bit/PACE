using Pace.Engineer.Core.Models;

namespace Pace.Engineer.Core.Interfaces;

public interface IEngineerAudioService
{
    Task QueueAsync(
        EngineerClip clip,
        EngineerAudioPriority priority = EngineerAudioPriority.Medium,
        CancellationToken cancellationToken = default
    );

    Task QueueAsync(EngineerResponse response, CancellationToken cancellationToken = default);

    Task StopAsync(CancellationToken cancellationToken = default);

    Task ClearQueueAsync(CancellationToken cancellationToken = default);
}
