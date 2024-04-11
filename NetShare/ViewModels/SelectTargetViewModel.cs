﻿using NetShare.Models;
using NetShare.Services;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using System.Windows.Input;

namespace NetShare.ViewModels
{
    public class SelectTargetViewModel : ViewModelBase
    {
        private readonly INavigationService navService;
        private readonly ISearchListenerService searchService;

        private FileCollection? content;
        private bool noTargets = false;
        private ObservableCollection<TransferTarget>? targets;
        private TransferTarget? selectedTarget;

        public ICommand TransferCommand { get; init; }

        public bool NoTargets
        {
            get => noTargets;
            set => SetProperty(ref noTargets, value);
        }

        public ObservableCollection<TransferTarget>? Targets
        {
            get => targets;
            set => SetProperty(ref targets, value);
        }

        public TransferTarget? SelectedTarget
        {
            get => selectedTarget;
            set => SetProperty(ref selectedTarget, value);
        }

        public SelectTargetViewModel(INavigationService navService, ISearchListenerService searchService)
        {
            this.navService = navService;
            this.searchService = searchService;
            this.searchService.TargetsChanged += UpdateTargets;
            this.searchService.Start();
            TransferCommand = new RelayCommand(BeginTransfer);
        }

        public override void OnClose()
        {
            base.OnClose();
            searchService.Stop();
        }

        public void SetContent(FileCollection content)
        {
            this.content = content;
        }

        private void UpdateTargets(IReadOnlyCollection<TransferTarget> targets)
        {
            IPAddress? localIp = TransferTarget.GetLocalIp();
            Targets = new ObservableCollection<TransferTarget>(targets.Where(n => n.Ip != localIp));
        }

        private void BeginTransfer()
        {
            if(SelectedTarget == null || content == null)
            {
                return;
            }

            TransferViewModel? tvm = navService.NavigateTo<TransferViewModel>();
            tvm?.BeginTransfer(SelectedTarget, content);
        }
    }
}