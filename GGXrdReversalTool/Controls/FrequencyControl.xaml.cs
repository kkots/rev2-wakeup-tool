using System;
using System.ComponentModel;
using System.Windows;
using GGXrdReversalTool.Library.Scenarios.Frequency;
using GGXrdReversalTool.Library.Scenarios.Frequency.Implementations;
using GGXrdReversalTool.ViewModels;
using System.Linq;

namespace GGXrdReversalTool.Controls;

public sealed partial class FrequencyControl : NotifiedUserControl
{
    public FrequencyControl()
    {
        InitializeComponent();
    }
    
    public FrequencyControlData? ControlData
    {
        get => (FrequencyControlData?)GetValue(ControlDataProperty);
        set => SetValue(ControlDataProperty, value);
    }
    public static readonly DependencyProperty ControlDataProperty =
        DependencyProperty.Register(nameof(ControlData), typeof(FrequencyControlData), typeof(FrequencyControl),
            new PropertyMetadata(default(FrequencyControlData), OnControlDataPropertyChanged));
    
    public SlotsControlData? SlotsData
    {
        get => (SlotsControlData?)GetValue(SlotsDataProperty);
        set => SetValue(SlotsDataProperty, value);
    }
    public static readonly DependencyProperty SlotsDataProperty =
        DependencyProperty.Register(nameof(SlotsData), typeof(SlotsControlData), typeof(FrequencyControl),
            new PropertyMetadata(null));
    
    private FrequencyControlData? _prevSubscribedControlData = null;
    public static void OnControlDataPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        FrequencyControl control = (FrequencyControl)d;
        if (control._prevSubscribedControlData != null)
        {
            control._prevSubscribedControlData.PropertyChanged -= control.OnSubscribedControlDataPropertyChanged;
            control._prevSubscribedControlData.SlotChanged -= control.OnSubscribedControlDataSlotChanged;
        }
        
