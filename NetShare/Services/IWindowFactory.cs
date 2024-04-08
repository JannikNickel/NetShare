using NetShare.ViewModels;
using System.Windows;

namespace NetShare.Services
{
    public interface IWindowFactory
    {
        Window? GetWindow<T>() where T : ViewModelBase;
    }
}
