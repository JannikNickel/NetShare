using Microsoft.Extensions.DependencyInjection;
using NetShare.Services;
using NetShare.ViewModels;
using NetShare.Views;
using System;
using System.Windows;

namespace NetShare
{
    public partial class App : Application
    {
        public ServiceProvider ServiceProvider { get; private set; }

        public App()
        {
            ServiceCollection services = new ServiceCollection();
            services.AddSingleton<MainWindow>();
            services.AddTransient<SettingsWindow>();

            services.AddSingleton<NavViewModel>();
            services.AddTransient<DropViewModel>();
            services.AddTransient<LoadViewModel>();
            services.AddTransient<SelectTargetViewModel>();
            services.AddTransient<TransferViewModel>();
            services.AddTransient<SettingsViewModel>();

            services.AddSingleton<INavigationService, MainNavService>();
            services.AddSingleton<INotificationService, SnackbarService>();
            services.AddSingleton<ISettingsService, PersistentSettingsService>();
            services.AddTransient<ISearchSenderService, UdpSearchSenderService>();
            services.AddTransient<ISearchListenerService, UdpSearchListenerService>();
            services.AddTransient<IReceiveContentService, TcpReceiveContentService>();
            services.AddTransient<ISendContentService, TcpSendContentService>();

            services.AddSingleton<IWindowService, WindowService>();
            services.AddSingleton<IWindowFactory>(new WindowFactoryFunc(GenerateWindow));

            services.AddSingleton<IServiceProvider>(n => n);
            ServiceProvider = services.BuildServiceProvider();
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            ISettingsService? settingsService = ServiceProvider.GetService<ISettingsService>();
            settingsService?.LoadSettings();

            MainWindow? mainWindow = ServiceProvider.GetService<MainWindow>();
            mainWindow?.Show();
        }

        private Window? GenerateWindow(Type viewModelType)
        {
            if(viewModelType == typeof(SettingsViewModel))
            {
                return ServiceProvider.GetService<SettingsWindow>();
            }
            return null;
        }
    }
}
