using Pace.Engineer.Core.Models;

namespace Pace.Engineer.App.Services;

public static class EngineerClipMappings
{
    public static string ToFolderKey(this EngineerClip clip)
    {
        var key = clip switch
        {
            // --------------------------------------
            // Acknowledge / System
            // --------------------------------------
            EngineerClip.AcknowledgeOk => "acknowledge/ok",
            EngineerClip.AcknowledgeNo => "acknowledge/no",
            EngineerClip.RadioCheck => "acknowledge/radio_check",
            EngineerClip.StandBy => "acknowledge/stand_by",
            EngineerClip.DidNotUnderstand => "acknowledge/didnt_understand",
            EngineerClip.NoTelemetry => "acknowledge/no_data",

            // --------------------------------------
            // Fuel (basic)
            // --------------------------------------
            EngineerClip.FillTheTank => "acknowledge/fill_the_tank",
            EngineerClip.FuelToEnd => "acknowledge/fuel_to_end",

            // --------------------------------------
            // Lap remaining fragments
            // --------------------------------------
            EngineerClip.OneLapRemaining => "fuel/one_lap_fuel",
            EngineerClip.TwoLapsRemaining => "fuel/two_laps_fuel",
            EngineerClip.ThreeLapsRemaining => "fuel/three_laps_fuel",
            EngineerClip.FourLapsRemaining => "fuel/four_laps_fuel",
            EngineerClip.FiveLapsRemaining => "fuel/five_laps_fuel",

            // --------------------------------------
            // Battery
            // --------------------------------------
            EngineerClip.LowBattery => "battery/low_battery",
            EngineerClip.CriticalBattery => "battery/critical_battery",
            EngineerClip.PlentyOfBattery => "battery/plenty_of_battery",

            // --------------------------------------
            // Fuel (race engineer logic)
            // --------------------------------------
            EngineerClip.FuelWillBeTight => "fuel/fuel_will_be_tight",
            EngineerClip.FuelShouldBeOk => "fuel/fuel_should_be_ok",
            EngineerClip.PlentyOfFuel => "fuel/plenty_of_fuel",
            EngineerClip.FuelCriticalBoxThisLap => "fuel/we_will_need_to_pit_for_fuel",

            // --------------------------------------
            // Flags
            // --------------------------------------
            EngineerClip.YellowFlag => "flags/yellow_flag",
            EngineerClip.LocalYellowAhead => "flags/local_yellow_ahead",
            EngineerClip.BlueFlag => "flags/blue_flag",
            EngineerClip.BlackFlag => "flags/black_flag",
            EngineerClip.ClearToOvertake => "flags/clear_to_overtake",

            // --------------------------------------
            // Damage
            // --------------------------------------
            EngineerClip.NoDamage => "damage_reporting/no_damage",
            EngineerClip.LeftFrontPuncture => "damage_reporting/left_front_puncture",
            EngineerClip.RightFrontPuncture => "damage_reporting/right_front_puncture",
            EngineerClip.LeftRearPuncture => "damage_reporting/left_rear_puncture",
            EngineerClip.RightRearPuncture => "damage_reporting/right_rear_puncture",
            EngineerClip.MinorAeroDamage => "damage_reporting/minor_aero_damage",
            EngineerClip.SevereAeroDamage => "damage_reporting/severe_aero_damage",
            EngineerClip.BustedEngine => "damage_reporting/busted_engine",
            EngineerClip.BustedSuspension => "damage_reporting/busted_suspension",
            EngineerClip.BustedTransmission => "damage_reporting/busted_transmission",

            // --------------------------------------
            // Engine
            // --------------------------------------
            EngineerClip.HotOil => "engine_monitor/hot_oil",
            EngineerClip.HotWater => "engine_monitor/hot_water",
            EngineerClip.HotOilAndWater => "engine_monitor/hot_oil_and_water",
            EngineerClip.LowFuelPressure => "engine_monitor/low_fuel_pressure",
            EngineerClip.LowOilPressure => "engine_monitor/low_oil_pressure",
            EngineerClip.Stalled => "engine_monitor/stalled",

            // --------------------------------------
            // Fallback (IMPORTANT)
            // --------------------------------------
            _ => throw new ArgumentOutOfRangeException(
                nameof(clip),
                clip,
                $"No folder mapping exists for engineer clip '{clip}'."
            ),
        };

#if DEBUG
        Console.WriteLine($"[VoiceMap] {clip} -> {key}");
#endif

        return key;
    }
}
