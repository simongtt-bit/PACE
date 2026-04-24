using Pace.Engineer.Core.Models;

namespace Pace.Engineer.Core.Interfaces;

public interface IEngineerClipResolver
{
    EngineerClip? Resolve(EngineerQuestionType questionType, string message);
}