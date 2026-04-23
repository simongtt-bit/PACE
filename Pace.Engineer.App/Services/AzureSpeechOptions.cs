namespace Pace.Engineer.App.Services;

public sealed class AzureSpeechOptions
{
    public string Key { get; set; } = string.Empty;
    public string Region { get; set; } = string.Empty;
    public string VoiceName { get; set; } = "en-GB-RyanNeural";
}