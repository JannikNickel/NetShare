﻿using NetShare.Models;
using NetShare.Services;
using System;
using System.Threading;
using System.Windows.Input;

namespace NetShare.ViewModels
{
    public class DropViewModel : ViewModelBase
    {
        private readonly INavigationService navService;
        private readonly INotificationService notificationService;
        private readonly ISearchSenderService searchService;
        private IReceiveContentService? receiveService;

        public ICommand DropFilesCommand { get; init; }

        public DropViewModel(INavigationService navService, INotificationService notificationService, ISearchSenderService searchService, IReceiveContentService receiveService)
        {
            this.navService = navService;
            this.notificationService = notificationService;
            this.searchService = searchService;
            this.searchService.Start();
            this.receiveService = receiveService;
            this.receiveService.Error += OnReceiveError;
            this.receiveService.BeginTransfer += OnBeginTransfer;
            this.receiveService.SetConfirmTransferCallback(ConfirmTransfer);
            this.receiveService.Start();
            DropFilesCommand = new RelayCommand<string[]>(HandleFileDrop);
        }

        public override void OnClose()
        {
            base.OnClose();
            searchService.Stop();
            if(receiveService != null)
            {
                receiveService.Error -= OnReceiveError;
                receiveService.BeginTransfer -= OnBeginTransfer;
                receiveService.Stop();
            }
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

        private bool ConfirmTransfer(string sender)
        {
            return notificationService.ShowDialog("Receive content?", $"Do you want to receive files from {sender}?");
        }

        private void OnReceiveError(string error)
        {
            notificationService.Show("Receive Error", error, NotificationType.Error);
            receiveService?.Start();
        }

        private void OnBeginTransfer()
        {
            IReceiveContentService? receiveService = this.receiveService;
            this.receiveService = null;
            if(receiveService != null)
            {
                receiveService.Error -= OnReceiveError;
                receiveService.BeginTransfer -= OnBeginTransfer;

                TransferViewModel? tvm = navService.NavigateTo<TransferViewModel>();
                if(tvm != null && tvm.ReceiveContentCommand.CanExecute(receiveService))
                {
                    tvm.ReceiveContentCommand.Execute(receiveService);
                }
            }
        }
    }
}
