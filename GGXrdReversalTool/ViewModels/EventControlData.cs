using GGXrdReversalTool.Library.Scenarios.Event;
using System;
using System.Windows;

namespace GGXrdReversalTool.ViewModels
{
    
    public interface IEventControlDataParent {
        void CreateScenario() { }
    }
    
    public class EventControlData : DependencyObject
    {
        public IEventControlDataParent Parent;
        public EventControlData(IEventControlDataParent Parent) {
            this.Parent = Parent;
        }
        public static void CreateScenario(DependencyObject d, DependencyPropertyChangedEventArgs e) {
            ((EventControlData)d).Parent.CreateScenario();
        }
        
        public ScenarioEventTypes? SelectedScenarioEvent
        {
            get => (ScenarioEventTypes?)GetValue(SelectedScenarioEventProperty);
            set => SetValue(SelectedScenarioEventProperty, value);
        }
        public static readonly DependencyProperty SelectedScenarioEventProperty =
            DependencyProperty.Register(nameof(SelectedScenarioEvent), typeof(ScenarioEventTypes?),
                typeof(EventControlData), new PropertyMetadata(null, CreateScenario));
        
        public int MinComboCount
        {
            get => (int)GetValue(MinComboCountProperty);
            set => SetValue(MinComboCountProperty, value);
        }
        public static readonly DependencyProperty MinComboCountProperty =
            DependencyProperty.Register(nameof(MinComboCount), typeof(int),
                typeof(EventControlData), new PropertyMetadata(1, CreateScenario, CoerceMinComboCount));
        
        public static object CoerceMinComboCount(DependencyObject d, object baseValue) {
            return Math.Clamp((int)baseValue, 1, ((EventControlData)d).MaxComboCount);
        }
        
        public int MaxComboCount
        {
            get => (int)GetValue(MaxComboCountProperty);
            set => SetValue(MaxComboCountProperty, value);
        }
        public static readonly DependencyProperty MaxComboCountProperty =
            DependencyProperty.Register(nameof(MaxComboCount), typeof(int),
                typeof(EventControlData), new PropertyMetadata(100, CreateScenario, CoerceMaxComboCount));
        
        public static object CoerceMaxComboCount(DependencyObject d, object baseValue) {
            return Math.Max((int)baseValue, ((EventControlData)d).MinComboCount);
        }
        
        public EndsStartsTypes ComboHitstunEndsStarts
        {
            get => (EndsStartsTypes)GetValue(ComboHitstunEndsStartsProperty);
            set => SetValue(ComboHitstunEndsStartsProperty, value);
        }
        public static readonly DependencyProperty ComboHitstunEndsStartsProperty =
            DependencyProperty.Register(nameof(ComboHitstunEndsStarts), typeof(EndsStartsTypes),
                typeof(EventControlData), new PropertyMetadata(EndsStartsTypes.Starts, CreateScenario));
        
        public int MinDelayAirRecoveryDelay
        {
            get => (int)GetValue(MinDelayAirRecoveryDelayProperty);
            set => SetValue(MinDelayAirRecoveryDelayProperty, value);
        }
        public static readonly DependencyProperty MinDelayAirRecoveryDelayProperty =
            DependencyProperty.Register(nameof(MinDelayAirRecoveryDelay), typeof(int),
                typeof(EventControlData), new PropertyMetadata(5, CreateScenario, CoerceMinDelayAirRecoveryDelay));
        
        public static object CoerceMinDelayAirRecoveryDelay(DependencyObject d, object baseValue) {
            return Math.Clamp((int)baseValue, 0, ((EventControlData)d).MaxDelayAirRecoveryDelay);
        }
        
        public int MaxDelayAirRecoveryDelay
        {
            get => (int)GetValue(MaxDelayAirRecoveryDelayProperty);
            set => SetValue(MaxDelayAirRecoveryDelayProperty, value);
        }
        public static readonly DependencyProperty MaxDelayAirRecoveryDelayProperty =
            DependencyProperty.Register(nameof(MaxDelayAirRecoveryDelay), typeof(int),
                typeof(EventControlData), new PropertyMetadata(20, CreateScenario, CoerceMaxDelayAirRecoveryDelay));
        
        public static object CoerceMaxDelayAirRecoveryDelay(DependencyObject d, object baseValue) {
            return Math.Max((int)baseValue, ((EventControlData)d).MinDelayAirRecoveryDelay);
        }
        
        public int DelayTechProbability
        {
            get => (int)GetValue(DelayTechProbabilityProperty);
            set => SetValue(DelayTechProbabilityProperty, value);
        }
        public static readonly DependencyProperty DelayTechProbabilityProperty =
            DependencyProperty.Register(nameof(DelayTechProbability), typeof(int),
                typeof(EventControlData), new PropertyMetadata(100, CreateScenario));
        
