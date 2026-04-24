namespace Pace.Engineer.Core.Models;

public sealed record EngineerResponse(
    string Message,
    EngineerClip? Clip,
    EngineerAudioPriority Priority,
    EngineerResponseSeverity Severity,
    DateTime TimestampUtc
);
