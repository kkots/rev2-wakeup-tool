using System;
using System.Windows;
using GGXrdReversalTool.Library.Scenarios.Frequency;
using GGXrdReversalTool.Library.Scenarios.Frequency.Implementations;

namespace GGXrdReversalTool.Controls;

public sealed partial class FrequencyRandomSlotControl : NotifiedUserControl
{
    public FrequencyRandomSlotControl()
    {
        InitializeComponent();
    }


    private bool _useSlot = false;
    public bool UseSlot
    {
        get => _useSlot;
        set
        {
            if (_useSlot == value) return;
            _useSlot = value;
            OnPropertyChanged("UseSlot");
        }
    }

    // Using a DependencyProperty as the backing store for GroupName.  This enables animation, styling, binding, etc...
    public static readonly DependencyProperty UseSlotProperty =
        DependencyProperty.Register(nameof(UseSlot), typeof(bool), typeof(FrequencyRandomSlotControl),
            new FrameworkPropertyMetadata(false, OnUseSlotPropertyChanged));
    private static void OnUseSlotPropertyChanged(DependencyObject source,
        DependencyPropertyChangedEventArgs eventArgs)
    {
        if (source is not FrequencyRandomSlotControl control)
        {
            return;
        }
        
        var value = eventArgs.NewValue;
        control.UseSlot = (bool)value;
    }
            
            
    private int _slotPercentage = 100;
    public int SlotPercentage
    {
        get => _slotPercentage;
        set
        {
            if (_slotPercentage == value) return;
            _slotPercentage = value;
            OnPropertyChanged("SlotPercentage");
        }
    }

    // Using a DependencyProperty as the backing store for GroupName.  This enables animation, styling, binding, etc...
    public static readonly DependencyProperty SlotPercentageProperty =
        DependencyProperty.Register(nameof(SlotPercentage), typeof(int), typeof(FrequencyRandomSlotControl),
            new PropertyMetadata(100, OnSlotPercentagePropertyChanged));
    private static void OnSlotPercentagePropertyChanged(DependencyObject source,
        DependencyPropertyChangedEventArgs eventArgs)
    {
        if (source is not FrequencyRandomSlotControl control)
        {
            return;
        }
        
        var value = eventArgs.NewValue;
        control.SlotPercentage = (int)value;
    }

    private int _slotNumber = 0;
    public int SlotNumber
    {
        get => _slotNumber;
        set
        {
            if (_slotNumber == value) return;
            _slotNumber = value;
            OnPropertyChanged("SlotNumber");
        }
    }

    // Using a DependencyProperty as the backing store for GroupName.  This enables animation, styling, binding, etc...
    public static readonly DependencyProperty SlotNumberProperty =
        DependencyProperty.Register(nameof(SlotNumber), typeof(int), typeof(FrequencyRandomSlotControl),
            new PropertyMetadata(1, OnSlotNumberPropertyChanged));

    private static void OnSlotNumberPropertyChanged(DependencyObject source,
        DependencyPropertyChangedEventArgs eventArgs)
    {
        if (source is not FrequencyRandomSlotControl control)
        {
            return;
        }
        
        var value = eventArgs.NewValue;
        control.SlotNumber = (int)value;
    }
}
