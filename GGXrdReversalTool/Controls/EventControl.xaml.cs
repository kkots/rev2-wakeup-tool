using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using GGXrdReversalTool.Library.Scenarios.Event;
using GGXrdReversalTool.Library.Scenarios.Event.Implementations;

namespace GGXrdReversalTool.Controls;

public sealed partial class EventControl
{
    public EventControl()
    {
        InitializeComponent();
    }

    public IEnumerable<ScenarioEventTypes> ActionTypes => Enum.GetValues<ScenarioEventTypes>();

    private ScenarioEventTypes? _selectedScenarioEvent;
    public ScenarioEventTypes? SelectedScenarioEvent
    {
        get => _selectedScenarioEvent;
        set
        {
            if (value == _selectedScenarioEvent) return;
            
            _selectedScenarioEvent = value;
            
            OnPropertyChanged();
            
            CreateScenario();
        }
    }

    private int _minComboCount = 1;
    public int MinComboCount
    {
        get => _minComboCount;
        set
        {
            var coercedValue = Math.Clamp(value, 1, MaxComboCount);
            if (coercedValue == _minComboCount) return;
            _minComboCount = coercedValue;
            OnPropertyChanged();
            CreateScenario();
        }
    }

    
    private int _maxComboCount = 100;
    public int MaxComboCount
    {
        get => _maxComboCount;
        set
        {
            var coercedValue = Math.Max(value, MinComboCount);
            if (coercedValue == _maxComboCount) return;
            _maxComboCount = coercedValue;
            OnPropertyChanged();
            CreateScenario();
        }
    }

    private int _minDelayAirRecoveryDelay = 5;
    public int MinDelayAirRecoveryDelay
    {
        get => _minDelayAirRecoveryDelay;
        set
        {
            var coercedValue = Math.Clamp(value, 0, MaxDelayAirRecoveryDelay);
            if (coercedValue == _minDelayAirRecoveryDelay) return;
            _minDelayAirRecoveryDelay = coercedValue;
            OnPropertyChanged();
            CreateScenario();
        }
    }

    
    private int _maxDelayAirRecoveryDelay = 20;
    public int MaxDelayAirRecoveryDelay
    {
        get => _maxDelayAirRecoveryDelay;
        set
        {
            var coercedValue = Math.Max(value, MinDelayAirRecoveryDelay);
            if (coercedValue == _maxDelayAirRecoveryDelay) return;
            _maxDelayAirRecoveryDelay = coercedValue;
            OnPropertyChanged();
            CreateScenario();
        }
    }

    public IEnumerable<AirRecoveryTypes> AirRecoveryTypesList => Enum.GetValues<AirRecoveryTypes>();
    public AirRecoveryTypes _selectedAirRecoveryType = AirRecoveryTypes.Forward;
    public AirRecoveryTypes SelectedAirRecoveryType
    {
        get => _selectedAirRecoveryType;
        set
        {
            if (value == _selectedAirRecoveryType) return;
            _selectedAirRecoveryType = value;
            OnPropertyChanged();
            CreateScenario();
        }
    }
    

    private bool _shouldCheckWakingUp = true;
    public bool ShouldCheckWakingUp
    {
        get => _shouldCheckWakingUp;
        set
        {
            if (value == _shouldCheckWakingUp) return;
            _shouldCheckWakingUp = value;
            OnPropertyChanged();
            CreateScenario();
        }
    }
    
    
    private bool _shouldCheckWallSplat;
    public bool ShouldCheckWallSplat
    {
        get => _shouldCheckWallSplat;
        set
        {
            if (value == _shouldCheckWallSplat) return;
            _shouldCheckWallSplat = value;
            OnPropertyChanged();
            CreateScenario();
        }
    }

    private bool _shouldCheckAirTech;
    public bool ShouldCheckAirTech
    {
        get => _shouldCheckAirTech;
        set
        {
            if (value == _shouldCheckAirTech) return;
            _shouldCheckAirTech = value;
            OnPropertyChanged();
            CreateScenario();
        }
    }

    private bool _shouldCheckStartBlocking;
    

    public bool ShouldCheckStartBlocking
    {
        get => _shouldCheckStartBlocking;
        set
        {
            if (value == _shouldCheckStartBlocking) return;
            _shouldCheckStartBlocking = value;
            OnPropertyChanged();
            CreateScenario();
        }
    }

    private bool _shouldCheckBlockstunEnding;
    public bool ShouldCheckBlockstunEnding
    {
        get => _shouldCheckBlockstunEnding;
        set
        {
            if (value == _shouldCheckBlockstunEnding) return;
            _shouldCheckBlockstunEnding = value;
            OnPropertyChanged();
            CreateScenario();
        }
    }

    public IScenarioEvent? ScenarioEvent
    {
        get => (IScenarioEvent)GetValue(ScenarioEventProperty);
        set => SetValue(ScenarioEventProperty, value);
    }
    
    public static readonly DependencyProperty ScenarioEventProperty = DependencyProperty.Register(nameof(ScenarioEvent),
        typeof(IScenarioEvent), typeof(EventControl), new PropertyMetadata(default(IScenarioEvent)));

    
    
    private void CreateScenario()
    {
        ScenarioEvent = _selectedScenarioEvent switch
        {
            ScenarioEventTypes.Animation => new AnimationEvent
            {
                ShouldCheckAirTech = ShouldCheckAirTech,
                ShouldCheckStartBlocking = ShouldCheckStartBlocking,
                ShouldCheckWakingUp = ShouldCheckWakingUp,
                ShouldCheckWallSplat = ShouldCheckWallSplat,
                ShouldCheckBlockstunEnding = ShouldCheckBlockstunEnding
            },
            ScenarioEventTypes.Combo => new ComboEvent
            {
                MaxComboCount = MaxComboCount,
                MinComboCount = MinComboCount
            },
            ScenarioEventTypes.SimulatedRoundstart => new SimulatedRoundstartEvent
            {
            },
            ScenarioEventTypes.DelayAirRecovery => new DelayAirRecoveryEvent
            {
                MinDelay = MinDelayAirRecoveryDelay,
                MaxDelay = MaxDelayAirRecoveryDelay,
                AirRecoveryType = SelectedAirRecoveryType
            },
            _ => null
            
        };
    }
    
    
}

public class EventControlDataTemplateSelector : DataTemplateSelector
{
    public DataTemplate ComboDataTemplate { get; set; } = null!;
    public DataTemplate AnimationDataTemplate { get; set; } = null!;
    public DataTemplate SimulatedRoundstartDataTemplate { get; set; } = null!;
    public DataTemplate DelayAirRecoveryDataTemplate { get; set; } = null!;

    public override DataTemplate SelectTemplate(object item, DependencyObject container)
    {
        if (item is ScenarioEventTypes actionType)
        {
            return actionType switch
            {
                ScenarioEventTypes.Animation => AnimationDataTemplate,
                ScenarioEventTypes.Combo => ComboDataTemplate,
                ScenarioEventTypes.SimulatedRoundstart => SimulatedRoundstartDataTemplate,
                ScenarioEventTypes.DelayAirRecovery => DelayAirRecoveryDataTemplate,
                _ => new DataTemplate()
            };
        }

        return new DataTemplate();
    }
}