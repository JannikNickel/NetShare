using System;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace NetShare.Services
{
    public interface INotificationService
    {
        TimeSpan DefaultTimeout { get; set; }

        void SetPresenter(ContentPresenter? presenter);
        void Show(string title, string message, NotificationType type, TimeSpan? duration = null);
        Task<bool> ShowDialog(string title, string message);
    }

    public enum NotificationType
    {
        None,
        Info,
        Warning,
        Error,
        Success
    }
}
