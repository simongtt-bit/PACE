using Pace.Engineer.Core.Interfaces;
using Pace.Engineer.Core.Models;

namespace Pace.Engineer.App.Services;

public sealed class EngineerClipResolver : IEngineerClipResolver
{
    public EngineerClip? Resolve(EngineerQuestionType questionType, string message)
    {
        var text = message.ToLowerInvariant();

        if (text.Contains("no telemetry available") || text.Contains("not enough data"))
        {
            return EngineerClip.StandBy;
        }

        if (text.Contains("radio check"))
        {
            return EngineerClip.RadioCheck;
        }

        if (text.Contains("fuel is critical"))
        {
            return EngineerClip.FuelCriticalBoxThisLap;
        }

        if (text.Contains("fuel is getting tight") || text.Contains("fuel is tight"))
        {
            return EngineerClip.FuelWillBeTight;
        }

        if (text.Contains("fuel looks good"))
        {
            return EngineerClip.FuelShouldBeOk;
        }

        if (text.Contains("plenty of fuel"))
        {
            return EngineerClip.PlentyOfFuel;
        }

        if (text.Contains("fill the tank"))
        {
            return EngineerClip.FillTheTank;
        }

        if (text.Contains("fuel to end"))
        {
            return EngineerClip.FuelToEnd;
        }

        if (text.Contains("yellow flag"))
        {
            return EngineerClip.YellowFlag;
        }

        if (text.Contains("local yellow"))
        {
            return EngineerClip.LocalYellowAhead;
        }

        if (text.Contains("blue flag"))
        {
            return EngineerClip.BlueFlag;
        }

        if (text.Contains("black flag"))
        {
            return EngineerClip.BlackFlag;
        }

        if (text.Contains("clear to overtake"))
        {
            return EngineerClip.ClearToOvertake;
        }

        if (text.Contains("no damage"))
        {
            return EngineerClip.NoDamage;
        }

        if (text.Contains("left front puncture"))
        {
            return EngineerClip.LeftFrontPuncture;
        }

        if (text.Contains("right front puncture"))
        {
            return EngineerClip.RightFrontPuncture;
        }

        if (text.Contains("left rear puncture"))
        {
            return EngineerClip.LeftRearPuncture;
        }

        if (text.Contains("right rear puncture"))
        {
            return EngineerClip.RightRearPuncture;
        }

        if (text.Contains("minor aero damage"))
        {
            return EngineerClip.MinorAeroDamage;
        }

        if (text.Contains("severe aero damage"))
        {
            return EngineerClip.SevereAeroDamage;
        }

        if (text.Contains("busted engine"))
        {
            return EngineerClip.BustedEngine;
        }

        if (text.Contains("busted suspension"))
        {
            return EngineerClip.BustedSuspension;
        }

        if (text.Contains("busted transmission"))
        {
            return EngineerClip.BustedTransmission;
        }

        if (text.Contains("hot oil and water"))
        {
            return EngineerClip.HotOilAndWater;
        }

        if (text.Contains("hot oil"))
        {
            return EngineerClip.HotOil;
        }

        if (text.Contains("hot water"))
        {
            return EngineerClip.HotWater;
        }

        if (text.Contains("low fuel pressure"))
        {
            return EngineerClip.LowFuelPressure;
        }

        if (text.Contains("low oil pressure"))
        {
            return EngineerClip.LowOilPressure;
        }

        if (text.Contains("stalled"))
        {
            return EngineerClip.Stalled;
        }

        return questionType switch
        {
            EngineerQuestionType.Fuel => EngineerClip.FuelShouldBeOk,
            _ => null
        };
    }
}