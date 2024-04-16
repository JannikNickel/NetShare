using NetShare.Models;
using NetShare.Services;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Threading;

namespace NetShare.ViewModels
{
    public class TransferViewModel : ViewModelBase
    {
        private INavigationService navService;
        private INotificationService notificationService;
        private ISendContentService sendService;
        private FileCollection? content;
        private int transferredFiles;
        private double transferredSize;
        private double progress;
        private long transferSpeed = 123531533;

        public AsyncRelayCommand<(TransferTarget, FileCollection)> TransferContentCommand { get; init; }
        public ICommand CancelTransferCommand { get; init; }

        public int TotalFiles => content?.EntryCount ?? 0;
        public double TotalSize => (content?.TotalSize ?? 0) / 1024d / 1024d;
        public int TransferredFiles => transferredFiles;
        public double TransferredSize => transferredSize;
        public double Progress => progress;
        public double TransferSpeed => transferSpeed * 8 / 1024d / 1024d;

        public TransferViewModel(INavigationService navService, INotificationService notificationService, ISendContentService sendService)
        {
            this.navService = navService;
            this.notificationService = notificationService;
            this.sendService = sendService;
            this.sendService.Error += OnSendError;

            TransferContentCommand = new AsyncRelayCommand<(TransferTarget, FileCollection)>(TransferContent, () => new CancellationTokenSource());
            CancelTransferCommand = new RelayCommand(CancelTransfer, null);//TODO transfer in progress

            //TODO ReceiveContent command (Run when a new tcp connection is etablished... needs IReceiveContentService which runs in the drop content page)
        }

        private async Task TransferContent((TransferTarget?, FileCollection?) param, CancellationToken ct)
        {
            if(param.Item1 == null || param.Item2 == null)
            {
                return;
            }

            (TransferTarget target, FileCollection content) = param;

            Dispatcher dispatcher = Dispatcher.CurrentDispatcher;
            Progress<(int files, double size)> progress = new Progress<(int files, double size)>(p =>
            {
                dispatcher.Invoke(() =>
                {
                    //TODO
                });
            });

            //TODO try etablish connection
            sendService.SetTransferData(target, content);
            sendService.Start();

            //if(fileCollection.EntryCount == 0)
            //{
            //    notificationService.Show("No files found!", "The dropped content doesn't not contain any files that can be transfered...", NotificationType.Error);
            //    navService.NavigateTo<DropViewModel>();
            //    return;
            //}

            //TODO begin transfer

        }

        private void CancelTransfer()
        {

        }

        private void OnSendError(string error)
        {
            notificationService.Show("Send Error", error, NotificationType.Error);
            navService.NavigateTo<DropViewModel>();
        }
    }
}
