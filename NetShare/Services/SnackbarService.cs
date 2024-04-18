using System;
using System.Threading.Tasks;
using System.Windows.Controls;
using Wpf.Ui.Controls;

namespace NetShare.Services
{
    public class SnackbarService : INotificationService
    {
        private SnackbarPresenter? presenter;
        private Snackbar? snackbar;

        public TimeSpan DefaultTimeout { get; set; } = TimeSpan.FromSeconds(3.0);

        public void SetPresenter(ContentPresenter? presenter)
        {
            if(presenter != null && presenter is not SnackbarPresenter)
            {
                throw new ArgumentException($"Expected {nameof(presenter)} of type SnackbarPresenter!", nameof(presenter));
            }
            this.presenter = presenter as SnackbarPresenter;
        }

        public void Show(string title, string message, NotificationType type, TimeSpan? timeout = null)
        {
            if(presenter == null)
            {
                throw new InvalidOperationException("Presenter has to be set previously!");
            }

            snackbar ??= new Snackbar(presenter);
            snackbar.Title = title;
            snackbar.Content = message;
            snackbar.Appearance = type switch
            {
                NotificationType.None => ControlAppearance.Transparent,
                NotificationType.Info => ControlAppearance.Info,
                NotificationType.Warning => ControlAppearance.Caution,
                NotificationType.Error => ControlAppearance.Danger,
                NotificationType.Success => ControlAppearance.Success,
                _ => ControlAppearance.Transparent
            };
            snackbar.Timeout = timeout ?? DefaultTimeout;
            snackbar.Show();
        }

        public async Task<bool> ShowDialog(string title, string message)
        {
            MessageBox mb = new MessageBox()
            {
                Title = title,
                Content = message,
                PrimaryButtonText = "Yes",
                CloseButtonText = "No"
            };
            MessageBoxResult res = await mb.ShowDialogAsync();
            return res == MessageBoxResult.Primary;
        }
    }
}
