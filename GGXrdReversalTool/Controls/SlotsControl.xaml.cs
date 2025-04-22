using GGXrdReversalTool.ViewModels;
using System.Windows;
using System.Windows.Controls;

namespace GGXrdReversalTool.Controls;

public partial class SlotsControl : NotifiedUserControl
{
    public SlotsControl()
    {
        InitializeComponent();
    }


    #region SlotNumber Property
    
    // This property is separate from ControlData.SlotNumber so that it can be bound to
    public int SlotNumber
    {
        get => (int)GetValue(SlotNumberProperty);
        set => SetValue(SlotNumberProperty, value);
    }
    public static readonly DependencyProperty SlotNumberProperty =
        DependencyProperty.Register(nameof(SlotNumber), typeof(int), typeof(SlotsControl),
            new PropertyMetadata(1));
    
    private void OnRadioButtonChecked(object sender, RoutedEventArgs e)
    {
        if (ControlData == null) return;
        
        RadioButton checkbox = (RadioButton)sender;
        int index = (int)checkbox.Tag;
        
        foreach (SlotsControlSlotData slot in ControlData.Slots)
        {
            if (slot.Index != index)
            {
                slot.IsChecked = false;
            }
        }
        
        SlotNumber = ControlData.SlotNumber = index + 1;
    }

    #endregion
    
    public SlotsControlData? ControlData
    {
        get => (SlotsControlData?)GetValue(ControlDataProperty);
        set => SetValue(ControlDataProperty, value);
    }
    
    public static readonly DependencyProperty ControlDataProperty =
        DependencyProperty.Register(nameof(ControlData), typeof(SlotsControlData), typeof(SlotsControl),
            new PropertyMetadata(null, OnControlDataPropertyChanged));
    
    public static void OnControlDataPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        SlotsControl control = (SlotsControl)d;
        if (control.ControlData != null)
        {
            control.SlotNumber = control.ControlData.SlotNumber;
        }
        control.OnPropertyChanged("NumberOfSlots");
        control.OnPropertyChanged("RemoveSlotButtonVisible");
    }
    
    public int NumberOfSlots
    {
        get => ControlData?.Slots.Count ?? 0;
    }
    
    #region GroupName Property


    public string GroupName
    {
        get => (string)GetValue(GroupNameProperty);
        set => SetValue(GroupNameProperty, value);
    }

    // Using a DependencyProperty as the backing store for GroupName.  This enables animation, styling, binding, etc...
    public static readonly DependencyProperty GroupNameProperty =
        DependencyProperty.Register(nameof(GroupName), typeof(string), typeof(SlotsControl),
            new PropertyMetadata("GroupName"));


    #endregion
    
    public Visibility RemoveSlotButtonVisible
    {
        get => NumberOfSlots > 3 ? Visibility.Visible : Visibility.Hidden;
    }
    
    private void OnAddSlotClick(object sender, RoutedEventArgs e)
    {
        if (ControlData == null) return;
        ControlData.AddSlotAt(ControlData.Slots.Count);
        SlotNumber = -1;
        SlotNumber = ControlData.SlotNumber;
        OnPropertyChanged("NumberOfSlots");
        OnPropertyChanged("RemoveSlotButtonVisible");
    }
    
    private void OnRemoveSlotClick(object sender, RoutedEventArgs e)
    {
        if (ControlData == null) return;
        ControlData.RemoveSlotAt(SlotNumber - 1);
        SlotNumber = -1;
        SlotNumber = ControlData.SlotNumber;
        OnPropertyChanged("NumberOfSlots");
        OnPropertyChanged("RemoveSlotButtonVisible");
        
    }
    
}