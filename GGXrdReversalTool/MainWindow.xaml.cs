using System;
using System.Windows;
using GGXrdReversalTool.ViewModels;

namespace GGXrdReversalTool;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }

    private void MainWindow_OnClosed(object? sender, EventArgs e)
    {
        if (DataContext is not ScenarioWindowViewModel scenarioWindowViewModel) return;
        if (!scenarioWindowViewModel.DisableCommand.CanExecute()) return;
        scenarioWindowViewModel.DisableCommand.Execute();
    }

    private void ActionControl_ImportExportSlot(object sender, Controls.ImportExportSlotEventArgs e)
    {
        if (DataContext is not ScenarioWindowViewModel scenarioWindowViewModel) return;
        scenarioWindowViewModel.ActionControl_ImportExportSlot(e);
    }
}