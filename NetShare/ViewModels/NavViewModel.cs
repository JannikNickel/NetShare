using NetShare.Services;

namespace NetShare.ViewModels
{
    public class NavViewModel(INavigationService navService) : ViewModelBase
    {
        private ViewModelBase? currViewModel = new DropViewModel(navService);

        public ViewModelBase? CurrentViewModel
        {
            get => currViewModel;
            set => SetProperty(ref currViewModel, value);
        }
    }
}
