using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using RiskFlow.Core.Risks;
using RiskFlow.Services;
using RiskFlow.ViewModels;
using RiskFlow.Views;
using WinRT.Interop;

namespace RiskFlow
{
    /// <summary>Fenêtre principale : barre latérale des analyses + registre des risques.</summary>
    public sealed partial class MainWindow : Window
    {
        private readonly SettingsService _settings;
        private readonly SettingsViewModel _settingsViewModel;

        public ShellViewModel ViewModel { get; }

        public MainWindow(ShellViewModel viewModel, MainPage mainPage, SettingsViewModel settingsViewModel, SettingsService settings)
        {
            ViewModel = viewModel;
            _settingsViewModel = settingsViewModel;
            _settings = settings;

            InitializeComponent();
            ContentHost.Children.Add(mainPage);

            SetWindowIcon();
            ApplyTheme();
            _settings.Changed += ApplyTheme;

            // Sélection initiale de l'analyse (le binding SelectedItem a été retiré).
            Nav.SelectedItem = ViewModel.SelectedAnalysis;
        }

        private async void OnNavSelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
        {
            if (args.IsSettingsSelected)
            {
                await ShowSettingsAsync();
                // Quitte l'item Réglages : revient sur l'analyse courante.
                Nav.SelectedItem = ViewModel.SelectedAnalysis;
                return;
            }

            if (args.SelectedItem is Analysis analysis)
                ViewModel.SelectedAnalysis = analysis;
        }

        private async System.Threading.Tasks.Task ShowSettingsAsync()
        {
            var dialog = new SettingsDialog(_settingsViewModel) { XamlRoot = Content.XamlRoot };
            await dialog.ShowAsync();
        }

        private async void OnNewAnalysisClick(object sender, RoutedEventArgs e)
        {
            var dialog = new NewAnalysisDialog { XamlRoot = Content.XamlRoot };
            if (await dialog.ShowAsync() == ContentDialogResult.Primary)
            {
                await ViewModel.CreateAnalysisAsync(dialog.AnalysisName, dialog.SelectedModelKey);
                Nav.SelectedItem = ViewModel.SelectedAnalysis;
            }
        }

        private void ApplyTheme()
        {
            if (Content is FrameworkElement root)
            {
                root.RequestedTheme = _settings.Current.Theme switch
                {
                    ThemeMode.Light => ElementTheme.Light,
                    ThemeMode.Dark => ElementTheme.Dark,
                    _ => ElementTheme.Default,
                };
            }
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
