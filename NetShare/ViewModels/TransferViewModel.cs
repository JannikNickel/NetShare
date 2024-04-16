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
        private FileCollection? content;
        private int transferredFiles;
        private double transferredSize;
        private double progress;
        private long transferSpeed = 123531533;

        public AsyncRelayCommand<(ISendContentService transferService, TransferTarget, FileCollection)> TransferContentCommand { get; init; }
        public AsyncRelayCommand<IReceiveContentService> ReceiveContentCommand { get; init; }
        public ICommand CancelTransferCommand { get; init; }

        public int TotalFiles => content?.EntryCount ?? 0;
        public double TotalSize => (content?.TotalSize ?? 0) / 1024d / 1024d;
        public int TransferredFiles => transferredFiles;
        public double TransferredSize => transferredSize;
        public double Progress => progress;
        public double TransferSpeed => transferSpeed * 8 / 1024d / 1024d;

        public TransferViewModel(INavigationService navService, INotificationService notificationService)
        {
            this.navService = navService;
            this.notificationService = notificationService;

            TransferContentCommand = new AsyncRelayCommand<(ISendContentService, TransferTarget, FileCollection)>(TransferContent, () => new CancellationTokenSource());
            ReceiveContentCommand = new AsyncRelayCommand<IReceiveContentService>(ReceiveContent, () => new CancellationTokenSource());
            CancelTransferCommand = new RelayCommand(CancelTransfer, null);//TODO transfer in progress

            //TODO ReceiveContent command (Run when a new tcp connection is etablished... needs IReceiveContentService which runs in the drop content page)
        }

        private async Task TransferContent((ISendContentService?, TransferTarget?, FileCollection?) param, CancellationToken ct)
        {
            if(param.Item1 == null || param.Item2 == null || param.Item3 == null)
            {
                return;
            }

            (ISendContentService transferService, TransferTarget target, FileCollection content) = param;

            Dispatcher dispatcher = Dispatcher.CurrentDispatcher;
            Progress<(int files, double size)> progress = new Progress<(int files, double size)>(p =>
            {
                dispatcher.Invoke(() =>
                {
                    //TODO
                });
            });

            //TODO try etablish connection
            transferService.SetTransferData(target, content);
            PrepTransferService(transferService);

            //if(fileCollection.EntryCount == 0)
            //{
            //    notificationService.Show("No files found!", "The dropped content doesn't not contain any files that can be transfered...", NotificationType.Error);
            //    navService.NavigateTo<DropViewModel>();
            //    return;
            //}

            //TODO begin transfer

        }

        private async Task ReceiveContent(IReceiveContentService? transferService, CancellationToken ct)
        {
            if(transferService == null)
            {
                return;
            }

            PrepTransferService(transferService);
        }

        private void PrepTransferService(IContentTransferService service)
        {
            service.Error += OnTransferError;
            service.Completed += OnTransferCompleted;
            service.Start();
        }

        private void OnTransferError(string error)
        {
            notificationService.Show("Transfer Error", error, NotificationType.Error);
            navService.NavigateTo<DropViewModel>();
        }

        private void OnTransferCompleted()
        {
            notificationService.Show("Success", "Transfer complete!", NotificationType.Success, TimeSpan.FromSeconds(120));
            Dispatcher dispatcher = Dispatcher.CurrentDispatcher;
            Task.Delay(3000).ContinueWith(_ =>
            {
                dispatcher.Invoke(() => navService.NavigateTo<DropViewModel>());
            });
        }

        private void CancelTransfer()
        {

        }
    }
}
