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

            services.AddScoped<MainWindow>();
            services.AddScoped<DropViewModel>();
            services.AddScoped<NavViewModel>();
            services.AddScoped<LoadViewModel>();

            services.AddSingleton<INavigationService, MainNavService>();
            services.AddSingleton<INotificationService, SnackbarService>();

            services.AddSingleton<IServiceProvider>(n => n);

            ServiceProvider = services.BuildServiceProvider();
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            MainWindow? mainWindow = ServiceProvider.GetService<MainWindow>();
            mainWindow?.Show();
        }
    }
}
