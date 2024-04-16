using NetShare.Services;
using NetShare.ViewModels;
using System;
using System.Windows;
using System.Windows.Interop;
using Wpf.Ui.Controls;

namespace NetShare.Views
{
    public partial class MainWindow : FluentWindow, IView
    {
        public MainWindow(NavViewModel viewModel, INotificationService notificationService)
        {
            InitializeComponent();
            this.DataContext = viewModel;

            notificationService.SetPresenter(SnackbarPresenter);
            Loaded += MainWindow_Loaded;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            IntPtr wndHandle = new WindowInteropHelper(this).Handle;
            HwndSource hwndSrc = HwndSource.FromHwnd(wndHandle);
            hwndSrc.AddHook(new HwndSourceHook(WndProc));

            IntPtr sysMenuHandle = Win32.GetSystemMenu(wndHandle, false);
            Win32.InsertMenu(sysMenuHandle, 5, Win32.MF_BYPOSITION | Win32.MF_SEPARATOR, 0, string.Empty);
            Win32.InsertMenu(sysMenuHandle, 6, Win32.MF_BYPOSITION, Win32.SETTINGS_ITEM_ID, "Settings");
        }

        private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            if(msg == Win32.WM_SYSCOMMAND)
            {
                if(wParam.ToInt32() == Win32.SETTINGS_ITEM_ID)
                {
                    ((NavViewModel)this.DataContext).OpenSettingsCommand.Execute(null);
                    handled = true;
                }
            }
            return IntPtr.Zero;
        }
    }
}