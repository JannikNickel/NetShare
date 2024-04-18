using NetShare.Models;
using NetShare.Services;
using System;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Threading;

namespace NetShare.ViewModels
{
    public class TransferViewModel : ViewModelBase
    {
        private readonly INavigationService navService;
        private readonly INotificationService notificationService;
        private FileCollection? content;
        private TransferReqInfo reqInfo;
        private IContentTransferService? transferService;

        private string? statusText = "Waiting for connection...";
        private int transferredFiles;
        private double transferredSize;
        private double progress;
        private long transferSpeed;

        public RelayCommand<(ISendContentService transferService, TransferTarget, FileCollection)> TransferContentCommand { get; init; }
        public RelayCommand<(IReceiveContentService, TransferReqInfo)> ReceiveContentCommand { get; init; }
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
            get => transferredSize;
            private set => SetProperty(ref transferredSize, value);
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

            TransferContentCommand = new RelayCommand<(ISendContentService, TransferTarget, FileCollection)>(TransferContent);
            ReceiveContentCommand = new RelayCommand<(IReceiveContentService, TransferReqInfo)>(ReceiveContent);
            CancelTransferCommand = new RelayCommand(CancelTransfer, null);
        }

        private void TransferContent((ISendContentService?, TransferTarget?, FileCollection?) param)
        {
            if(param.Item1 == null || param.Item2 == null || param.Item3 == null)
            {
                return;
            }
            (ISendContentService transferService, TransferTarget target, FileCollection content) = param;
            this.content = content;
            transferService.SetTransferData(target, content);
            RegisterTransferService(transferService);
        }

        private void ReceiveContent((IReceiveContentService?, TransferReqInfo) param)
        {
            if(param.Item1 == null)
            {
                return;
            }
            (IReceiveContentService transferService, TransferReqInfo reqInfo) = param;
            this.reqInfo = reqInfo;
            RegisterTransferService(transferService);
        }

        private void CancelTransfer()
        {
            transferService?.CancelTransfer();
            notificationService.Show("Transfer cancelled!", "", NotificationType.Info);
            navService.NavigateTo<DropViewModel>();
        }

        private void RegisterTransferService(IContentTransferService service)
        {
            transferService = service;
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
            notificationService.Show("Success", "Transfer complete!", NotificationType.Success, TimeSpan.FromSeconds(10));
            Dispatcher dispatcher = Dispatcher.CurrentDispatcher;
            Task.Delay(3000).ContinueWith(_ =>
            {
                dispatcher.Invoke(() => navService.NavigateTo<DropViewModel>());
            });
        }
    }
}
