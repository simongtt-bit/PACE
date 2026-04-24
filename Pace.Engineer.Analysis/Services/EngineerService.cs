using Pace.Engineer.Core.Models;

namespace Pace.Engineer.Analysis.Services;

public sealed class EngineerService
{
    private readonly FuelProjectionService _fuelProjectionService;
    private readonly TyreAnalysisService _tyreAnalysisService;
    private readonly PaceAnalysisService _paceAnalysisService;

    public EngineerService(
        FuelProjectionService fuelProjectionService,
        TyreAnalysisService tyreAnalysisService,
        PaceAnalysisService paceAnalysisService
    )
    {
        _fuelProjectionService = fuelProjectionService;
        _tyreAnalysisService = tyreAnalysisService;
        _paceAnalysisService = paceAnalysisService;
    }

    public EngineerResponse Answer(SessionSnapshot? snapshot, EngineerQuestionType questionType)
    {
        if (snapshot is null)
        {
            return EngineerResponse.Create(
                questionType,
                "No telemetry available yet.",
                EngineerClip.NoTelemetry,
                EngineerAudioPriority.Low,
                EngineerResponseSeverity.Caution
            );
        }

        return questionType switch
        {
            EngineerQuestionType.Fuel => BuildFuelAnswer(snapshot, questionType),

            EngineerQuestionType.Tyres => EngineerResponse.Create(
                questionType,
                _tyreAnalysisService.BuildTyreSummary(snapshot.Tyres),
                EngineerClip.AcknowledgeOk,
                EngineerAudioPriority.Low,
                EngineerResponseSeverity.Info
            ),

            EngineerQuestionType.Pace => EngineerResponse.Create(
                questionType,
                _paceAnalysisService.BuildPaceSummary(snapshot.LastLapTime, snapshot.BestLapTime),
                EngineerClip.AcknowledgeOk,
                EngineerAudioPriority.Low,
                EngineerResponseSeverity.Info
            ),

            EngineerQuestionType.CompareToBest => EngineerResponse.Create(
                questionType,
                _paceAnalysisService.BuildBestComparison(
                    snapshot.LastLapTime,
                    snapshot.BestLapTime
                ),
                EngineerClip.AcknowledgeOk,
                EngineerAudioPriority.Medium,
                EngineerResponseSeverity.Info
            ),

            _ => EngineerResponse.Create(
                questionType,
                "I do not have an answer for that yet.",
                (EngineerClip?)null,
                EngineerAudioPriority.Low,
                EngineerResponseSeverity.Info
            ),
        };
    }

    private EngineerResponse BuildFuelAnswer(
        SessionSnapshot snapshot,
        EngineerQuestionType questionType
    )
    {
        var lapsRemaining = snapshot.EstimatedLapsRemaining;

        if (lapsRemaining is null)
        {
            return EngineerResponse.Create(
                questionType,
                "Not enough data for a fuel estimate yet.",
                EngineerClip.StandBy,
                EngineerAudioPriority.Low,
                EngineerResponseSeverity.Caution
            );
        }

        if (lapsRemaining < 2)
        {
            return EngineerResponse.Create(
                questionType,
                "Fuel is critical. Box this lap.",
                EngineerClip.FuelCriticalBoxThisLap,
                EngineerAudioPriority.Critical,
                EngineerResponseSeverity.Critical
            );
        }

        if (lapsRemaining < 5)
        {
            return EngineerResponse.CreateChain(
                questionType,
                $"Fuel will be tight. You’ve got about {lapsRemaining.Value:F1} laps.",
                [EngineerClip.FuelWillBeTight, ToLapRemainingClip(lapsRemaining.Value)],
                EngineerAudioPriority.High,
                EngineerResponseSeverity.Warning
            );
        }

        if (lapsRemaining < 10)
        {
            return EngineerResponse.CreateChain(
                questionType,
                $"Fuel should be okay. Around {lapsRemaining.Value:F1} laps remaining.",
                [EngineerClip.FuelShouldBeOk, ToLapRemainingClip(lapsRemaining.Value)],
                EngineerAudioPriority.Medium,
                EngineerResponseSeverity.Info
            );
        }

        return EngineerResponse.CreateChain(
            questionType,
            $"Plenty of fuel. About {lapsRemaining.Value:F1} laps remaining.",
            [EngineerClip.PlentyOfFuel, ToLapRemainingClip(lapsRemaining.Value)],
            EngineerAudioPriority.Low,
            EngineerResponseSeverity.Info
        );
    }

    private static EngineerClip ToLapRemainingClip(double lapsRemaining)
    {
        var rounded = (int)Math.Round(lapsRemaining, MidpointRounding.AwayFromZero);

        return rounded switch
        {
            <= 1 => EngineerClip.OneLapRemaining,
            2 => EngineerClip.TwoLapsRemaining,
            3 => EngineerClip.ThreeLapsRemaining,
            4 => EngineerClip.FourLapsRemaining,
            _ => EngineerClip.FiveLapsRemaining,
        };
    }
}
