using NetShare.ViewModels;

namespace NetShare.Services
{
    public interface INavigationService
    {
        T? NavigateTo<T>() where T : ViewModelBase;
    }
}
