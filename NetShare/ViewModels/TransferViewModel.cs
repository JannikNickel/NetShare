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
        private TransferReqInfo reqInfo;

        private string? statusText = "Waiting for connection...";
        private int transferredFiles;
        private double transferredSize;
        private double progress;
        private long transferSpeed = 123531533;

        public AsyncRelayCommand<(ISendContentService transferService, TransferTarget, FileCollection)> TransferContentCommand { get; init; }
        public AsyncRelayCommand<(IReceiveContentService, TransferReqInfo)> ReceiveContentCommand { get; init; }
        public ICommand CancelTransferCommand { get; init; }

        public string? StatusText
        {
            get => statusText;
            private set => SetProperty(ref statusText, value);
        }
        public int TransferredFiles
        {
            get => transferredFiles;
            private set => SetProperty(ref transferredFiles, value);
        }
        public double TransferredSize
        {
            get => transferredSize / 1024d / 1024d;
            private set => SetProperty(ref transferredSize, (long)Math.Round(value) * 1024 * 1024);
        }
        public double Progress
        {
            get => progress;
            private set => SetProperty(ref progress, Math.Clamp(value, 0.0, 1.0));
        }
        public long TransferSpeed
        {
            get => transferSpeed;
            private set => SetProperty(ref transferSpeed, value);
        }
        public int TotalFiles => content?.EntryCount ?? reqInfo.TotalFiles;
        public long TotalSize => (content?.TotalSize ?? reqInfo.TotalSize);
        public double TotalSizeMb => TotalSize / 1024d / 1024d;

        public TransferViewModel(INavigationService navService, INotificationService notificationService)
        {
            this.navService = navService;
            this.notificationService = notificationService;

            TransferContentCommand = new AsyncRelayCommand<(ISendContentService, TransferTarget, FileCollection)>(TransferContent, () => new CancellationTokenSource());
            ReceiveContentCommand = new AsyncRelayCommand<(IReceiveContentService, TransferReqInfo)>(ReceiveContent, () => new CancellationTokenSource());
            CancelTransferCommand = new RelayCommand(CancelTransfer, null);//TODO transfer in progress
        }

        private async Task TransferContent((ISendContentService?, TransferTarget?, FileCollection?) param, CancellationToken ct)
        {
            if(param.Item1 == null || param.Item2 == null || param.Item3 == null)
            {
                return;
            }

            (ISendContentService transferService, TransferTarget target, FileCollection content) = param;
            this.content = content;
            transferService.SetTransferData(target, content);
            PrepTransferService(transferService);
        }

        private async Task ReceiveContent((IReceiveContentService?, TransferReqInfo) param, CancellationToken ct)
        {
            if(param.Item1 == null)
            {
                return;
            }

            (IReceiveContentService transferService, TransferReqInfo reqInfo) = param;
            PrepTransferService(transferService);
            this.reqInfo = reqInfo;
        }

        private void PrepTransferService(IContentTransferService service)
        {
            service.Error += OnTransferError;
            service.Progress += OnTransferProgress;
            service.Completed += OnTransferCompleted;
            service.Start();
        }

        private void OnTransferError(string error)
        {
            notificationService.Show("Transfer Error", error, NotificationType.Error);
            navService.NavigateTo<DropViewModel>();
        }

        private void OnTransferProgress(TransferProgressEventArgs p)
        {
            StatusText = content != null ? "Transferring files..." : "Receiving files...";
            TransferredFiles = p.FilesCompleted;
            TransferredSize = p.BytesCompleted / (double)(1024 * 1024);
            TransferSpeed = p.Rate * 8 / (1024 * 1024);
            Progress = p.BytesCompleted / (double)TotalSize;
        }

        private void OnTransferCompleted()
        {
            StatusText = "Transfer completed...";
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
