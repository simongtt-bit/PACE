using System.Security;
using Microsoft.CognitiveServices.Speech;
using Microsoft.Extensions.Options;

namespace Pace.Engineer.App.Services;

public sealed class SpeechService : IDisposable
{
    private readonly SpeechConfig _config;
    private readonly string _voice;
    private readonly SemaphoreSlim _lock = new(1, 1);
    private SpeechSynthesizer? _synth;

    public bool IsEnabled { get; set; } = true;

    public SpeechService(IOptions<AzureSpeechOptions> options)
    {
        var o = options.Value;

        if (string.IsNullOrWhiteSpace(o.Key) || string.IsNullOrWhiteSpace(o.Region))
        {
            throw new InvalidOperationException("Azure Speech config missing.");
        }

        _voice = o.VoiceName;

        _config = SpeechConfig.FromSubscription(o.Key, o.Region);
        _config.SpeechSynthesisVoiceName = _voice;
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

            _synth = new SpeechSynthesizer(_config);

            var result = await _synth.SpeakSsmlAsync(ssml);

            if (result.Reason != ResultReason.SynthesizingAudioCompleted)
            {
                var details = SpeechSynthesisCancellationDetails.FromResult(result);

                throw new Exception(
                    $"Speech failed: {details.Reason} | {details.ErrorDetails}");
            }
        }
        finally
        {
            _synth?.Dispose();
            _synth = null;
            _lock.Release();
        }
    }

    public async Task StopAsync()
    {
        if (_synth != null)
        {
            try
            {
                await _synth.StopSpeakingAsync();
            }
            catch { }
        }
    }

    private static string Normalise(string text)
    {
        return text
            .Replace("°C", " degrees")
            .Replace("kph", " kilometers per hour")
            .Replace("You are", "You're");
    }

    private string BuildSsml(string text)
    {
        var escaped = SecurityElement.Escape(text);

        return $"""
<speak version="1.0" xml:lang="en-GB">
  <voice name="{_voice}">
    <prosody rate="-10%" pitch="-2%">
      {escaped}
    </prosody>
  </voice>
</speak>
""";
    }

    public void Dispose()
    {
        _synth?.Dispose();
        _lock.Dispose();
    }
}