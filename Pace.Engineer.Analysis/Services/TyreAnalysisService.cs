using Pace.Engineer.Core.Models;

namespace Pace.Engineer.Analysis.Services;

public sealed class TyreAnalysisService
{
    public string BuildTyreSummary(TyreSetSnapshot tyres)
    {
        var temperatures = new Dictionary<string, double?>
        {
            ["Front left"] = tyres.FrontLeft.TemperatureCelsius,
            ["Front right"] = tyres.FrontRight.TemperatureCelsius,
            ["Rear left"] = tyres.RearLeft.TemperatureCelsius,
            ["Rear right"] = tyres.RearRight.TemperatureCelsius
        };

        var validTemps = temperatures
            .Where(x => x.Value.HasValue)
            .Select(x => new KeyValuePair<string, double>(x.Key, x.Value!.Value))
            .ToList();

        if (validTemps.Count == 0)
        {
            return "No tyre data yet.";
        }

        var hottest = validTemps.OrderByDescending(x => x.Value).First();
        var coolest = validTemps.OrderBy(x => x.Value).First();
        var spread = hottest.Value - coolest.Value;
        var average = validTemps.Average(x => x.Value);

        // 🔥 Engineer-style interpretation

        if (average < 50)
        {
            return "Tyres are still coming up to temperature.";
        }

        if (spread < 3)
        {
            return $"Tyres look consistent. Hottest is {hottest.Key} at {hottest.Value:F1} degrees.";
        }

        if (spread < 10)
        {
            return $"{hottest.Key} is slightly hotter. Balance looks okay.";
        }

        // High spread = imbalance
        var bias = GetBias(hottest.Key);

        return $"{hottest.Key} is running hot at {hottest.Value:F1} degrees. You're loading the {bias} more.";
    }

    private static string GetBias(string tyre)
    {
        if (tyre.Contains("Front"))
        {
            return "front tyres";
        }

        if (tyre.Contains("Rear"))
        {
            return "rear tyres";
        }

        return "car";
    }
}