        public int DelayAirRecoveryMinHit
        {
            get => (int)GetValue(DelayAirRecoveryMinHitProperty);
            set => SetValue(DelayAirRecoveryMinHitProperty, value);
        }
        public static readonly DependencyProperty DelayAirRecoveryMinHitProperty =
            DependencyProperty.Register(nameof(DelayAirRecoveryMinHit), typeof(int),
                typeof(EventControlData), new PropertyMetadata(1, CreateScenario, CoerceDelayAirRecoveryMinHit));
        
        public static object CoerceDelayAirRecoveryMinHit(DependencyObject d, object baseValue) {
            return Math.Clamp((int)baseValue, 1, ((EventControlData)d).DelayAirRecoveryMaxHit);
        }
        
        public int DelayAirRecoveryMaxHit
        {
            get => (int)GetValue(DelayAirRecoveryMaxHitProperty);
            set => SetValue(DelayAirRecoveryMaxHitProperty, value);
        }
        public static readonly DependencyProperty DelayAirRecoveryMaxHitProperty =
            DependencyProperty.Register(nameof(DelayAirRecoveryMaxHit), typeof(int),
                typeof(EventControlData), new PropertyMetadata(100, CreateScenario, CoerceDelayAirRecoveryMaxHit));
        
        public static object CoerceDelayAirRecoveryMaxHit(DependencyObject d, object baseValue) {
            return Math.Max((int)baseValue, ((EventControlData)d).DelayAirRecoveryMinHit);
        }
        
        public bool PeriodicallyOnlyWhenIdle
        {
            get => (bool)GetValue(PeriodicallyOnlyWhenIdleProperty);
            set => SetValue(PeriodicallyOnlyWhenIdleProperty, value);
        }
        public static readonly DependencyProperty PeriodicallyOnlyWhenIdleProperty =
            DependencyProperty.Register(nameof(PeriodicallyOnlyWhenIdle), typeof(bool),
                typeof(EventControlData), new PropertyMetadata(true, CreateScenario));
        
        public int MinPeriodic
        {
            get => (int)GetValue(MinPeriodicProperty);
            set => SetValue(MinPeriodicProperty, value);
        }
        public static readonly DependencyProperty MinPeriodicProperty =
            DependencyProperty.Register(nameof(MinPeriodic), typeof(int),
                typeof(EventControlData), new PropertyMetadata(180, CreateScenario, CoerceMinPeriodic));
        
        public static object CoerceMinPeriodic(DependencyObject d, object baseValue) {
            return Math.Clamp((int)baseValue, 1, ((EventControlData)d).MaxPeriodic);
        }
        
        public int MaxPeriodic
        {
            get => (int)GetValue(MaxPeriodicProperty);
            set => SetValue(MaxPeriodicProperty, value);
        }
        public static readonly DependencyProperty MaxPeriodicProperty =
            DependencyProperty.Register(nameof(MaxPeriodic), typeof(int),
                typeof(EventControlData), new PropertyMetadata(180, CreateScenario, CoerceMaxPeriodic));
        
        public static object CoerceMaxPeriodic(DependencyObject d, object baseValue) {
            return Math.Max((int)baseValue, ((EventControlData)d).MinPeriodic);
        }
        
        public AirRecoveryTypes SelectedAirRecoveryType
        {
            get => (AirRecoveryTypes)GetValue(SelectedAirRecoveryTypeProperty);
            set => SetValue(SelectedAirRecoveryTypeProperty, value);
        }
        public static readonly DependencyProperty SelectedAirRecoveryTypeProperty =
            DependencyProperty.Register(nameof(SelectedAirRecoveryType), typeof(AirRecoveryTypes),
                typeof(EventControlData), new PropertyMetadata(AirRecoveryTypes.Forward, CreateScenario));
        
        public bool ShouldCheckWakingUp
        {
            get => (bool)GetValue(ShouldCheckWakingUpProperty);
            set => SetValue(ShouldCheckWakingUpProperty, value);
        }
        public static readonly DependencyProperty ShouldCheckWakingUpProperty =
            DependencyProperty.Register(nameof(ShouldCheckWakingUp), typeof(bool),
                typeof(EventControlData), new PropertyMetadata(true, CreateScenario));
        
        public bool ShouldCheckWallSplat
        {
            get => (bool)GetValue(ShouldCheckWallSplatProperty);
            set => SetValue(ShouldCheckWallSplatProperty, value);
        }
        public static readonly DependencyProperty ShouldCheckWallSplatProperty =
            DependencyProperty.Register(nameof(ShouldCheckWallSplat), typeof(bool),
                typeof(EventControlData), new PropertyMetadata(false, CreateScenario));
        
