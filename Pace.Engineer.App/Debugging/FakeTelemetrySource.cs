using System.Runtime.CompilerServices;
using Pace.Engineer.Core.Interfaces;
using Pace.Engineer.Core.Models;

namespace Pace.Engineer.App.Debugging;

public sealed class FakeTelemetrySource : ILiveTelemetrySource
{
    public async IAsyncEnumerable<SessionSnapshot> StreamAsync(
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        var lapNumber = 12;
        var lapStartTime = DateTimeOffset.UtcNow;

        while (!cancellationToken.IsCancellationRequested)
        {
            var now = DateTimeOffset.UtcNow;
            var elapsed = now - lapStartTime;
            var lapSeconds = elapsed.TotalSeconds;

            if (lapSeconds >= 90)
            {
                lapNumber++;
                lapStartTime = now;
                elapsed = TimeSpan.Zero;
                lapSeconds = 0;
            }

            var sectorNumber = lapSeconds switch
            {
                < 30 => 1,
                < 60 => 2,
                _ => 3
            };

            var speed = 210 + (Math.Sin(lapSeconds * 0.35) * 55);
            var throttle = Math.Clamp(70 + (Math.Sin(lapSeconds * 0.8) * 30), 0, 100);
            var brake = Math.Clamp(Math.Max(0, Math.Sin(lapSeconds * 0.55) * 100), 0, 100);
            var gear = speed switch
            {
                < 40 => 1,
                < 75 => 2,
                < 110 => 3,
                < 150 => 4,
                < 190 => 5,
                _ => 6
            };

            var rpm = 4200 + (speed * 22);

            yield return new SessionSnapshot
            {
                TimestampUtc = now,
                Simulator = "Assetto Corsa",
                SessionType = "Practice",
                TrackName = "Monza",
                CarName = "Ferrari 488 GT3",
                LapNumber = lapNumber,
                SectorNumber = sectorNumber,
                CurrentLapTime = elapsed,
                LastLapTime = TimeSpan.FromSeconds(89.412),
                BestLapTime = TimeSpan.FromSeconds(88.971),
                DeltaToBest = elapsed - TimeSpan.FromSeconds(88.971),
                SpeedKph = speed,
                ThrottlePercent = throttle,
                BrakePercent = brake,
                Gear = gear,
                Rpm = rpm,
                FuelLitresRemaining = Math.Max(0, 34.8 - ((lapNumber - 12) * 2.15) - (lapSeconds / 90d * 2.15)),
                EstimatedLapsRemaining = 7.8 - ((lapNumber - 12) * 0.1),
                IsInPitLane = false,
                IsOnTrack = true,
                IsValidLap = true,
                TelemetrySource = "Fake Telemetry",
                Tyres = new TyreSetSnapshot
                {
                    FrontLeft = new TyreSnapshot
                    {
                        TemperatureCelsius = 92 + (Math.Sin(lapSeconds * 0.4) * 3)
                    },
                    FrontRight = new TyreSnapshot
                    {
                        TemperatureCelsius = 89 + (Math.Sin(lapSeconds * 0.45) * 3)
                    },
                    RearLeft = new TyreSnapshot
                    {
                        TemperatureCelsius = 87 + (Math.Sin(lapSeconds * 0.42) * 2)
                    },
                    RearRight = new TyreSnapshot
                    {
                        TemperatureCelsius = 88 + (Math.Sin(lapSeconds * 0.38) * 2)
                    }
                }
            };

            await Task.Delay(200, cancellationToken);
        }
    }
}