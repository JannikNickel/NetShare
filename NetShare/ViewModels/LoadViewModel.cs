using NetShare.Models;
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

        public LoadViewModel()
        {
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
        }
    }
}