        control._prevSubscribedControlData = control.ControlData;
        if (control.ControlData != null)
        {
            control.ControlData.PropertyChanged += control.OnSubscribedControlDataPropertyChanged;
            control.ControlData.SlotChanged += control.OnSubscribedControlDataSlotChanged;
        }
        control.ResetLastChangedSlider();
    }
    
    public void OnSubscribedControlDataPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        CreateScenario();
    }
    
    private bool _ignoreAnyEvents = false;
    public void OnSubscribedControlDataSlotChanged(object? sender, SlotChangedEventArgs e)
    {
        if (_ignoreAnyEvents) return;
        _ignoreAnyEvents = true;
        if (e.Action == SlotChangedAction.Use || e.Action == SlotChangedAction.Everything)
        {
            ResetLastChangedSlider();
            if (e.Slot.Use)
            {
                LimitThisPercentage(e.Index);
            }
        }
        if (e.Action == SlotChangedAction.Percentage || e.Action == SlotChangedAction.Everything)
        {
            SetLastChangedSlider(e.Index);
            LimitOtherPercentages(e.Index);
        }
        CreateScenario();
        _ignoreAnyEvents = false;
    }
    
    private int _lastChangedSlider = -1;

    private IScenarioFrequency? _scenarioFrequency
    {
        get => TabElement?.ScenarioFrequency;
        set => TabElement!.ScenarioFrequency = value;
    }
    
    public EventTabElement? TabElement
    {
        get => (EventTabElement?)GetValue(TabElementProperty);
        set => SetValue(TabElementProperty, value);
    }
    public static readonly DependencyProperty TabElementProperty =
        DependencyProperty.Register(nameof(TabElement), typeof(EventTabElement), typeof(FrequencyControl),
            new PropertyMetadata(default(EventTabElement)));
    
    private void CreateScenario()
    {
        if (SlotsData == null || ControlData == null || TabElement == null) return;
        
        if (!ControlData.PlayRandomSlot && !ControlData.PlaySlotsInOrder && ControlData.Percentage >= 100)
        {
            _scenarioFrequency = new SingleSlotFrequency();
        }
        else if (!ControlData.PlayRandomSlot && !ControlData.PlaySlotsInOrder && ControlData.Percentage < 100)
        {
            _scenarioFrequency = new PercentageFrequency()
            {
                Percentage = ControlData.Percentage
            };
        }
        else if (!ControlData.PlayRandomSlot && ControlData.PlaySlotsInOrder)
        {
            _scenarioFrequency = new SlotsInOrderFrequency()
            {
                Percentage = ControlData.Percentage,
                ResetOnStageReset = ControlData.ResetOnStageReset,
                UsedSlotIndices = SlotsData.Slots.Where(slot => slot.Use).Select(slot => slot.Index).ToArray()
            };
        }
        else if (ControlData.PlayRandomSlot && !ControlData.PlaySlotsInOrder)
        {
            _scenarioFrequency = new RandomSlotFrequency()
            {
                Slots = SlotsData.Slots.Where(slot => slot.Use).Select(slot => new RandomSlotFrequencySlot()
                    {
                        Index = slot.Index,
                        Percentage = slot.Percentage
                    }).ToArray()
            };
        }
        else
        {
            _scenarioFrequency = null;
        }
    }
    
    private int SumPercentage()
    {
        if (SlotsData == null) return 0;
        return SlotsData.Slots.Where(slot => slot.Use).Sum(slot => slot.Percentage);
    }
    
    private int TotalUsedSlots()
    {
        if (SlotsData == null) return 0;
        return SlotsData.Slots.Where(slot => slot.Use).Count();
    }
    
    private void LimitOtherPercentages(int initiatorIndex)
    {
        int summary = SumPercentage();
        int count = TotalUsedSlots();
        if (count <= 1 || summary <= 100 || SlotsData == null) return;
        int diff = summary - 100;
        if (count == 2)
        {
            foreach (SlotsControlSlotData slot in SlotsData.Slots)
            {
                if (slot.Use && initiatorIndex != slot.Index)
                {
                    slot.Percentage = Math.Max(0, slot.Percentage - diff);
                    return;
                }
            }
            return;
        }
        
        
        SlotsControlSlotData[] otherSlots = SlotsData.Slots
            .Where(slot => slot.Use && initiatorIndex != slot.Index).ToArray();
        
        int theOtherSum = otherSlots.Sum(otherSlot => otherSlot.StartingValue);
        if (theOtherSum == 0) return;
        
        float diffFloat = diff;
        foreach (SlotsControlSlotData other in otherSlots)
        {
            float ratio = (float)other.StartingValue / (float)theOtherSum;
            int thisDiffInt = (int)(ratio * diffFloat);
            int percentageValue = other.Percentage;
            if (percentageValue < thisDiffInt)
            {
                thisDiffInt = percentageValue;
            }
            other.Percentage = percentageValue - thisDiffInt;
            diff -= thisDiffInt;
        }
        
        while (diff > 0)
        {
            int nonZeroCount = otherSlots.Where(otherSlot => otherSlot.Percentage != 0).Count();
            
            if (nonZeroCount == 0)
            {
                break;
            }
            if (nonZeroCount == 1)
            {
                foreach (SlotsControlSlotData other in otherSlots)
                {
                    int percentageValue = other.Percentage;
                    if (percentageValue != 0)
                    {
                        other.Percentage = Math.Max(0, percentageValue - diff);
                        break;
                    }
                }
                break;
            }
            
            float lowestHypotheticalPercentageChange = 0.0F;
            SlotsControlSlotData? lowestHypothetical = null;
            foreach (SlotsControlSlotData other in otherSlots)
            {
                int startingValue = other.StartingValue;
                int percentageValue = other.Percentage;
                if (percentageValue == 0 || startingValue == 0) continue;
                int hypotheticalValue = percentageValue - 1;
                float hypotheticalPercentageChange = (float)(startingValue - hypotheticalValue) / (float)startingValue;
                if (lowestHypothetical == null
                        || hypotheticalPercentageChange < lowestHypotheticalPercentageChange)
                {
                    lowestHypotheticalPercentageChange = hypotheticalPercentageChange;
                    lowestHypothetical = other;
                }
            }
            
            if (lowestHypothetical == null) break;
            else
            {
                lowestHypothetical.Percentage = Math.Max(0, lowestHypothetical.Percentage - 1);
            }
            
            --diff;
        }
    }
    private void LimitThisPercentage(int initiatorIndex)
    {
        
        int summary = SumPercentage();
        if (TotalUsedSlots() == 1 || SlotsData == null) return;
        if (summary > 100)
        {
            int diff = summary - 100;
            SlotsData[initiatorIndex].Percentage -= diff;
        }
    }
    private void ResetLastChangedSlider()
    {
        _lastChangedSlider = -1;
    }
    private void SetLastChangedSlider(int initiatorIndex)
    {
        if (_lastChangedSlider == initiatorIndex || SlotsData == null) return;
        _lastChangedSlider = initiatorIndex;
        foreach (SlotsControlSlotData slot in SlotsData.Slots)
        {
            if (slot.Use && slot.Index != initiatorIndex)
            {
                slot.StartingValue = slot.Percentage;
            }
        }
    }
    
}
