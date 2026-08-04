using System;
using System.Windows.Input;

namespace BookSmith.UI.Commands;

public class RelayCommand : ICommand
{
    private readonly Action<object?> _execute;
    private readonly Predicate<object?>? _canExecute;

    public RelayCommand(Action<object?> execute, Predicate<object?>? canExecute = null)
    {
        _execute = execute ?? throw new ArgumentNullException(nameof(execute));
        _canExecute = canExecute;
    }

    public RelayCommand(Action execute, Func<bool>? canExecute = null)
    {
        if (execute == null) throw new ArgumentNullException(nameof(execute));
        _execute = _ => execute();
        _canExecute = canExecute != null ? _ => canExecute() : null;
    }

    public bool CanExecute(object? parameter) => _canExecute == null || _canExecute(parameter);

    public void Execute(object? parameter) => _execute(parameter);

    public event EventHandler? CanExecuteChanged
    {
        add => CommandManager.RequerySuggested += value;
        remove => CommandManager.RequerySuggested -= value;
    }
}

/// <summary>Typed relay command that casts the command parameter to <typeparamref name="T"/>.</summary>
public class RelayCommand<T> : ICommand
{
    private readonly Action<T?> _execute;
    private readonly Func<T?, bool>? _canExecute;

    public RelayCommand(Action<T?> execute, Func<T?, bool>? canExecute = null)
    {
        _execute = execute ?? throw new ArgumentNullException(nameof(execute));
        _canExecute = canExecute;
    }

    public bool CanExecute(object? parameter)
    {
        if (_canExecute == null) return true;
        return _canExecute(parameter is T t ? t : default);
    }

    public void Execute(object? parameter) => _execute(parameter is T t ? t : default);

    public event EventHandler? CanExecuteChanged
    {
        add => CommandManager.RequerySuggested += value;
        remove => CommandManager.RequerySuggested -= value;
    }
}
