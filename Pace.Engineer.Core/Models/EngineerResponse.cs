namespace Pace.Engineer.Core.Models;

public sealed record EngineerResponse(
    EngineerQuestionType QuestionType,
    string Message,
    IReadOnlyList<EngineerClip> Clips,
    EngineerAudioPriority Priority,
    EngineerResponseSeverity Severity,
    DateTime TimestampUtc
)
{
    public EngineerClip? Clip => Clips.Count == 1 ? Clips[0] : null;

    public static EngineerResponse Create(
        EngineerQuestionType questionType,
        string message,
        EngineerClip? clip,
        EngineerAudioPriority priority,
        EngineerResponseSeverity severity
    )
    {
        return new EngineerResponse(
            questionType,
            message,
            clip is null ? Array.Empty<EngineerClip>() : [clip.Value],
            priority,
            severity,
            DateTime.UtcNow
        );
    }

    public static EngineerResponse CreateChain(
        EngineerQuestionType questionType,
        string message,
        IReadOnlyList<EngineerClip> clips,
        EngineerAudioPriority priority,
        EngineerResponseSeverity severity
    )
    {
        return new EngineerResponse(
            questionType,
            message,
            clips,
            priority,
            severity,
            DateTime.UtcNow
        );
    }
}
