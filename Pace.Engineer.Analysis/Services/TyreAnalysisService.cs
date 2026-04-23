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
            return "No tyre temperature data available yet.";
        }

        var hottest = validTemps.OrderByDescending(x => x.Value).First();
        var coolest = validTemps.OrderBy(x => x.Value).First();
        var spread = hottest.Value - coolest.Value;

        if (spread < 3)
        {
            return $"Tyres look stable. Hottest is {hottest.Key} at {hottest.Value:F1}°C.";
        }

        return $"{hottest.Key} is hottest at {hottest.Value:F1}°C. Temperature spread is {spread:F1}°C across the car.";
    }
}