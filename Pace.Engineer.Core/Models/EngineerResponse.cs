namespace Pace.Engineer.Core.Models;

public sealed class EngineerResponse
{
    public required EngineerQuestionType QuestionType { get; init; }

    public required string Message { get; init; }

    public EngineerClip? Clip { get; init; }

    public EngineerAudioPriority Priority { get; init; } = EngineerAudioPriority.Medium;

    public EngineerResponseSeverity Severity { get; init; } = EngineerResponseSeverity.Info;

    public DateTimeOffset TimestampUtc { get; init; } = DateTimeOffset.UtcNow;

    public static EngineerResponse Create(
        EngineerQuestionType questionType,
        string message,
        EngineerClip? clip,
        EngineerAudioPriority priority,
        EngineerResponseSeverity severity
    )
    {
        return new EngineerResponse
        {
            QuestionType = questionType,
            Message = message,
            Clip = clip,
            Priority = priority,
            Severity = severity,
            TimestampUtc = DateTimeOffset.UtcNow,
        };
    }
}
