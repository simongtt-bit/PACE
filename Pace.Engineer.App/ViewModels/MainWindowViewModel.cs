using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using Pace.Engineer.Analysis.Services;
using Pace.Engineer.App.Services;
using Pace.Engineer.Core.Interfaces;
using Pace.Engineer.Core.Models;

namespace Pace.Engineer.App.ViewModels;

public sealed class MainWindowViewModel : INotifyPropertyChanged
{
    private readonly ISessionSnapshotPublisher _publisher;
    private readonly ITelemetryConnectionMonitor _connectionMonitor;
    private readonly EngineerService _engineerService;
    private readonly FuelProjectionService _fuelProjectionService;
    private readonly PaceAnalysisService _paceAnalysisService;
    private readonly IEngineerAudioService _audioService;

    private SessionSnapshot? _currentSnapshot;
    private int _lastProcessedLapNumber;
    private double? _lastLapFuelAtStart;

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
    private string _engineerResponse = "PACE standing by.";
    private string _lastQuestionAsked = "-";
    private string _responseTimestamp = "-";
    private string _responseSeverity = "Info";

    public MainWindowViewModel(
        ISessionSnapshotPublisher publisher,
        ITelemetryConnectionMonitor connectionMonitor,
        EngineerService engineerService,
        FuelProjectionService fuelProjectionService,
        PaceAnalysisService paceAnalysisService,
        IEngineerAudioService audioService
    )
    {
        _publisher = publisher;
        _connectionMonitor = connectionMonitor;
        _engineerService = engineerService;
        _fuelProjectionService = fuelProjectionService;
        _paceAnalysisService = paceAnalysisService;
        _audioService = audioService;

        Status = connectionMonitor.Current.StatusMessage;

        AskFuelCommand = new RelayCommand(_ => AskQuestion(EngineerQuestionType.Fuel));
        AskTyresCommand = new RelayCommand(_ => AskQuestion(EngineerQuestionType.Tyres));
        AskPaceCommand = new RelayCommand(_ => AskQuestion(EngineerQuestionType.Pace));
        AskCompareToBestCommand = new RelayCommand(_ =>
            AskQuestion(EngineerQuestionType.CompareToBest)
        );

        TestNoTelemetryCommand = new RelayCommand(_ => TestNoTelemetry());

        TestFuelCriticalCommand = new RelayCommand(_ =>
            TestQuestionWithSnapshot(EngineerQuestionType.Fuel, CreateTestSnapshot(1.4))
        );

        TestFuelTightCommand = new RelayCommand(_ =>
            TestQuestionWithSnapshot(EngineerQuestionType.Fuel, CreateTestSnapshot(4.2))
        );

        TestFuelOkCommand = new RelayCommand(_ =>
            TestQuestionWithSnapshot(EngineerQuestionType.Fuel, CreateTestSnapshot(8.6))
        );

        TestPlentyFuelCommand = new RelayCommand(_ =>
            TestQuestionWithSnapshot(EngineerQuestionType.Fuel, CreateTestSnapshot(14.8))
        );

        _publisher.SnapshotReceived += OnSnapshotReceived;
        _connectionMonitor.ConnectionStateChanged += OnConnectionStateChanged;
    }

    public ObservableCollection<string> Logs { get; } = [];

    public string Status
    {
        get => _status;
        set => SetField(ref _status, value);
    }

    public string Simulator
    {
        get => _simulator;
        set => SetField(ref _simulator, value);
    }

    public string TrackName
    {
        get => _trackName;
        set => SetField(ref _trackName, value);
    }

    public string CarName
    {
        get => _carName;
        set => SetField(ref _carName, value);
    }

    public string SessionType
    {
        get => _sessionType;
        set => SetField(ref _sessionType, value);
    }

    public int LapNumber
    {
        get => _lapNumber;
        set => SetField(ref _lapNumber, value);
    }

    public int SectorNumber
    {
        get => _sectorNumber;
        set => SetField(ref _sectorNumber, value);
    }

    public string CurrentLap
    {
        get => _currentLap;
        set => SetField(ref _currentLap, value);
    }

