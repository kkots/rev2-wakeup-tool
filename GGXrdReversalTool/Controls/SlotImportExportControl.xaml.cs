using System.Windows;
using System.Windows.Controls;
using GGXrdReversalTool.Commands;

namespace GGXrdReversalTool.Controls;

public partial class SlotImportExportControl : UserControl
{
    public SlotImportExportControl()
    {
        InitializeComponent();
    }

    public RelayCommand<int> ImportCommand
    {
        get => (RelayCommand<int>)GetValue(ImportCommandProperty);
        set => SetValue(ImportCommandProperty, value);
    }

    public static readonly DependencyProperty ImportCommandProperty = DependencyProperty.Register(
        nameof(ImportCommand), typeof(RelayCommand<int>), typeof(SlotImportExportControl), new PropertyMetadata(default(RelayCommand<int>)));


    public RelayCommand<int> ExportCommand
    {
        get => (RelayCommand<int>)GetValue(ExportCommandProperty);
        set => SetValue(ExportCommandProperty, value);
    }

    public static readonly DependencyProperty ExportCommandProperty = DependencyProperty.Register(
        nameof(ExportCommand), typeof(RelayCommand<int>), typeof(SlotImportExportControl), new PropertyMetadata(default(RelayCommand<int>)));

}