        public bool ShouldCheckAirTech
        {
            get => (bool)GetValue(ShouldCheckAirTechProperty);
            set => SetValue(ShouldCheckAirTechProperty, value);
        }
        public static readonly DependencyProperty ShouldCheckAirTechProperty =
            DependencyProperty.Register(nameof(ShouldCheckAirTech), typeof(bool),
                typeof(EventControlData), new PropertyMetadata(false, CreateScenario));
        
        public bool ShouldCheckStartBlocking
        {
            get => (bool)GetValue(ShouldCheckStartBlockingProperty);
            set => SetValue(ShouldCheckStartBlockingProperty, value);
        }
        public static readonly DependencyProperty ShouldCheckStartBlockingProperty =
            DependencyProperty.Register(nameof(ShouldCheckStartBlocking), typeof(bool),
                typeof(EventControlData), new PropertyMetadata(false, CreateScenario));
        
        public bool ShouldCheckBlockstunEnding
        {
            get => (bool)GetValue(ShouldCheckBlockstunEndingProperty);
            set => SetValue(ShouldCheckBlockstunEndingProperty, value);
        }
        public static readonly DependencyProperty ShouldCheckBlockstunEndingProperty =
            DependencyProperty.Register(nameof(ShouldCheckBlockstunEnding), typeof(bool),
                typeof(EventControlData), new PropertyMetadata(false, CreateScenario));
        
        public bool ShouldCheckHitstunEnding
        {
            get => (bool)GetValue(ShouldCheckHitstunEndingProperty);
            set => SetValue(ShouldCheckHitstunEndingProperty, value);
        }
        public static readonly DependencyProperty ShouldCheckHitstunEndingProperty =
            DependencyProperty.Register(nameof(ShouldCheckHitstunEnding), typeof(bool),
                typeof(EventControlData), new PropertyMetadata(false, CreateScenario));
        
        public bool ShouldCheckHitstunStarting
        {
            get => (bool)GetValue(ShouldCheckHitstunStartingProperty);
            set => SetValue(ShouldCheckHitstunStartingProperty, value);
        }
        public static readonly DependencyProperty ShouldCheckHitstunStartingProperty =
            DependencyProperty.Register(nameof(ShouldCheckHitstunStarting), typeof(bool),
                typeof(EventControlData), new PropertyMetadata(false, CreateScenario));
        
        public int MinHitNumber
        {
            get => (int)GetValue(MinHitNumberProperty);
            set => SetValue(MinHitNumberProperty, value);
        }
        public static readonly DependencyProperty MinHitNumberProperty =
            DependencyProperty.Register(nameof(MinHitNumber), typeof(int),
                typeof(EventControlData), new PropertyMetadata(1, CreateScenario));
        
        public int MaxHitNumber
        {
            get => (int)GetValue(MaxHitNumberProperty);
            set => SetValue(MaxHitNumberProperty, value);
        }
        public static readonly DependencyProperty MaxHitNumberProperty =
            DependencyProperty.Register(nameof(MaxHitNumber), typeof(int),
                typeof(EventControlData), new PropertyMetadata(100, CreateScenario));
        
        public EndsStartsTypes BlockstunEndsStartsFilter
        {
            get => (EndsStartsTypes)GetValue(BlockstunEndsStartsFilterProperty);
            set => SetValue(BlockstunEndsStartsFilterProperty, value);
        }
        public static readonly DependencyProperty BlockstunEndsStartsFilterProperty =
            DependencyProperty.Register(nameof(BlockstunEndsStartsFilter), typeof(EndsStartsTypes),
                typeof(EventControlData), new PropertyMetadata(EndsStartsTypes.Ends, CreateScenario));
        
        public BlockTypes BlockTypeFilter
        {
            get => (BlockTypes)GetValue(BlockTypeFilterProperty);
            set => SetValue(BlockTypeFilterProperty, value);
        }
        public static readonly DependencyProperty BlockTypeFilterProperty =
            DependencyProperty.Register(nameof(BlockTypeFilter), typeof(BlockTypes),
                typeof(EventControlData), new PropertyMetadata(BlockTypes.Any, CreateScenario));
        
        public bool UseBlockSwitching
        {
            get => (bool)GetValue(UseBlockSwitchingProperty);
            set => SetValue(UseBlockSwitchingProperty, value);
        }
        public static readonly DependencyProperty UseBlockSwitchingProperty =
            DependencyProperty.Register(nameof(UseBlockSwitching), typeof(bool),
                typeof(EventControlData), new PropertyMetadata(false, CreateScenario));
        
    }
}