    public string LastLap
    {
        get => _lastLap;
        set => SetField(ref _lastLap, value);
    }

    public string BestLap
    {
        get => _bestLap;
        set => SetField(ref _bestLap, value);
    }

    public string Delta
    {
        get => _delta;
        set => SetField(ref _delta, value);
    }

    public double SpeedKph
    {
        get => _speedKph;
        set => SetField(ref _speedKph, value);
    }

    public int Gear
    {
        get => _gear;
        set => SetField(ref _gear, value);
    }

    public double Rpm
    {
        get => _rpm;
        set => SetField(ref _rpm, value);
    }

    public double ThrottlePercent
    {
        get => _throttlePercent;
        set => SetField(ref _throttlePercent, value);
    }

    public double BrakePercent
    {
        get => _brakePercent;
        set => SetField(ref _brakePercent, value);
    }

    public double FuelLitres
    {
        get => _fuelLitres;
        set => SetField(ref _fuelLitres, value);
    }

    public double? LapsRemaining
    {
        get => _lapsRemaining;
        set => SetField(ref _lapsRemaining, value);
    }

    public double? FrontLeftTemp
    {
        get => _frontLeftTemp;
        set => SetField(ref _frontLeftTemp, value);
    }

    public double? FrontRightTemp
    {
        get => _frontRightTemp;
        set => SetField(ref _frontRightTemp, value);
    }

    public double? RearLeftTemp
    {
        get => _rearLeftTemp;
        set => SetField(ref _rearLeftTemp, value);
    }

    public double? RearRightTemp
    {
        get => _rearRightTemp;
        set => SetField(ref _rearRightTemp, value);
    }

    public string EngineerResponse
    {
        get => _engineerResponse;
        set => SetField(ref _engineerResponse, value);
    }

    public string LastQuestionAsked
    {
        get => _lastQuestionAsked;
        set => SetField(ref _lastQuestionAsked, value);
    }

    public string ResponseTimestamp
    {
        get => _responseTimestamp;
        set => SetField(ref _responseTimestamp, value);
    }

    public string ResponseSeverity
    {
        get => _responseSeverity;
        set
        {
            if (SetField(ref _responseSeverity, value))
            {
                OnPropertyChanged(nameof(ResponseBadgeText));
            }
        }
    }

    public string ResponseBadgeText => ResponseSeverity;

    public ICommand AskFuelCommand { get; }
    public ICommand AskTyresCommand { get; }
    public ICommand AskPaceCommand { get; }
    public ICommand AskCompareToBestCommand { get; }

    public ICommand TestNoTelemetryCommand { get; }
    public ICommand TestFuelCriticalCommand { get; }
    public ICommand TestFuelTightCommand { get; }
    public ICommand TestFuelOkCommand { get; }
    public ICommand TestPlentyFuelCommand { get; }

    public event PropertyChangedEventHandler? PropertyChanged;

    private async void AskQuestion(EngineerQuestionType questionType)
    {
        try
        {
            LastQuestionAsked = questionType.ToString();

            var response = _engineerService.Answer(_currentSnapshot, questionType);

            await HandleEngineerResponseAsync(questionType.ToString(), response);
        }
        catch (Exception ex)
        {
            HandleEngineerError(ex);
        }
    }

    private async void TestNoTelemetry()
    {
        try
        {
            LastQuestionAsked = "Test no telemetry";

            var response = _engineerService.Answer(null, EngineerQuestionType.Fuel);

            await HandleEngineerResponseAsync("Test no telemetry", response);
        }
        catch (Exception ex)
        {
            HandleEngineerError(ex);
        }
    }

    private async void TestQuestionWithSnapshot(
        EngineerQuestionType questionType,
        SessionSnapshot snapshot
    )
    {
        try
        {
            LastQuestionAsked = $"Test {questionType}";

            ApplySnapshotToScreen(snapshot);

            var response = _engineerService.Answer(snapshot, questionType);

            await HandleEngineerResponseAsync($"Test {questionType}", response);
        }
        catch (Exception ex)
        {
            HandleEngineerError(ex);
        }
    }

