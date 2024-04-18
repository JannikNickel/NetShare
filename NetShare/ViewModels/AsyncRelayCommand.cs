using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;

namespace NetShare.ViewModels
{
    public class AsyncRelayCommand<T>(Func<T?, CancellationToken, Task> execute, Func<CancellationTokenSource>? cancellationFactory = null, Predicate<T?>? canExecute = null) : ICommand
    {
        private readonly Func<T?, CancellationToken, Task> execute = execute;
        private readonly Predicate<T?>? canExecute = canExecute;
        private readonly Func<CancellationTokenSource>? cancellationFactory = cancellationFactory;
        private readonly SemaphoreSlim semaphore = new SemaphoreSlim(1, 1);
        private CancellationTokenSource? cts;

        public bool IsRunning => semaphore.CurrentCount > 0;

        public event EventHandler? CanExecuteChanged
        {
            add => CommandManager.RequerySuggested += value;
            remove => CommandManager.RequerySuggested -= value;
        }

        public bool CanExecute(object? parameter)
        {
            return semaphore.CurrentCount > 0 && (canExecute?.Invoke((T?)parameter) ?? true);
        }

        public async void Execute(object? parameter)
        {
            await semaphore.WaitAsync();
            cts = cancellationFactory?.Invoke() ?? new CancellationTokenSource();

            try
            {
                if(execute != null)
                {
                    await execute.Invoke((T?)parameter, cts.Token);
                }
            }
            catch(OperationCanceledException) { }
            catch(Exception)
            {
                throw;
            }
            finally
            {
                cts?.Dispose();
                cts = null;
                semaphore.Release();
            }
        }

        public void Cancel()
        {
            cts?.Cancel();
        }
    }
}
