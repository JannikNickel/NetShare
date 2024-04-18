using Microsoft.Win32;
using NetShare.Models;
using NetShare.Services;
using System;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace NetShare.ViewModels
{
    public class SettingsViewModel : ViewModelBase
    {
        private readonly ISettingsService settingsService;

        private string? displayName = "";
        private string? downloadPath = "";
        private int broadcastPort = 0;
        private int transferPort = 0;

        public ICommand SelectDownloadPathCommand { get; init; }
        public ICommand SaveCommand { get; init; }
        public ICommand CancelCommand { get; init; }

        public string? DisplayName
        {
            get => displayName;
            set => SetProperty(ref displayName, value);
        }

        public string? DownloadPath
        {
            get => downloadPath;
            set => SetProperty(ref downloadPath, value);
        }

        public int BroadcastPort
        {
            get => broadcastPort;
            set => SetProperty(ref broadcastPort, Math.Clamp(value, 0, 65535));
        }

        public int TransferPort
        {
            get => transferPort;
            set => SetProperty(ref transferPort, Math.Clamp(value, 0, 65535));
        }

        public bool IsValid
            => !string.IsNullOrEmpty(displayName?.Trim())
            && Settings.IsValidDownloadPath(downloadPath)
            && Settings.IsValidPort(broadcastPort)
            && Settings.IsValidPort(transferPort);

        public SettingsViewModel(ISettingsService settingsService)
        {
            this.settingsService = settingsService;
            SelectDownloadPathCommand = new RelayCommand(SelectDownloadPath);
            SaveCommand = new RelayCommand<IView>(SaveAndClose, _ => IsValid);
            CancelCommand = new RelayCommand<IView>(Close);

            ApplySettingsToUI(settingsService.CurrentSettings);
        }

        protected override void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            base.OnPropertyChanged(propertyName);
            base.OnPropertyChanged(nameof(IsValid));
        }

        private void ApplySettingsToUI(Settings? settings)
        {
            if(settings != null)
            {
                DisplayName = settings.DisplayName;
                DownloadPath = settings.DownloadPath;
                BroadcastPort = settings.BroadcastPort;
                TransferPort = settings.TransferPort;
            }
        }

        private Settings ReadSettingsFromUI()
        {
            Settings settings = new Settings();
            settings.DisplayName = DisplayName;
            settings.DownloadPath = DownloadPath;
            settings.BroadcastPort = BroadcastPort;
            settings.TransferPort = TransferPort;
            return settings;
        }

        private void SelectDownloadPath()
        {
            OpenFolderDialog dialog = new OpenFolderDialog();
            dialog.Multiselect = false;
            if(dialog.ShowDialog().GetValueOrDefault())
            {
                DownloadPath = dialog.FolderName;
            }
        }

        private void SaveAndClose(IView? view)
        {
            settingsService.SetSettings(ReadSettingsFromUI(), true);
            view?.Close();
        }

        private void Close(IView? view)
        {
            view?.Close();
        }
    }
}