    private async Task HandleEngineerResponseAsync(string source, EngineerResponse response)
    {
        EngineerResponse = response.Message;
        ResponseSeverity = response.Severity.ToString();
        ResponseTimestamp = response.TimestampUtc.ToLocalTime().ToString("HH:mm:ss");

        Logs.Insert(0, $"{DateTime.Now:HH:mm:ss} | {source} | {response.Message}");
        TrimLogs();

        await _audioService.QueueAsync(response);
    }

    private void HandleEngineerError(Exception ex)
    {
        EngineerResponse = "Engineer response failed.";
        ResponseSeverity = "Warning";
        ResponseTimestamp = DateTime.Now.ToString("HH:mm:ss");
        Logs.Insert(0, $"{DateTime.Now:HH:mm:ss} | Error | {ex.Message}");
        TrimLogs();
    }

    private static SessionSnapshot CreateTestSnapshot(double estimatedLapsRemaining)
    {
        return new SessionSnapshot
        {
            TimestampUtc = DateTime.UtcNow,
            Simulator = "DEV",
            SessionType = "Test Session",
            TrackName = "PACE Test Track",
            CarName = "PACE Test Car",
            LapNumber = 12,
            SectorNumber = 2,
            CurrentLapTime = TimeSpan.FromSeconds(72.418),
            LastLapTime = TimeSpan.FromSeconds(91.284),
            BestLapTime = TimeSpan.FromSeconds(90.112),
            DeltaToBest = TimeSpan.FromSeconds(1.172),
            SpeedKph = 184,
            ThrottlePercent = 72,
            BrakePercent = 0,
            Gear = 5,
            Rpm = 7250,
            FuelLitresRemaining = 18.4,
            EstimatedLapsRemaining = estimatedLapsRemaining,
            Tyres = new TyreSetSnapshot()
            {
                FrontLeft = new TyreSnapshot { TemperatureCelsius = 82.4 },
                FrontRight = new TyreSnapshot { TemperatureCelsius = 84.1 },
                RearLeft = new TyreSnapshot { TemperatureCelsius = 78.9 },
                RearRight = new TyreSnapshot { TemperatureCelsius = 79.6 },
            },
            IsInPitLane = false,
            IsOnTrack = true,
            IsValidLap = true,
            TelemetrySource = "Dev harness",
        };
    }

    private void OnConnectionStateChanged(object? sender, TelemetryConnectionState state)
    {
        Application.Current.Dispatcher.Invoke(() =>
        {
            Status = state.StatusMessage;
            Logs.Insert(0, $"{state.TimestampUtc:HH:mm:ss} | {state.StatusMessage}");
            TrimLogs();
        });
    }

    private void OnSnapshotReceived(object? sender, SessionSnapshot snapshot)
    {
        Application.Current.Dispatcher.Invoke(() =>
        {
            ProcessLapTransitions(snapshot);

            _currentSnapshot ??= snapshot;

            if (_currentSnapshot is not null && _currentSnapshot.LapNumber != snapshot.LapNumber)
            {
                _currentSnapshot = snapshot;
            }
            else if (_currentSnapshot is null)
            {
                _currentSnapshot = snapshot;
            }
            else
            {
                _currentSnapshot = new SessionSnapshot
                {
                    TimestampUtc = snapshot.TimestampUtc,
                    Simulator = snapshot.Simulator,
                    SessionType = snapshot.SessionType,
                    TrackName = snapshot.TrackName,
                    CarName = snapshot.CarName,
                    LapNumber = snapshot.LapNumber,
                    SectorNumber = snapshot.SectorNumber,
                    CurrentLapTime = snapshot.CurrentLapTime,
                    LastLapTime = snapshot.LastLapTime,
                    BestLapTime = snapshot.BestLapTime,
                    DeltaToBest = snapshot.DeltaToBest,
                    SpeedKph = snapshot.SpeedKph,
                    ThrottlePercent = snapshot.ThrottlePercent,
                    BrakePercent = snapshot.BrakePercent,
                    Gear = snapshot.Gear,
                    Rpm = snapshot.Rpm,
                    FuelLitresRemaining = snapshot.FuelLitresRemaining,
                    EstimatedLapsRemaining = _currentSnapshot.EstimatedLapsRemaining,
                    Tyres = snapshot.Tyres,
                    IsInPitLane = snapshot.IsInPitLane,
                    IsOnTrack = snapshot.IsOnTrack,
                    IsValidLap = snapshot.IsValidLap,
                    TelemetrySource = snapshot.TelemetrySource,
                };
            }

            ApplySnapshotToScreen(snapshot);

            TrimLogs();
        });
    }

