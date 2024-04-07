using System;
using System.Windows.Controls;

namespace NetShare.Services
{
    public interface INotificationService
    {
        TimeSpan DefaultTimeout { get; set; }

        void SetPresenter(ContentPresenter? presenter);
        void Show(string title, string message, NotificationType type, TimeSpan? duration = null);
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
