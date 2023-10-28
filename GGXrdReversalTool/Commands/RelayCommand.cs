using System;
using System.Windows.Input;

namespace GGXrdReversalTool.Commands;

public class RelayCommand<TActionParam> : ICommand
{
    private readonly Action<TActionParam> _execute;
    private readonly Func<TActionParam, bool> _canExecute;

    public event EventHandler? CanExecuteChanged
    {
        add => CommandManager.RequerySuggested += value;
        remove => CommandManager.RequerySuggested -= value;
    }

    public RelayCommand(Action<TActionParam> execute, Func<TActionParam, bool>? canExecute = null)
    {
        _execute = execute;
        _canExecute = canExecute ?? (_ => true);
    }

    public bool CanExecute(object? parameter)
    {
        return parameter is TActionParam param && _canExecute(param);
    }

    public void Execute(object? parameter)
    {
        if (parameter is not TActionParam param) throw new InvalidOperationException();
        
        _execute(param);
    }
}

public class RelayCommand : ICommand
{
    private readonly Action _execute;
    private readonly Func<bool> _canExecute;
    
    public event EventHandler? CanExecuteChanged
    {
        add => CommandManager.RequerySuggested += value;
        remove => CommandManager.RequerySuggested -= value;
    }

    public RelayCommand(Action execute, Func<bool>? canExecute = null)
    {
        _execute = execute;
        _canExecute = canExecute ?? (() => true);
    }

    

    public bool CanExecute(object? parameter = null)
    {
        return _canExecute();
    }

    public void Execute(object? parameter = null)
    {
        _execute();
    }

}