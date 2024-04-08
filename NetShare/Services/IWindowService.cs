using NetShare.ViewModels;

namespace NetShare.Services
{
    public interface IWindowService
    {
        void ShowWindow<T>() where T : ViewModelBase;
        void ShowDialog<T>() where T : ViewModelBase;
    }
}
