using Pace.Engineer.Core.Models;

namespace Pace.Engineer.Core.Interfaces;

public interface ISessionSnapshotPublisher
{
    SessionSnapshot? Current { get; }
    event EventHandler<SessionSnapshot>? SnapshotReceived;
    Task StartAsync(CancellationToken cancellationToken);
}