using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using Pace.Engineer.Core.Interfaces;
using Pace.Engineer.Core.Models;

namespace Pace.Engineer.App.ViewModels;

public sealed class MainWindowViewModel : INotifyPropertyChanged
{
    private readonly ISessionSnapshotPublisher _publisher;

    private string _status = "Waiting for telemetry...";
    private string _simulator = "-";
    private string _trackName = "-";
    private string _carName = "-";
    private string _sessionType = "-";
    private int _lapNumber;
    private int _sectorNumber;
    private string _currentLap = "-";
    private string _lastLap = "-";
    private string _bestLap = "-";
    private string _delta = "-";
    private double _speedKph;
    private int _gear;
    private double _rpm;
    private double _throttlePercent;
    private double _brakePercent;
    private double _fuelLitres;
    private double? _lapsRemaining;
    private double? _frontLeftTemp;
    private double? _frontRightTemp;
    private double? _rearLeftTemp;
    private double? _rearRightTemp;

    public MainWindowViewModel(ISessionSnapshotPublisher publisher)
    {
        _publisher = publisher;
        _publisher.SnapshotReceived += OnSnapshotReceived;
    }

    public ObservableCollection<string> Logs { get; } = [];

    public string Status { get => _status; set => SetField(ref _status, value); }
    public string Simulator { get => _simulator; set => SetField(ref _simulator, value); }
    public string TrackName { get => _trackName; set => SetField(ref _trackName, value); }
    public string CarName { get => _carName; set => SetField(ref _carName, value); }
    public string SessionType { get => _sessionType; set => SetField(ref _sessionType, value); }
    public int LapNumber { get => _lapNumber; set => SetField(ref _lapNumber, value); }
    public int SectorNumber { get => _sectorNumber; set => SetField(ref _sectorNumber, value); }
    public string CurrentLap { get => _currentLap; set => SetField(ref _currentLap, value); }
    public string LastLap { get => _lastLap; set => SetField(ref _lastLap, value); }
    public string BestLap { get => _bestLap; set => SetField(ref _bestLap, value); }
    public string Delta { get => _delta; set => SetField(ref _delta, value); }
    public double SpeedKph { get => _speedKph; set => SetField(ref _speedKph, value); }
    public int Gear { get => _gear; set => SetField(ref _gear, value); }
    public double Rpm { get => _rpm; set => SetField(ref _rpm, value); }
    public double ThrottlePercent { get => _throttlePercent; set => SetField(ref _throttlePercent, value); }
    public double BrakePercent { get => _brakePercent; set => SetField(ref _brakePercent, value); }
    public double FuelLitres { get => _fuelLitres; set => SetField(ref _fuelLitres, value); }
    public double? LapsRemaining { get => _lapsRemaining; set => SetField(ref _lapsRemaining, value); }
    public double? FrontLeftTemp { get => _frontLeftTemp; set => SetField(ref _frontLeftTemp, value); }
    public double? FrontRightTemp { get => _frontRightTemp; set => SetField(ref _frontRightTemp, value); }
    public double? RearLeftTemp { get => _rearLeftTemp; set => SetField(ref _rearLeftTemp, value); }
    public double? RearRightTemp { get => _rearRightTemp; set => SetField(ref _rearRightTemp, value); }

    public event PropertyChangedEventHandler? PropertyChanged;

    private void OnSnapshotReceived(object? sender, SessionSnapshot snapshot)
    {
        Application.Current.Dispatcher.Invoke(() =>
        {
            Status = $"Connected via {snapshot.TelemetrySource}";
            Simulator = snapshot.Simulator;
            TrackName = snapshot.TrackName;
            CarName = snapshot.CarName;
            SessionType = snapshot.SessionType;
            LapNumber = snapshot.LapNumber;
            SectorNumber = snapshot.SectorNumber;
            CurrentLap = FormatLapTime(snapshot.CurrentLapTime);
            LastLap = FormatLapTime(snapshot.LastLapTime);
            BestLap = FormatLapTime(snapshot.BestLapTime);
            Delta = FormatDelta(snapshot.DeltaToBest);
            SpeedKph = snapshot.SpeedKph;
            Gear = snapshot.Gear;
            Rpm = snapshot.Rpm;
            ThrottlePercent = snapshot.ThrottlePercent;
            BrakePercent = snapshot.BrakePercent;
            FuelLitres = snapshot.FuelLitresRemaining;
            LapsRemaining = snapshot.EstimatedLapsRemaining;
            FrontLeftTemp = snapshot.Tyres.FrontLeft.TemperatureCelsius;
            FrontRightTemp = snapshot.Tyres.FrontRight.TemperatureCelsius;
            RearLeftTemp = snapshot.Tyres.RearLeft.TemperatureCelsius;
            RearRightTemp = snapshot.Tyres.RearRight.TemperatureCelsius;

            Logs.Insert(0,
                $"{snapshot.TimestampUtc:HH:mm:ss} | Lap {snapshot.LapNumber} | S{snapshot.SectorNumber} | {snapshot.SpeedKph:F0} kph");

            while (Logs.Count > 100)
            {
                Logs.RemoveAt(Logs.Count - 1);
            }
        });
    }

    private static string FormatLapTime(TimeSpan? value)
    {
        if (value is null)
        {
            return "-";
        }

        var totalMinutes = (int)value.Value.TotalMinutes;
        var seconds = value.Value.Seconds;
        var milliseconds = value.Value.Milliseconds;

        return $"{totalMinutes:00}:{seconds:00}.{milliseconds:000}";
    }

    private static string FormatDelta(TimeSpan? value)
    {
        if (value is null)
        {
            return "-";
        }

        var sign = value.Value >= TimeSpan.Zero ? "+" : "-";
        var absolute = value.Value.Duration();

        return $"{sign}{absolute:mm\\:ss\\.fff}";
    }

    private void SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value))
        {
            return;
        }

        field = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}