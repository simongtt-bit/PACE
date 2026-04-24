using Pace.Engineer.Core.Models;

namespace Pace.Engineer.Core.Interfaces;

public interface IVoiceClipService
{
    Task PlayAsync(EngineerClip clip, CancellationToken cancellationToken = default);

    Task<bool> TryPlayAsync(EngineerClip clip, CancellationToken cancellationToken = default);

    bool HasClip(EngineerClip clip);

    Task StopAsync(CancellationToken cancellationToken = default);
}
