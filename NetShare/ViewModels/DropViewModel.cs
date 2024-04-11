using NetShare.Models;
using NetShare.Services;
using System.Windows.Input;

namespace NetShare.ViewModels
{
    public class DropViewModel : ViewModelBase
    {
        private readonly INavigationService navService;
        private readonly ISearchSenderService searchService;

        public ICommand DropFilesCommand { get; init; }

        public DropViewModel(INavigationService navService, ISearchSenderService searchService)
        {
            this.navService = navService;
            this.searchService = searchService;
            this.searchService.Start();
            DropFilesCommand = new RelayCommand<string[]>(HandleFileDrop);
        }

        public override void OnClose()
        {
            base.OnClose();
            searchService.Stop();
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
