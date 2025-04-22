using GGXrdReversalTool.Library.Scenarios.Event;
using GGXrdReversalTool.ViewModels;
using System.Windows;
using System.Windows.Controls;
using System.Collections.ObjectModel;
using GGXrdReversalTool.Library.Scenarios.Event.Implementations;
namespace GGXrdReversalTool.Controls;

public sealed partial class EventTabsControl : NotifiedUserControl, IEventControlDataParent
{
    
    public ObservableCollection<EventTabElement> Tabs
    {
        get => (ObservableCollection<EventTabElement>)GetValue(TabsProperty);
        set => SetValue(TabsProperty, value);
    }
    public static readonly DependencyProperty TabsProperty =
        DependencyProperty.Register(nameof(Tabs), typeof(ObservableCollection<EventTabElement>), typeof(EventTabsControl),
            new PropertyMetadata(default(ObservableCollection<EventTabElement>), OnTabsPropertyChanged));
    
    public static void OnTabsPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        EventTabsControl control = (EventTabsControl)d;
        control.Tabs.Add(new EventTabElement( 0, false, control, true ));
        control.Tabs.Add(new EventTabElement( -1, false, control, false ));
        control.SelectedItem = control.Tabs[0];
        control.SetValue(ScenarioEventProperty, control.SelectedItem!.ScenarioEvent);
    }
    
    public EventTabsControl()
    {
        InitializeComponent();
    }
    
    public EventTabElement? SelectedItem
    {
        get => (EventTabElement?)GetValue(SelectedItemProperty);
        set => SetValue(SelectedItemProperty, value);
    }
    public static readonly DependencyProperty SelectedItemProperty =
        DependencyProperty.Register(nameof(SelectedItem), typeof(EventTabElement), typeof(EventTabsControl),
            new PropertyMetadata(null, OnSelectedItemPropertyChanged));
    
    public static void OnSelectedItemPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        EventTabsControl control = (EventTabsControl)d;
        control.OnControlSelectedItemPropertyChanged(e);
    }
    public void OnControlSelectedItemPropertyChanged(DependencyPropertyChangedEventArgs e)
    {
        EventTabElement? oldValue = (EventTabElement?)e.OldValue;
        EventTabElement? newValue = (EventTabElement?)e.NewValue;
        if (newValue != null && newValue.Index == -1) {
            if (!IsTabContentEnabled && oldValue != null)
            {
                SelectedItem = oldValue;
                TabControl.Focus();  // it does not visually update the current selected tab if I don't do this
                return;
            }
            if (Tabs.Count == 2) {
                Tabs[0].ShowCrossmark = true;
            }
            Tabs.Insert(Tabs.Count - 1,
                new EventTabElement(Tabs.Count - 1, true, this, false)
            );
            SelectedItem = Tabs[Tabs.Count - 2];
        }
        ScenarioEvent = SelectedItem?.ScenarioEvent;
        FrequencyData = SelectedItem?.FrequencyData;
        SlotsData = SelectedItem?.SlotsData;
    }
    
    private void OnTabCrossmarkClick(object sender, RoutedEventArgs e)
    {
        Button source = (Button)sender;
        int indexToRemove = (int)source.Tag;
        if (SelectedItem == Tabs[indexToRemove]) {
            if (indexToRemove + 1 >= Tabs.Count - 1) {
                SelectedItem = Tabs[indexToRemove - 1];
            } else {
                SelectedItem = Tabs[indexToRemove + 1];
            }
            SetValue(ScenarioEventProperty, SelectedItem!.ScenarioEvent);
        }
        Tabs.RemoveAt(indexToRemove);
        if (Tabs.Count == 2) {
            Tabs[0].ShowCrossmark = false;
        }
        for (int i = indexToRemove; i < Tabs.Count - 1; ++i) {
            Tabs[i].Index = i;
        }
    }
    
    public IScenarioEvent? ScenarioEvent
    {
        get => (IScenarioEvent?)GetValue(ScenarioEventProperty);
        set => SetValue(ScenarioEventProperty, value);
    }
    public static readonly DependencyProperty ScenarioEventProperty =
        DependencyProperty.Register(nameof(ScenarioEvent), typeof(IScenarioEvent), typeof(EventTabsControl),
            new PropertyMetadata(null));
    
    public void CreateScenario()
    {
        if (SelectedItem == null) return;
        EventControlData controlData = SelectedItem.ControlData;
        ScenarioEvent = SelectedItem.ScenarioEvent = controlData.SelectedScenarioEvent switch
        {
            ScenarioEventTypes.Animation => new AnimationEvent
            {
                ShouldCheckAirTech = controlData.ShouldCheckAirTech,
                ShouldCheckStartBlocking = controlData.ShouldCheckStartBlocking,
                ShouldCheckWakingUp = controlData.ShouldCheckWakingUp,
                ShouldCheckWallSplat = controlData.ShouldCheckWallSplat,
                ShouldCheckBlockstunEnding = controlData.ShouldCheckBlockstunEnding,
                ShouldCheckHitstunStarting = controlData.ShouldCheckHitstunStarting,
                ShouldCheckHitstunEnding = controlData.ShouldCheckHitstunEnding
            },
            ScenarioEventTypes.Combo => new ComboEvent
            {
                MaxComboCount = controlData.MaxComboCount,
                MinComboCount = controlData.MinComboCount,
                HitstunStartsEnds = controlData.ComboHitstunEndsStarts
            },
            ScenarioEventTypes.SimulatedRoundstart => new SimulatedRoundstartEvent
            {
            },
            ScenarioEventTypes.DelayAirRecovery => new DelayAirRecoveryEvent
            {
                MinDelay = controlData.MinDelayAirRecoveryDelay,
                MaxDelay = controlData.MaxDelayAirRecoveryDelay,
                AirRecoveryType = controlData.SelectedAirRecoveryType,
                Probability = controlData.DelayTechProbability,
                MinHitCount = controlData.DelayAirRecoveryMinHit,
                MaxHitCount = controlData.DelayAirRecoveryMaxHit
            },
            ScenarioEventTypes.Periodically => new PeriodicEvent
            {
                MinDelay = controlData.MinPeriodic,
                MaxDelay = controlData.MaxPeriodic,
                OnlyWhenIdle = controlData.PeriodicallyOnlyWhenIdle
            },
            ScenarioEventTypes.BlockedACertainHit => new BlockedACertainHitEvent
            {
                MinHitNumber = controlData.MinHitNumber,
                MaxHitNumber = controlData.MaxHitNumber,
                BlockstunEndsStartsFilter = controlData.BlockstunEndsStartsFilter,
                BlockTypeFilter = controlData.BlockTypeFilter,
                UseBlockSwitching = controlData.UseBlockSwitching
            },
            _ => null
            
        };
    }
    public bool IsTabContentEnabled
    {
        get => (bool)GetValue(IsTabContentEnabledProperty);
        set => SetValue(IsTabContentEnabledProperty, value);
    }
    
    public static readonly DependencyProperty IsTabContentEnabledProperty =
        DependencyProperty.Register(nameof(IsTabContentEnabled), typeof(bool), typeof(EventTabsControl));
    
    public FrequencyControlData? FrequencyData
    {
        get => (FrequencyControlData?)GetValue(FrequencyDataProperty);
        set => SetValue(FrequencyDataProperty, value);
    }
    public static readonly DependencyProperty FrequencyDataProperty =
        DependencyProperty.Register(nameof(FrequencyData), typeof(FrequencyControlData), typeof(EventTabsControl),
            new PropertyMetadata(null));
    
    public SlotsControlData? SlotsData
    {
        get => (SlotsControlData?)GetValue(SlotsDataProperty);
        set => SetValue(SlotsDataProperty, value);
    }
    public static readonly DependencyProperty SlotsDataProperty =
        DependencyProperty.Register(nameof(SlotsData), typeof(SlotsControlData), typeof(EventTabsControl),
            new PropertyMetadata(null));
    
}
