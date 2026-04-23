namespace Pace.Engineer.Core.Models;

public sealed class EngineerResponse
{
    public EngineerQuestionType QuestionType { get; init; }
    public string Message { get; init; } = string.Empty;
    public DateTimeOffset TimestampUtc { get; init; }
}