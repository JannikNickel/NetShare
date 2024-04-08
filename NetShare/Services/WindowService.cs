using NetShare.ViewModels;
using System.Windows;

namespace NetShare.Services
{
    public class WindowService(IWindowFactory windowFactory) : IWindowService
    {
        private readonly IWindowFactory windowFactory = windowFactory;

        public void ShowWindow<T>() where T : ViewModelBase
        {
            Window? window = windowFactory.GetWindow<T>();
            window?.Show();
        }

        public void ShowDialog<T>() where T : ViewModelBase
        {
            Window? window = windowFactory.GetWindow<T>();
            window?.ShowDialog();
        }
    }
}
