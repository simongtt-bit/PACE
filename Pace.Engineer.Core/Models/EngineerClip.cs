namespace Pace.Engineer.Core.Models;

public enum EngineerClip
{
    AcknowledgeOk,
    AcknowledgeNo,
    RadioCheck,
    StandBy,
    DidNotUnderstand,
    NoTelemetry,

    FillTheTank,
    FuelToEnd,

    LowBattery,
    CriticalBattery,
    PlentyOfBattery,

    FuelCriticalBoxThisLap,
    FuelWillBeTight,
    FuelShouldBeOk,
    PlentyOfFuel,
    PitForFuel,

    YellowFlag,
    LocalYellowAhead,
    BlueFlag,
    BlackFlag,
    ClearToOvertake,

    NoDamage,
    LeftFrontPuncture,
    RightFrontPuncture,
    LeftRearPuncture,
    RightRearPuncture,
    MinorAeroDamage,
    SevereAeroDamage,
    BustedEngine,
    BustedSuspension,
    BustedTransmission,

    HotOil,
    HotWater,
    HotOilAndWater,
    LowFuelPressure,
    LowOilPressure,
    Stalled,
}
