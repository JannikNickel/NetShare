using NetShare.Models;
using NetShare.Services;
using System.Windows.Input;

namespace NetShare.ViewModels
{
    public class DropViewModel : ViewModelBase
    {
        private readonly INavigationService navService;

        public ICommand DropFilesCommand { get; init; }

        public DropViewModel(INavigationService navService)
        {
            this.navService = navService;
            DropFilesCommand = new RelayCommand<string[]>(HandleFileDrop);
        }

        private void HandleFileDrop(string[]? files)
        {
            if(files?.Length > 0)
            {
                LoadViewModel? lvm = navService.NavigateTo<LoadViewModel>();
                FileCollection fc = new FileCollection(files);
                if(lvm?.LoadContentCommand.CanExecute(fc) == true)
                {
                    lvm?.LoadContentCommand.Execute(fc);
                }
            }
        }
    }
}
