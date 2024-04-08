using NetShare.ViewModels;
using Wpf.Ui.Controls;

namespace NetShare.Views
{
    public partial class SettingsWindow : FluentWindow, IView
    {
        public SettingsWindow(SettingsViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
        }
    }
}