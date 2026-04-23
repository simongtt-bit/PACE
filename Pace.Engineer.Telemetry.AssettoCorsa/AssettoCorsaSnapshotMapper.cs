using Pace.Engineer.Core.Models;
using Pace.Engineer.Telemetry.AssettoCorsa.SharedMemory;

namespace Pace.Engineer.Telemetry.AssettoCorsa;

internal static class AssettoCorsaSnapshotMapper
{
    public static SessionSnapshot Map(
        PhysicsPageFile physics,
        GraphicsPageFile graphics,
        StaticPageFile statics)
    {
        return new SessionSnapshot
        {
            TimestampUtc = DateTimeOffset.UtcNow,
            Simulator = "Assetto Corsa",
            SessionType = MapSessionType(graphics.Session),
            TrackName = BuildTrackName(statics.Track, statics.TrackConfiguration),
            CarName = statics.CarModel?.Trim() ?? string.Empty,
            LapNumber = Math.Max(0, graphics.CompletedLaps + 1),
            SectorNumber = Math.Max(1, graphics.CurrentSectorIndex + 1),
            CurrentLapTime = ParseAcTime(graphics.iCurrentTime, graphics.CurrentTime),
            LastLapTime = ParseAcTime(graphics.iLastTime, graphics.LastTime),
            BestLapTime = ParseAcTime(graphics.iBestTime, graphics.BestTime),
            DeltaToBest = BuildDelta(graphics.iCurrentTime, graphics.iBestTime),
            SpeedKph = physics.SpeedKmh,
            ThrottlePercent = physics.Gas * 100d,
            BrakePercent = physics.Brake * 100d,
            Gear = NormalizeGear(physics.Gear),
            Rpm = physics.Rpms,
            FuelLitresRemaining = physics.Fuel,
            EstimatedLapsRemaining = null,
            IsInPitLane = graphics.IsInPit != 0,
            IsOnTrack = graphics.Status is AcStatus.Live or AcStatus.Pause,
            IsValidLap = true,
            TelemetrySource = "Assetto Corsa Shared Memory",
            Tyres = new TyreSetSnapshot
            {
                FrontLeft = BuildTyre(physics, 0),
                FrontRight = BuildTyre(physics, 1),
                RearLeft = BuildTyre(physics, 2),
                RearRight = BuildTyre(physics, 3)
            }
        };
    }

    private static TyreSnapshot BuildTyre(PhysicsPageFile physics, int index)
    {
        return new TyreSnapshot
        {
            TemperatureCelsius = GetTyreTemperature(physics, index),
            PressureKpa = GetArrayValue(physics.WheelsPressure, index) is float bar
                ? bar * 100d
                : null,
            WearPercent = GetArrayValue(physics.TyreWear, index) is float wear
                ? wear * 100d
                : null
        };
    }

    private static double? GetTyreTemperature(PhysicsPageFile physics, int index)
    {
        var core = GetArrayValue(physics.TyreCoreTemperature, index);
        if (core is > 0)
        {
            return core.Value;
        }

        var i = GetArrayValue(physics.TyreTempI, index);
        var m = GetArrayValue(physics.TyreTempM, index);
        var o = GetArrayValue(physics.TyreTempO, index);

        var values = new[] { i, m, o }.Where(v => v is > 0).Select(v => v!.Value).ToArray();
        if (values.Length == 0)
        {
            return null;
        }

        return values.Average();
    }

    private static float? GetArrayValue(float[]? values, int index)
    {
        if (values is null || values.Length <= index)
        {
            return null;
        }

        return values[index];
    }

    private static string BuildTrackName(string? track, string? configuration)
    {
        var baseTrack = (track ?? string.Empty).Trim();
        var config = (configuration ?? string.Empty).Trim();

        if (string.IsNullOrWhiteSpace(config))
        {
            return baseTrack;
        }

        return $"{baseTrack} - {config}";
    }

    private static string MapSessionType(AcSessionType sessionType) =>
        sessionType switch
        {
            AcSessionType.Practice => "Practice",
            AcSessionType.Qualify => "Qualifying",
            AcSessionType.Race => "Race",
            AcSessionType.Hotlap => "Hotlap",
            AcSessionType.TimeAttack => "Time Attack",
            AcSessionType.Drift => "Drift",
            AcSessionType.Drag => "Drag",
            _ => "Unknown"
        };

    private static int NormalizeGear(int rawGear)
    {
        return rawGear switch
        {
            < 0 => -1,
            0 => 0,
            _ => rawGear - 1
        };
    }

    private static TimeSpan? ParseAcTime(int millisecondsValue, string? fallbackText)
    {
        if (millisecondsValue > 0)
        {
            return TimeSpan.FromMilliseconds(millisecondsValue);
        }

        if (string.IsNullOrWhiteSpace(fallbackText))
        {
            return null;
        }

        var trimmed = fallbackText.Trim();
        if (trimmed is "-" or "--:--:---" or "00:00:000")
        {
            return null;
        }

        var normalized = trimmed.Replace(':', '.');
        var parts = trimmed.Split(':', StringSplitOptions.RemoveEmptyEntries);

        if (parts.Length == 3 &&
            int.TryParse(parts[0], out var minutes) &&
            int.TryParse(parts[1], out var seconds) &&
            int.TryParse(parts[2], out var millis))
        {
            return new TimeSpan(0, 0, minutes, seconds, millis);
        }

        return null;
    }

    private static TimeSpan? BuildDelta(int currentMs, int bestMs)
    {
        if (currentMs <= 0 || bestMs <= 0)
        {
            return null;
        }

        return TimeSpan.FromMilliseconds(currentMs - bestMs);
    }
}