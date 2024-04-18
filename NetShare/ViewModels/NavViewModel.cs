using NetShare.Services;
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

        public NavViewModel(IWindowService windowService, DropViewModel viewModel)
        {
            this.windowService = windowService;
            this.currViewModel = viewModel;
            OpenSettingsCommand = new RelayCommand(OpenSettings);
        }

        private void OpenSettings()
        {
            windowService.ShowDialog<SettingsViewModel>();
        }
    }
}