    private void ApplySnapshotToScreen(SessionSnapshot snapshot)
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
    }

    private void ProcessLapTransitions(SessionSnapshot snapshot)
    {
        if (_lastProcessedLapNumber == 0)
        {
            _lastProcessedLapNumber = snapshot.LapNumber;
            _lastLapFuelAtStart = snapshot.FuelLitresRemaining;
            return;
        }

        if (snapshot.LapNumber <= _lastProcessedLapNumber)
        {
            return;
        }

        double? litresUsed = null;

        if (_lastLapFuelAtStart.HasValue)
        {
            litresUsed = _lastLapFuelAtStart.Value - snapshot.FuelLitresRemaining;

            if (litresUsed > 0)
            {
                _fuelProjectionService.RecordLapConsumption(litresUsed.Value);
            }
        }

        if (snapshot.LastLapTime.HasValue)
        {
            _paceAnalysisService.RecordLap(snapshot.LastLapTime.Value);
        }

        var estimated = _fuelProjectionService.EstimateLapsRemaining(snapshot.FuelLitresRemaining);

        _currentSnapshot = new SessionSnapshot
        {
            TimestampUtc = snapshot.TimestampUtc,
            Simulator = snapshot.Simulator,
            SessionType = snapshot.SessionType,
            TrackName = snapshot.TrackName,
            CarName = snapshot.CarName,
            LapNumber = snapshot.LapNumber,
            SectorNumber = snapshot.SectorNumber,
            CurrentLapTime = snapshot.CurrentLapTime,
            LastLapTime = snapshot.LastLapTime,
            BestLapTime = snapshot.BestLapTime,
            DeltaToBest = snapshot.DeltaToBest,
            SpeedKph = snapshot.SpeedKph,
            ThrottlePercent = snapshot.ThrottlePercent,
            BrakePercent = snapshot.BrakePercent,
            Gear = snapshot.Gear,
            Rpm = snapshot.Rpm,
            FuelLitresRemaining = snapshot.FuelLitresRemaining,
            EstimatedLapsRemaining = estimated,
            Tyres = snapshot.Tyres,
            IsInPitLane = snapshot.IsInPitLane,
            IsOnTrack = snapshot.IsOnTrack,
            IsValidLap = snapshot.IsValidLap,
            TelemetrySource = snapshot.TelemetrySource,
        };

        var lapTimeText = snapshot.LastLapTime.HasValue ? FormatLapTime(snapshot.LastLapTime) : "-";

        var fuelText =
            litresUsed.HasValue && litresUsed.Value > 0
                ? $"{litresUsed.Value:F2}L used"
                : "fuel usage unavailable";

        Logs.Insert(
            0,
            $"{snapshot.TimestampUtc:HH:mm:ss} | Completed lap {snapshot.LapNumber - 1} | Lap time {lapTimeText} | {fuelText}"
        );

        TrimLogs();

        _lastProcessedLapNumber = snapshot.LapNumber;
        _lastLapFuelAtStart = snapshot.FuelLitresRemaining;
    }

    private void TrimLogs()
    {
        while (Logs.Count > 200)
        {
            Logs.RemoveAt(Logs.Count - 1);
        }
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

    private bool SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value))
        {
            return false;
        }

        field = value;
        OnPropertyChanged(propertyName);
        return true;
    }

    private void OnPropertyChanged(string? propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
