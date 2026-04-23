namespace Pace.Engineer.Analysis.Services;

public sealed class FuelProjectionService
{
    private readonly Queue<double> _recentLapConsumption = new();

    public void RecordLapConsumption(double litresUsed)
    {
        if (litresUsed <= 0)
        {
            return;
        }

        _recentLapConsumption.Enqueue(litresUsed);

        while (_recentLapConsumption.Count > 10)
        {
            _recentLapConsumption.Dequeue();
        }
    }

    public double? EstimateLapsRemaining(double fuelLitresRemaining)
    {
        if (_recentLapConsumption.Count < 2)
        {
            return null;
        }

        var averageConsumption = _recentLapConsumption.Average();

        if (averageConsumption <= 0)
        {
            return null;
        }

        return fuelLitresRemaining / averageConsumption;
    }

    public double? GetAverageLapConsumption()
    {
        if (_recentLapConsumption.Count == 0)
        {
            return null;
        }

        return _recentLapConsumption.Average();
    }
}