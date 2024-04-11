using NetShare.Models;
using NetShare.Services;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace NetShare.ViewModels
{
    public class LoadViewModel : ViewModelBase
    {
        private int fileCount;
        private double fileSize;

        private INavigationService navService;
        private INotificationService notificationService;

        public AsyncRelayCommand<FileCollection> LoadContentCommand { get; init; }

        public int FileCount
        {
            get => fileCount;
            set => SetProperty(ref fileCount, value);
        }

        public double FileSize
        {
            get => fileSize;
            set => SetProperty(ref fileSize, value);
        }

        public LoadViewModel(INavigationService navService, INotificationService notificationService)
        {
            this.navService = navService;
            this.notificationService = notificationService;
            LoadContentCommand = new AsyncRelayCommand<FileCollection>(LoadContent, () => new CancellationTokenSource());
        }

        private async Task LoadContent(FileCollection? fileCollection, CancellationToken ct)
        {
            if(fileCollection == null)
            {
                return;
            }

            Dispatcher dispatcher = Dispatcher.CurrentDispatcher;
            Progress<(int files, double size)> progress = new Progress<(int files, double size)>(p =>
            {
                dispatcher.Invoke(() =>
                {
                    FileCount = p.files;
                    FileSize = p.size;
                });
            });

            await fileCollection.LoadFilesAsync(progress);
            if(fileCollection.EntryCount == 0)
            {
                notificationService.Show("No files found!", "The dropped content doesn't not contain any files that can be transfered...", NotificationType.Error);
                navService.NavigateTo<DropViewModel>();
                return;
            }

            SelectTargetViewModel? stvm = navService.NavigateTo<SelectTargetViewModel>();
            stvm?.SetContent(fileCollection);
        }
    }
}
