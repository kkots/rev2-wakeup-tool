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
        if (DataContext is ScenarioWindowViewModel scenarioWindowViewModel)
        {
            if (scenarioWindowViewModel.DisableCommand.CanExecute())
            {
                scenarioWindowViewModel.DisableCommand.Execute();
            }
        }
    }
}