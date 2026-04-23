using Pace.Engineer.Core.Models;

namespace Pace.Engineer.Analysis.Services;

public sealed class PaceAnalysisService
{
    private readonly Queue<TimeSpan> _recentLaps = new();

    public void RecordLap(TimeSpan lapTime)
    {
        if (lapTime <= TimeSpan.Zero)
        {
            return;
        }

        _recentLaps.Enqueue(lapTime);

        while (_recentLaps.Count > 5)
        {
            _recentLaps.Dequeue();
        }
    }

    public string BuildPaceSummary(TimeSpan? lastLap, TimeSpan? bestLap)
    {
        if (lastLap is null && _recentLaps.Count == 0)
        {
            return "Not enough lap data yet.";
        }

        if (lastLap is not null && bestLap is not null)
        {
            var delta = lastLap.Value - bestLap.Value;

            if (delta <= TimeSpan.FromMilliseconds(100))
            {
                return "You are very close to your best lap.";
            }

            if (delta < TimeSpan.FromSeconds(0.5))
            {
                return $"You are {delta.TotalSeconds:F2}s off your best lap.";
            }

            return $"Pace is off by {delta.TotalSeconds:F2}s compared to your best lap.";
        }

        if (_recentLaps.Count >= 2)
        {
            var first = _recentLaps.First();
            var last = _recentLaps.Last();

            if (last < first)
            {
                return "You are improving over the recent laps.";
            }

            if (last > first)
            {
                return "Recent pace is slipping slightly.";
            }
        }

        return "Pace looks steady.";
    }

    public string BuildBestComparison(TimeSpan? lastLap, TimeSpan? bestLap)
    {
        if (lastLap is null || bestLap is null)
        {
            return "I do not have enough completed lap data to compare against your best yet.";
        }

        var delta = lastLap.Value - bestLap.Value;

        if (Math.Abs(delta.TotalMilliseconds) < 100)
        {
            return "Your last lap matched your best lap almost exactly.";
        }

        if (delta < TimeSpan.Zero)
        {
            return $"Your last lap was {Math.Abs(delta.TotalSeconds):F2}s faster than your previous best.";
        }

        return $"Your last lap was {delta.TotalSeconds:F2}s slower than your best lap.";
    }
}