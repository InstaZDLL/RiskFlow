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
            mainPage.EditAnalysisRequested += OnEditAnalysisRequested;
            mainPage.ExportPdfRequested += OnExportPdfRequested;
            mainPage.ExportExcelRequested += OnExportExcelRequested;
            mainPage.ExportJsonRequested += OnExportJsonRequested;

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
            var languageBefore = _settings.Current.Language;
            var dialog = new SettingsDialog(_settingsViewModel) { XamlRoot = Content.XamlRoot };
            await dialog.ShowAsync();

            if (_settings.Current.Language != languageBefore)
                await ViewModel.Risks.ShowToastAsync(LanguageManager.Get("Toast_RestartLanguage"), seconds: 4);
        }

        private async void OnNewAnalysisClick(object sender, RoutedEventArgs e)
        {
            var dialog = new NewAnalysisDialog { XamlRoot = Content.XamlRoot };
            if (await dialog.ShowAsync() == ContentDialogResult.Primary)
            {
                await ViewModel.CreateAnalysisAsync(dialog.AnalysisName, dialog.SelectedModelKey,
                    dialog.Author, dialog.Organization, dialog.ProjectDescription);
                Nav.SelectedItem = ViewModel.SelectedAnalysis;
            }
        }

        private async void OnEditAnalysisRequested(object? sender, System.EventArgs e)
            => await EditAnalysisAsync(ViewModel.SelectedAnalysis);

        private async void OnEditAnalysisMenuClick(object sender, RoutedEventArgs e)
            => await EditAnalysisAsync(AnalysisFrom(sender));

        private async System.Threading.Tasks.Task EditAnalysisAsync(Analysis? analysis)
        {
            if (analysis is null)
                return;

            Nav.SelectedItem = analysis;
            var dialog = new NewAnalysisDialog { XamlRoot = Content.XamlRoot };
            dialog.ConfigureForEdit(analysis);
            if (await dialog.ShowAsync() == ContentDialogResult.Primary)
            {
                await ViewModel.UpdateAnalysisAsync(analysis, dialog.AnalysisName,
                    dialog.Author, dialog.Organization, dialog.ProjectDescription);
            }
        }

        private async void OnDeleteAnalysisClick(object sender, RoutedEventArgs e)
        {
            var analysis = AnalysisFrom(sender) ?? ViewModel.SelectedAnalysis;
            if (analysis is null)
                return;

            if (ViewModel.Analyses.Count <= 1)
            {
                await ShowInfoAsync(RiskFlow.Services.LanguageManager.Get("Msg_LastAnalysis"));
                return;
            }

            var confirm = new ContentDialog
            {
                Title = RiskFlow.Services.LanguageManager.Get("Confirm_DeleteTitle"),
                Content = string.Format(RiskFlow.Services.LanguageManager.Get("Confirm_DeleteBody"), analysis.Name),
                PrimaryButtonText = RiskFlow.Services.LanguageManager.Get("Common_Delete"),
                CloseButtonText = RiskFlow.Services.LanguageManager.Get("Common_Cancel"),
                DefaultButton = ContentDialogButton.Close,
                XamlRoot = Content.XamlRoot,
            };

            if (await confirm.ShowAsync() == ContentDialogResult.Primary)
            {
                await ViewModel.DeleteAnalysisCommand.ExecuteAsync(analysis);
                Nav.SelectedItem = ViewModel.SelectedAnalysis;
            }
        }

        private static Analysis? AnalysisFrom(object sender)
            => (sender as FrameworkElement)?.DataContext as Analysis;

        private System.Threading.Tasks.Task ShowInfoAsync(string message)
            => new ContentDialog
            {
                Title = "RiskFlow",
                Content = message,
                CloseButtonText = RiskFlow.Services.LanguageManager.Get("Common_Ok"),
                XamlRoot = Content.XamlRoot,
            }.ShowAsync().AsTask();

        private async void OnExportPdfRequested(object? sender, System.EventArgs e)
            => await ExportAsync(RiskFlow.Services.LanguageManager.Get("Picker_Pdf"), ".pdf", RiskReportPdf.Generate);

        private async void OnExportExcelRequested(object? sender, System.EventArgs e)
            => await ExportAsync(RiskFlow.Services.LanguageManager.Get("Picker_Excel"), ".xlsx", RiskReportExcel.Generate);

        private async System.Threading.Tasks.Task ExportAsync(string label, string extension,
            System.Func<Analysis, System.Collections.Generic.IReadOnlyList<Core.Risks.Risk>,
                Core.Risks.RiskMatrixModel, string, string, System.DateTimeOffset, byte[]> generate)
        {
            var analysis = ViewModel.SelectedAnalysis;
            if (analysis is null)
                return;

            var hwnd = WindowNative.GetWindowHandle(this);
            var picker = new Windows.Storage.Pickers.FileSavePicker
            {
                SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.DocumentsLibrary,
                SuggestedFileName = SafeFileName(analysis.Name),
            };
            picker.FileTypeChoices.Add(label, new System.Collections.Generic.List<string> { extension });
            InitializeWithWindow.Initialize(picker, hwnd);

            var file = await picker.PickSaveFileAsync();
            if (file is null)
                return;

            var risks = ViewModel.Risks.SnapshotForExport();
            var model = ViewModel.Risks.CurrentModel;
            var author = FirstNonEmpty(analysis.Author, _settings.Current.ReportAuthor);
            var organization = FirstNonEmpty(analysis.Organization, _settings.Current.ReportOrganization);

            var bytes = generate(analysis, risks, model, author, organization, System.DateTimeOffset.Now);
            await Windows.Storage.FileIO.WriteBytesAsync(file, bytes);
            await Windows.System.Launcher.LaunchFileAsync(file);
        }

        private async void OnExportJsonRequested(object? sender, System.EventArgs e)
        {
            var analysis = ViewModel.SelectedAnalysis;
            if (analysis is null)
                return;

            var picker = new Windows.Storage.Pickers.FileSavePicker
            {
                SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.DocumentsLibrary,
                SuggestedFileName = SafeFileName(analysis.Name),
            };
            picker.FileTypeChoices.Add(RiskFlow.Services.LanguageManager.Get("Picker_Json"), new System.Collections.Generic.List<string> { ".json" });
            InitializeWithWindow.Initialize(picker, WindowNative.GetWindowHandle(this));

            var file = await picker.PickSaveFileAsync();
            if (file is null)
                return;

            var json = AnalysisJson.Serialize(analysis, ViewModel.Risks.SnapshotForExport(), System.DateTimeOffset.Now);
            await Windows.Storage.FileIO.WriteTextAsync(file, json);
        }

        private async void OnImportClick(object sender, RoutedEventArgs e)
        {
            var picker = new Windows.Storage.Pickers.FileOpenPicker
            {
                SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.DocumentsLibrary,
            };
            picker.FileTypeFilter.Add(".json");
            InitializeWithWindow.Initialize(picker, WindowNative.GetWindowHandle(this));

            var file = await picker.PickSingleFileAsync();
            if (file is null)
                return;

            try
            {
                var json = await Windows.Storage.FileIO.ReadTextAsync(file);
                var dto = AnalysisJson.Deserialize(json);
                await ViewModel.ImportAnalysisAsync(dto);
                Nav.SelectedItem = ViewModel.SelectedAnalysis;
            }
            catch (System.Exception ex)
            {
                await ShowInfoAsync(string.Format(RiskFlow.Services.LanguageManager.Get("Msg_ImportError"), ex.Message));
            }
        }

        private static string FirstNonEmpty(string? a, string? b)
            => !string.IsNullOrWhiteSpace(a) ? a! : (b ?? string.Empty);

        private static string SafeFileName(string name)
        {
            var safe = string.Concat(name.Split(System.IO.Path.GetInvalidFileNameChars()));
            return string.IsNullOrWhiteSpace(safe) ? "analyse-risques" : safe;
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
