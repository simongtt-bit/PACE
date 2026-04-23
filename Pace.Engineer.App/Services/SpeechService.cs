using System.IO;
using System.Security;
using Microsoft.CognitiveServices.Speech;
using Microsoft.Extensions.Options;
using NAudio.Dsp;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;

namespace Pace.Engineer.App.Services;

public sealed class SpeechService : IDisposable
{
    private readonly SpeechConfig _config;
    private readonly string _voice;
    private readonly SemaphoreSlim _lock = new(1, 1);

    private WaveOutEvent? _waveOut;
    private MemoryStream? _currentAudioStream;

    public bool IsEnabled { get; set; } = true;
    public bool RadioEffectsEnabled { get; set; } = true;

    public SpeechService(IOptions<AzureSpeechOptions> options)
    {
        var o = options.Value;

        if (string.IsNullOrWhiteSpace(o.Key) || string.IsNullOrWhiteSpace(o.Region))
        {
            throw new InvalidOperationException("Azure Speech config missing.");
        }

        _voice = string.IsNullOrWhiteSpace(o.VoiceName)
            ? "en-GB-RyanNeural"
            : o.VoiceName;

        _config = SpeechConfig.FromSubscription(o.Key, o.Region);
        _config.SpeechSynthesisVoiceName = _voice;
        _config.SetSpeechSynthesisOutputFormat(SpeechSynthesisOutputFormat.Riff16Khz16BitMonoPcm);
    }

    public async Task SpeakAsync(string text)
    {
        if (!IsEnabled || string.IsNullOrWhiteSpace(text))
        {
            return;
        }

        var cleaned = Normalise(text);
        var ssml = BuildSsml(cleaned);

        await _lock.WaitAsync();

        try
        {
            await StopAsync();

            using var synthesizer = new SpeechSynthesizer(_config);
            var result = await synthesizer.SpeakSsmlAsync(ssml);

            if (result.Reason != ResultReason.SynthesizingAudioCompleted)
            {
                var details = SpeechSynthesisCancellationDetails.FromResult(result);

                throw new Exception(
                    $"Speech failed: {details.Reason} | {details.ErrorDetails}");
            }

            await PlayAudioAsync(result.AudioData);
        }
        finally
        {
            _lock.Release();
        }
    }

    public Task StopAsync()
    {
        try
        {
            _waveOut?.Stop();
        }
        catch
        {
        }

        DisposePlayback();

        return Task.CompletedTask;
    }

    private async Task PlayAudioAsync(byte[] audioData)
    {
        DisposePlayback();

        _currentAudioStream = new MemoryStream(audioData);

        using var reader = new WaveFileReader(_currentAudioStream);
        ISampleProvider provider = reader.ToSampleProvider();

        if (RadioEffectsEnabled)
        {
            provider = new RadioEffectSampleProvider(provider);
        }

        var tcs = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);

        _waveOut = new WaveOutEvent();
        _waveOut.PlaybackStopped += (_, _) => tcs.TrySetResult(true);
        _waveOut.Init(provider);
        _waveOut.Play();

        await tcs.Task;
    }

    private static string Normalise(string text)
    {
        return text
            .Replace("°C", " degrees", StringComparison.OrdinalIgnoreCase)
            .Replace("kph", "", StringComparison.OrdinalIgnoreCase)
            .Replace("kilometers per hour", "", StringComparison.OrdinalIgnoreCase)
            .Replace("You are", "You're", StringComparison.OrdinalIgnoreCase)
            .Replace("You have", "You've", StringComparison.OrdinalIgnoreCase)
            .Replace("approximately", "about", StringComparison.OrdinalIgnoreCase)
            .Replace("roughly", "about", StringComparison.OrdinalIgnoreCase)
            .Replace("Fuel is fine.", "Fuel looks good.", StringComparison.OrdinalIgnoreCase);
    }

    private string BuildSsml(string text)
    {
        var escaped = SecurityElement.Escape(text) ?? string.Empty;

        return $"""
<speak version="1.0"
       xml:lang="en-GB"
       xmlns="http://www.w3.org/2001/10/synthesis">
  <voice name="{_voice}">
    <prosody rate="+18%" pitch="-5%">
      {escaped}
    </prosody>
  </voice>
</speak>
""";
    }

    private void DisposePlayback()
    {
        _waveOut?.Dispose();
        _waveOut = null;

        _currentAudioStream?.Dispose();
        _currentAudioStream = null;
    }

    public void Dispose()
    {
        DisposePlayback();
        _lock.Dispose();
    }

    private sealed class RadioEffectSampleProvider : ISampleProvider
    {
        private readonly ISampleProvider _source;
        private readonly int _sampleRate;
        private readonly int _channels;
        private readonly BiQuadFilter _highPassLeft;
        private readonly BiQuadFilter _lowPassLeft;
        private readonly BiQuadFilter _highPassRight;
        private readonly BiQuadFilter _lowPassRight;
        private readonly Random _random = new();

        public RadioEffectSampleProvider(ISampleProvider source)
        {
            _source = source;
            _sampleRate = source.WaveFormat.SampleRate;
            _channels = source.WaveFormat.Channels;

            _highPassLeft = BiQuadFilter.HighPassFilter(_sampleRate, 650f, 0.707f);
            _lowPassLeft = BiQuadFilter.LowPassFilter(_sampleRate, 2600f, 0.707f);

            _highPassRight = BiQuadFilter.HighPassFilter(_sampleRate, 650f, 0.707f);
            _lowPassRight = BiQuadFilter.LowPassFilter(_sampleRate, 2600f, 0.707f);
        }

        public WaveFormat WaveFormat => _source.WaveFormat;

        public int Read(float[] buffer, int offset, int count)
        {
            var samplesRead = _source.Read(buffer, offset, count);

            for (var n = 0; n < samplesRead; n += _channels)
            {
                for (var ch = 0; ch < _channels; ch++)
                {
                    var index = offset + n + ch;
                    var sample = buffer[index];

                    sample *= 1.8f;

                    sample = ch == 0
                        ? _lowPassLeft.Transform(_highPassLeft.Transform(sample))
                        : _lowPassRight.Transform(_highPassRight.Transform(sample));

                    sample = MathF.Tanh(sample * 1.6f) * 0.75f;

                    var staticNoise = ((float)_random.NextDouble() * 2f - 1f) * 0.008f;
                    sample += staticNoise;

                    if (MathF.Abs(sample) < 0.015f)
                    {
                        sample *= 0.65f;
                    }

                    sample = Math.Clamp(sample, -1f, 1f);
                    buffer[index] = sample;
                }
            }

            return samplesRead;
        }
    }
}