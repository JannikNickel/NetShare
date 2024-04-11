using NetShare.Services;
using NetShare.Views;
using System;
using System.Windows.Input;

namespace NetShare.ViewModels
{
    public class NavViewModel : ViewModelBase
    {
        private readonly IWindowService windowService;
        private ViewModelBase? currViewModel;

        public ICommand OpenSettingsCommand { get; init; }

        public ViewModelBase? CurrentViewModel
        {
            get => currViewModel;
            set => SetProperty(ref currViewModel, value);
        }

        public NavViewModel(INavigationService navService, IWindowService windowService, ISearchSenderService searchService)
        {
            this.windowService = windowService;
            this.currViewModel = new DropViewModel(navService, searchService);
            OpenSettingsCommand = new RelayCommand(OpenSettings);
        }

        private void OpenSettings()
        {
            windowService.ShowDialog<SettingsViewModel>();
        }
    }
}
