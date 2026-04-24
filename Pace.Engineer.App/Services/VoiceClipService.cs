using System.IO;
using NAudio.Wave;
using Pace.Engineer.Core.Models;

namespace Pace.Engineer.App.Services;

public sealed class VoiceClipService : IVoiceClipService, IDisposable
{
    private readonly string _voiceRoot;
    private readonly SemaphoreSlim _lock = new(1, 1);
    private readonly Random _random = new();
    private readonly Dictionary<string, List<string>> _clips = new(StringComparer.OrdinalIgnoreCase);

    private WaveOutEvent? _waveOut;
    private AudioFileReader? _audioReader;
    private TaskCompletionSource<bool>? _playbackCompletionSource;

    public VoiceClipService()
    {
        _voiceRoot = Path.Combine(AppContext.BaseDirectory, "Assets", "Voice", "voice");
        LoadLibrary();
    }

    public async Task PlayAsync(EngineerClip clip, CancellationToken cancellationToken = default)
    {
        var key = clip.ToFolderKey();
        var found = await TryPlayAsync(clip, cancellationToken);

        if (!found)
        {
            throw new InvalidOperationException(
                $"No clip files found for '{clip}' ({key}). Voice root: {_voiceRoot}");
        }
    }

    public async Task<bool> TryPlayAsync(EngineerClip clip, CancellationToken cancellationToken = default)
    {
        var key = clip.ToFolderKey();

        await _lock.WaitAsync(cancellationToken);

        try
        {
            if (!_clips.TryGetValue(key, out var files) || files.Count == 0)
            {
                return false;
            }

            var file = files[_random.Next(files.Count)];

            await StopInternalAsync();

            _audioReader = new AudioFileReader(file);

            _playbackCompletionSource = new TaskCompletionSource<bool>(
                TaskCreationOptions.RunContinuationsAsynchronously);

            _waveOut = new WaveOutEvent
            {
                DesiredLatency = 80,
                NumberOfBuffers = 2
            };

            _waveOut.PlaybackStopped += OnPlaybackStopped;
            _waveOut.Init(_audioReader);
            _waveOut.Play();

            await _playbackCompletionSource.Task.WaitAsync(cancellationToken);
            return true;
        }
        finally
        {
            _lock.Release();
        }
    }

    public bool HasClip(EngineerClip clip)
    {
        var key = clip.ToFolderKey();
        return _clips.TryGetValue(key, out var files) && files.Count > 0;
    }

    public async Task StopAsync()
    {
        await _lock.WaitAsync();

        try
        {
            await StopInternalAsync();
        }
        finally
        {
            _lock.Release();
        }
    }

    private Task StopInternalAsync()
    {
        try
        {
            if (_waveOut is not null)
            {
                _waveOut.PlaybackStopped -= OnPlaybackStopped;
                _waveOut.Stop();
            }
        }
        catch
        {
        }

        DisposePlayback();
        return Task.CompletedTask;
    }

    private void OnPlaybackStopped(object? sender, StoppedEventArgs e)
    {
        _playbackCompletionSource?.TrySetResult(true);
    }

    private void LoadLibrary()
    {
        _clips.Clear();

        if (!Directory.Exists(_voiceRoot))
        {
            Console.WriteLine($"[Voice] Root not found: {_voiceRoot}");
            return;
        }

        Console.WriteLine($"[Voice] Loading clips from: {_voiceRoot}");

        var totalFiles = 0;

        foreach (var directory in Directory.EnumerateDirectories(_voiceRoot, "*", SearchOption.AllDirectories))
        {
            var wavFiles = Directory
                .EnumerateFiles(directory, "*.wav", SearchOption.TopDirectoryOnly)
                .OrderBy(x => x, StringComparer.OrdinalIgnoreCase)
                .ToList();

            if (wavFiles.Count == 0)
            {
                continue;
            }

            var relativePath = Path.GetRelativePath(_voiceRoot, directory)
                .Replace('\\', '/')
                .Trim()
                .ToLowerInvariant();

            if (_clips.TryGetValue(relativePath, out var existingFiles))
            {
                existingFiles.AddRange(wavFiles);
            }
            else
            {
                _clips[relativePath] = wavFiles;
            }

            totalFiles += wavFiles.Count;

            Console.WriteLine($"[Voice] Loaded: {relativePath} ({wavFiles.Count} files)");
        }

        Console.WriteLine($"[Voice] Total clips loaded: {_clips.Count} folders, {totalFiles} files");

        var sampleKeys = _clips.Keys
            .OrderBy(x => x)
            .Take(25)
            .ToList();

        Console.WriteLine("[Voice] Sample keys:");
        foreach (var key in sampleKeys)
        {
            Console.WriteLine($"    {key}");
        }
    }

    private void DisposePlayback()
    {
        if (_waveOut is not null)
        {
            _waveOut.PlaybackStopped -= OnPlaybackStopped;
            _waveOut.Dispose();
            _waveOut = null;
        }

        _audioReader?.Dispose();
        _audioReader = null;
        _playbackCompletionSource = null;
    }

    public void Dispose()
    {
        DisposePlayback();
        _lock.Dispose();
    }
}