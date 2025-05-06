using System.Windows;

namespace GGXrdReversalTool.Controls;

public sealed partial class FrequencyRandomSlotControl : NotifiedUserControl
{
    public FrequencyRandomSlotControl()
    {
        InitializeComponent();
    }
    
    public bool UseSlot
    {
        get => (bool)GetValue(UseSlotProperty);
        set => SetValue(UseSlotProperty, value);
    }

    public static readonly DependencyProperty UseSlotProperty =
        DependencyProperty.Register(nameof(UseSlot), typeof(bool), typeof(FrequencyRandomSlotControl),
            new PropertyMetadata(false));
            
            
    public int SlotPercentage
    {
        get => (int)GetValue(SlotPercentageProperty);
        set => SetValue(SlotPercentageProperty, value);
    }

    // Using a DependencyProperty as the backing store for GroupName.  This enables animation, styling, binding, etc...
    public static readonly DependencyProperty SlotPercentageProperty =
        DependencyProperty.Register(nameof(SlotPercentage), typeof(int), typeof(FrequencyRandomSlotControl),
            new PropertyMetadata(100));

    public int SlotNumber
    {
        get => (int)GetValue(SlotNumberProperty);
        set => SetValue(SlotNumberProperty, value);
    }

    // Using a DependencyProperty as the backing store for GroupName.  This enables animation, styling, binding, etc...
    public static readonly DependencyProperty SlotNumberProperty =
        DependencyProperty.Register(nameof(SlotNumber), typeof(int), typeof(FrequencyRandomSlotControl),
            new PropertyMetadata(0));
}
