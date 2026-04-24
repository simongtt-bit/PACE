using Pace.Engineer.Core.Interfaces;
using Pace.Engineer.Core.Models;

namespace Pace.Engineer.App.Services;

public sealed class EngineerAudioService : IEngineerAudioService, IAsyncDisposable
{
    private readonly IVoiceClipService _voiceClipService;
    private readonly PriorityQueue<AudioRequest, AudioPriorityKey> _queue = new();
    private readonly SemaphoreSlim _signal = new(0);
    private readonly object _sync = new();

    private readonly CancellationTokenSource _shutdownCts = new();

    private CancellationTokenSource? _currentPlaybackCts;
    private EngineerAudioPriority? _currentPriority;
    private long _sequence;
    private Task? _workerTask;

    public EngineerAudioService(IVoiceClipService voiceClipService)
    {
        _voiceClipService = voiceClipService;
    }

    public Task QueueAsync(
        EngineerClip clip,
        EngineerAudioPriority priority = EngineerAudioPriority.Medium,
        CancellationToken cancellationToken = default
    )
    {
        cancellationToken.ThrowIfCancellationRequested();

        EnsureWorkerStarted();

        lock (_sync)
        {
            var request = new AudioRequest(clip, priority, ++_sequence);

            _queue.Enqueue(
                request,
                new AudioPriorityKey(Priority: -(int)priority, Sequence: request.Sequence)
            );

            if (_currentPriority.HasValue && priority > _currentPriority.Value)
            {
                _currentPlaybackCts?.Cancel();
                _ = _voiceClipService.StopAsync(CancellationToken.None);
            }
        }

        _signal.Release();

        return Task.CompletedTask;
    }

    public async Task StopAsync(CancellationToken cancellationToken = default)
    {
        lock (_sync)
        {
            _currentPlaybackCts?.Cancel();
            _queue.Clear();
        }

        await _voiceClipService.StopAsync(cancellationToken);
    }

    public Task ClearQueueAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        lock (_sync)
        {
            _queue.Clear();
        }

        return Task.CompletedTask;
    }

    private void EnsureWorkerStarted()
    {
        if (_workerTask is not null)
        {
            return;
        }

        lock (_sync)
        {
            _workerTask ??= Task.Run(ProcessQueueAsync);
        }
    }

    private async Task ProcessQueueAsync()
    {
        while (!_shutdownCts.IsCancellationRequested)
        {
            try
            {
                await _signal.WaitAsync(_shutdownCts.Token);

                AudioRequest? request = null;

                lock (_sync)
                {
                    if (_queue.Count > 0)
                    {
                        request = _queue.Dequeue();
                        _currentPriority = request.Priority;
                        _currentPlaybackCts = CancellationTokenSource.CreateLinkedTokenSource(
                            _shutdownCts.Token
                        );
                    }
                }

                if (request is null)
                {
                    continue;
                }

                try
                {
                    await _voiceClipService.PlayAsync(request.Clip, _currentPlaybackCts!.Token);
                }
                catch (OperationCanceledException)
                {
                    // Expected when interrupted by higher priority audio.
                }
                finally
                {
                    lock (_sync)
                    {
                        _currentPlaybackCts?.Dispose();
                        _currentPlaybackCts = null;
                        _currentPriority = null;
                    }
                }
            }
            catch (OperationCanceledException)
            {
                break;
            }
        }
    }

    public async ValueTask DisposeAsync()
    {
        _shutdownCts.Cancel();

        await _voiceClipService.StopAsync(CancellationToken.None);

        if (_workerTask is not null)
        {
            try
            {
                await _workerTask;
            }
            catch (OperationCanceledException) { }
        }

        _signal.Dispose();
        _shutdownCts.Dispose();
    }

    private sealed record AudioRequest(
        EngineerClip Clip,
        EngineerAudioPriority Priority,
        long Sequence
    );

    private readonly record struct AudioPriorityKey(int Priority, long Sequence);
}
