using System;
using System.Windows.Input;

namespace NetShare.ViewModels
{
    public class RelayCommand<T>(Action<T?> execute, Predicate<T?>? canExecute = null) : ICommand
    {
        private readonly Action<T?> execute = execute;
        private readonly Predicate<T?>? canExecute = canExecute;

        public event EventHandler? CanExecuteChanged
        {
            add => CommandManager.RequerySuggested += value;
            remove => CommandManager.RequerySuggested -= value;
        }

        public bool CanExecute(object? parameter)
        {
            return canExecute?.Invoke((T?)parameter) ?? true;
        }

        public void Execute(object? parameter)
        {
            execute?.Invoke((T?)parameter);
        }
    }
}
