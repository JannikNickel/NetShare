using NetShare.ViewModels;
using NetShare.Views;
using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using Wpf.Ui.Controls;

namespace NetShare.Views
{
    public partial class MainWindow : FluentWindow
    {
        private const int WM_SYSCOMMAND = 0x112;
        private const int MF_BYPOSITION = 0x400;
        private const int MF_SEPARATOR = 0x800;
        private const int SETTINGS_ITEM_ID = 1000;

        [DllImport("user32.dll")]
        private static extern IntPtr GetSystemMenu(IntPtr hWnd, bool bRevert);

        [DllImport("user32.dll")]
        private static extern bool InsertMenu(IntPtr hMenu, int wPostiion, int wFlags, int wIdNewItem, string lpNewItem);

        public MainWindow(NavViewModel viewModel)
        {
            InitializeComponent();
            this.DataContext = viewModel;

            Loaded += MainWindow_Loaded;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            IntPtr wndHandle = new WindowInteropHelper(this).Handle;
            HwndSource hwndSrc = HwndSource.FromHwnd(wndHandle);
            hwndSrc.AddHook(new HwndSourceHook(WndProc));

            IntPtr sysMenuHandle = GetSystemMenu(wndHandle, false);
            InsertMenu(sysMenuHandle, 5, MF_BYPOSITION | MF_SEPARATOR, 0, string.Empty);
            InsertMenu(sysMenuHandle, 6, MF_BYPOSITION, SETTINGS_ITEM_ID, "Settings");
        }

        private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            if(msg == WM_SYSCOMMAND)
            {
                if(wParam.ToInt32() == SETTINGS_ITEM_ID)
                {
                    System.Windows.MessageBox.Show("Settings");
                    handled = true;
                }
            }
            return IntPtr.Zero;
        }
    }
}