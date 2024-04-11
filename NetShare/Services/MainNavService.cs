using NetShare.ViewModels;
using System;

namespace NetShare.Services
{
    public class MainNavService(IServiceProvider serviceProvider) : INavigationService
    {
        private readonly IServiceProvider serviceProvider = serviceProvider;

        public T? NavigateTo<T>() where T : ViewModelBase
        {
            NavViewModel? navViewModel = serviceProvider.GetService<NavViewModel>();
            if(navViewModel != null)
            {
                navViewModel.CurrentViewModel?.OnClose();
                T? vm = serviceProvider.GetService<T>();
                navViewModel.CurrentViewModel = vm;
                return vm;
            }
            return null;
        }
    }
}
