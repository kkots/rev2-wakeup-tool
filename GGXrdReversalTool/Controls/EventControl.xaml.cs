using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using GGXrdReversalTool.Library.Scenarios.Event;
using GGXrdReversalTool.ViewModels;

namespace GGXrdReversalTool.Controls;

public sealed partial class EventControl
{
    public EventControl()
    {
        InitializeComponent();
    }
    
    public EventControlData? ControlData { get; set; }
    
    public static readonly DependencyProperty ControlDataProperty = DependencyProperty.Register(nameof(ControlData),
        typeof(EventControlData), typeof(EventControl), new PropertyMetadata(default(EventControlData)));
    
    public IEnumerable<ScenarioEventTypes> ActionTypes => Enum.GetValues<ScenarioEventTypes>();
    
    public IEnumerable<AirRecoveryTypes> AirRecoveryTypesList => Enum.GetValues<AirRecoveryTypes>();
    public IEnumerable<EndsStartsTypes> EndsStartsTypes => Enum.GetValues<EndsStartsTypes>();
    public IEnumerable<BlockTypes> BlockTypes => Enum.GetValues<BlockTypes>();
    
    
}

public class EventControlDataTemplateSelector : DataTemplateSelector
{
    public DataTemplate ComboDataTemplate { get; set; } = null!;
    public DataTemplate AnimationDataTemplate { get; set; } = null!;
    public DataTemplate SimulatedRoundstartDataTemplate { get; set; } = null!;
    public DataTemplate DelayAirRecoveryDataTemplate { get; set; } = null!;
    public DataTemplate PeriodicDataTemplate { get; set; } = null!;
    public DataTemplate BlockedACertainHitDataTemplate { get; set; } = null!;

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
                ScenarioEventTypes.Periodically => PeriodicDataTemplate,
                ScenarioEventTypes.BlockedACertainHit => BlockedACertainHitDataTemplate,
                _ => new DataTemplate()
            };
        }

        return new DataTemplate();
    }
}