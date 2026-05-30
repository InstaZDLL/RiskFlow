using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using RiskFlow.ViewModels;
using RiskFlow.Views;
using WinRT.Interop;

namespace RiskFlow
{
    /// <summary>Fenêtre principale : barre latérale des analyses + registre des risques.</summary>
    public sealed partial class MainWindow : Window
    {
        public ShellViewModel ViewModel { get; }

        public MainWindow(ShellViewModel viewModel, MainPage page)
        {
            ViewModel = viewModel;
            InitializeComponent();
            ContentHost.Children.Add(page);
            SetWindowIcon();
        }

        private async void OnNewAnalysisClick(object sender, RoutedEventArgs e)
        {
            var dialog = new NewAnalysisDialog { XamlRoot = Content.XamlRoot };
            if (await dialog.ShowAsync() == ContentDialogResult.Primary)
                await ViewModel.CreateAnalysisAsync(dialog.AnalysisName, dialog.SelectedModelKey);
        }

        /// <summary>Applique l'icône RiskFlow à la fenêtre (barre de titre + barre des tâches).</summary>
        private void SetWindowIcon()
        {
            var hwnd = WindowNative.GetWindowHandle(this);
            var windowId = Win32Interop.GetWindowIdFromWindow(hwnd);
            var appWindow = Microsoft.UI.Windowing.AppWindow.GetFromWindowId(windowId);
            appWindow.SetIcon("Assets/RiskFlow.ico");
        }
    }
}
