namespace Pace.Engineer.Core.Models;

public enum EngineerClip
{
    // Acknowledge
    AcknowledgeOk,
    AcknowledgeNo,
    RadioCheck,
    StandBy,
    DidNotUnderstand,
    NoTelemetry,

    // Fuel (primary)
    FillTheTank,
    FuelToEnd,
    FuelCriticalBoxThisLap,
    FuelWillBeTight,
    FuelShouldBeOk,
    PlentyOfFuel,
    PitForFuel,

    // Fuel (composable additions)
    OneLapRemaining,
    TwoLapsRemaining,
    ThreeLapsRemaining,
    FourLapsRemaining,
    FiveLapsRemaining,

    // Battery
    LowBattery,
    CriticalBattery,
    PlentyOfBattery,

    // Flags
    YellowFlag,
    LocalYellowAhead,
    BlueFlag,
    BlackFlag,
    ClearToOvertake,

    // Damage
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

    // Engine / telemetry
    HotOil,
    HotWater,
    HotOilAndWater,
    LowFuelPressure,
    LowOilPressure,
    Stalled,
}
