namespace Pace.Engineer.Core.Models;

public sealed class SessionSnapshot
{
    public DateTimeOffset TimestampUtc { get; init; }

    public string Simulator { get; init; } = "Assetto Corsa";
    public string SessionType { get; init; } = string.Empty;
    public string TrackName { get; init; } = string.Empty;
    public string CarName { get; init; } = string.Empty;

    public int LapNumber { get; init; }
    public int SectorNumber { get; init; }

    public TimeSpan? CurrentLapTime { get; init; }
    public TimeSpan? LastLapTime { get; init; }
    public TimeSpan? BestLapTime { get; init; }
    public TimeSpan? DeltaToBest { get; init; }

    public double SpeedKph { get; init; }
    public double ThrottlePercent { get; init; }
    public double BrakePercent { get; init; }
    public int Gear { get; init; }
    public double Rpm { get; init; }

    public double FuelLitresRemaining { get; init; }
    public double? EstimatedLapsRemaining { get; init; }

    public TyreSetSnapshot Tyres { get; init; } = new();

    public bool IsInPitLane { get; init; }
    public bool IsOnTrack { get; init; }
    public bool IsValidLap { get; init; }

    public string TelemetrySource { get; init; } = string.Empty;
}