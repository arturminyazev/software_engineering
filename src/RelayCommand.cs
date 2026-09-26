using System.Windows.Input;

namespace Blackjack;

// Связывает кнопку с методом ViewModel. Это стандартный интерфейс команд WPF.
public class RelayCommand : ICommand
{
    private readonly Action execute;
    private readonly Func<bool>? canExecute;

    public RelayCommand(Action execute, Func<bool>? canExecute = null)
    {
        this.execute = execute;
        this.canExecute = canExecute;
    }

    public bool CanExecute(object? parameter)
    {
        return canExecute == null || canExecute();
    }

    public void Execute(object? parameter)
    {
        if (CanExecute(parameter))
            execute();
    }

    public event EventHandler? CanExecuteChanged;

    public void UpdateCanExecute()
    {
        CanExecuteChanged?.Invoke(this, EventArgs.Empty);
    }
}

