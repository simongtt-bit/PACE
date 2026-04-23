namespace Pace.Engineer.Core.Models;

public sealed class TyreSnapshot
{
    public double? TemperatureCelsius { get; init; }
    public double? PressureKpa { get; init; }
    public double? WearPercent { get; init; }
}