using Pace.Engineer.Analysis.Services;
using Pace.Engineer.Core.Models;

namespace Pace.Engineer.App.Services;

public sealed class EngineerService
{
    private readonly FuelProjectionService _fuelProjectionService;
    private readonly TyreAnalysisService _tyreAnalysisService;
    private readonly PaceAnalysisService _paceAnalysisService;

    public EngineerService(
        FuelProjectionService fuelProjectionService,
        TyreAnalysisService tyreAnalysisService,
        PaceAnalysisService paceAnalysisService)
    {
        _fuelProjectionService = fuelProjectionService;
        _tyreAnalysisService = tyreAnalysisService;
        _paceAnalysisService = paceAnalysisService;
    }

    public EngineerResponse Answer(SessionSnapshot? snapshot, EngineerQuestionType questionType)
    {
        if (snapshot is null)
        {
            return new EngineerResponse
            {
                QuestionType = questionType,
                Message = "No telemetry available yet.",
                TimestampUtc = DateTimeOffset.UtcNow
            };
        }

        var message = questionType switch
        {
            EngineerQuestionType.Fuel => BuildFuelAnswer(snapshot),
            EngineerQuestionType.Tyres => _tyreAnalysisService.BuildTyreSummary(snapshot.Tyres),
            EngineerQuestionType.Pace => _paceAnalysisService.BuildPaceSummary(snapshot.LastLapTime, snapshot.BestLapTime),
            EngineerQuestionType.CompareToBest => _paceAnalysisService.BuildBestComparison(snapshot.CurrentLapTime, snapshot.BestLapTime),
            _ => "I do not have an answer for that yet."
        };

        return new EngineerResponse
        {
            QuestionType = questionType,
            Message = message,
            TimestampUtc = DateTimeOffset.UtcNow
        };
    }

    private string BuildFuelAnswer(SessionSnapshot snapshot)
    {
        var lapsRemaining = snapshot.EstimatedLapsRemaining;

        if (lapsRemaining is null)
        {
            return "Not enough data for a fuel estimate yet.";
        }

        if (lapsRemaining < 2)
        {
            return $"Fuel is critical. Roughly {lapsRemaining.Value:F1} laps remaining.";
        }

        if (lapsRemaining < 5)
        {
            return $"Fuel is getting tight. Roughly {lapsRemaining.Value:F1} laps remaining.";
        }

        return $"Fuel is fine. Roughly {lapsRemaining.Value:F1} laps remaining.";
    }
}