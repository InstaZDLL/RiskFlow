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

        /// <summary>Conteneurs de la barre latérale, indexés par analyse (références directes).</summary>
        private readonly System.Collections.Generic.Dictionary<Analysis, NavigationViewItem> _navItems = new();

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

            // Construit la barre latérale et garde des références directes aux conteneurs :
            // cela permet de basculer le libellé en numéro de façon fiable quand le menu est
            // replié (l'API ContainerFromMenuItem est sujette à des problèmes de réalisation).
            BuildNavItems();
            ViewModel.Analyses.CollectionChanged += (_, _) => BuildNavItems();

            // Barre repliée → numéros « 1 », « 2 »… ; barre étendue → nom complet.
            Nav.PaneClosing += (_, _) => ApplyNumberIcons(true);
            Nav.PaneOpening += (_, _) => ApplyNumberIcons(false);
            Nav.DisplayModeChanged += (_, _) => ApplyNumberIcons(!Nav.IsPaneOpen);
            Nav.Loaded += (_, _) => ApplyNumberIcons(!Nav.IsPaneOpen);
        }

        /// <summary>(Re)construit les items de la barre latérale à partir des analyses.</summary>
        private void BuildNavItems()
        {
            _navItems.Clear();
            Nav.MenuItems.Clear();

            foreach (var analysis in ViewModel.Analyses)
            {
                var item = CreateNavItem(analysis);
                _navItems[analysis] = item;
                Nav.MenuItems.Add(item);
            }

            ApplyNumberIcons(!Nav.IsPaneOpen);
            SelectInNav(ViewModel.SelectedAnalysis);
        }

        private NavigationViewItem CreateNavItem(Analysis analysis)
        {
            // Nom (lié pour se rafraîchir au renommage) + dimensions du modèle en sous-titre.
            var name = new TextBlock();
            name.SetBinding(TextBlock.TextProperty, new Microsoft.UI.Xaml.Data.Binding
            {
                Path = new PropertyPath(nameof(Analysis.Name)),
                Source = analysis,
            });

            var dimensions = new TextBlock
            {
                Text = analysis.Model.Dimensions,
                Style = (Style)Application.Current.Resources["CaptionTextBlockStyle"],
                Foreground = (Microsoft.UI.Xaml.Media.Brush)Application.Current.Resources["TextFillColorSecondaryBrush"],
            };

            var content = new StackPanel { Spacing = 0 };
            content.Children.Add(name);
            content.Children.Add(dimensions);

            return new NavigationViewItem
            {
                Content = content,
                DataContext = analysis,
                ContextFlyout = CreateItemFlyout(analysis),
            };
        }

        private MenuFlyout CreateItemFlyout(Analysis analysis)
        {
            var edit = new MenuFlyoutItem
            {
                Text = LanguageManager.Get("Menu_Edit"),
                Icon = new FontIcon { Glyph = "" },
                DataContext = analysis,
            };
            edit.Click += OnEditAnalysisMenuClick;

            var delete = new MenuFlyoutItem
            {
                Text = LanguageManager.Get("Menu_Delete"),
                Icon = new FontIcon { Glyph = "" },
                DataContext = analysis,
            };
            delete.Click += OnDeleteAnalysisClick;

            var flyout = new MenuFlyout();
            flyout.Items.Add(edit);
            flyout.Items.Add(delete);
            return flyout;
        }

        /// <summary>Affiche (true) ou retire (false) le numéro d'ordre en icône de chaque analyse.</summary>
        private void ApplyNumberIcons(bool show)
        {
            var i = 0;
            foreach (var analysis in ViewModel.Analyses)
            {
                if (_navItems.TryGetValue(analysis, out var item))
                {
                    item.Icon = show
                        ? new FontIcon
                        {
                            Glyph = (i + 1).ToString(),
                            FontFamily = new Microsoft.UI.Xaml.Media.FontFamily("Segoe UI"),
                            FontSize = 15,
                        }
                        : null;
                }
                i++;
            }
        }

        /// <summary>Sélectionne le conteneur de la barre correspondant à l'analyse donnée.</summary>
        private void SelectInNav(Analysis? analysis)
            => Nav.SelectedItem = analysis is not null && _navItems.TryGetValue(analysis, out var item) ? item : null;

        private async void OnNavSelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
        {
            if (args.IsSettingsSelected)
            {
                await ShowSettingsAsync();
                // Quitte l'item Réglages : revient sur l'analyse courante.
                SelectInNav(ViewModel.SelectedAnalysis);
                return;
            }

            if (args.SelectedItem is NavigationViewItem { DataContext: Analysis analysis })
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
                SelectInNav(ViewModel.SelectedAnalysis);
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

            SelectInNav(analysis);
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
                SelectInNav(ViewModel.SelectedAnalysis);
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

            try
            {
                var risks = ViewModel.Risks.SnapshotForExport();
                var model = ViewModel.Risks.CurrentModel;
                var author = FirstNonEmpty(analysis.Author, _settings.Current.ReportAuthor);
                var organization = FirstNonEmpty(analysis.Organization, _settings.Current.ReportOrganization);

                var bytes = generate(analysis, risks, model, author, organization, System.DateTimeOffset.Now);
                await Windows.Storage.FileIO.WriteBytesAsync(file, bytes);
                await Windows.System.Launcher.LaunchFileAsync(file);
            }
            catch (System.Exception ex)
            {
                await ShowInfoAsync(string.Format(RiskFlow.Services.LanguageManager.Get("Msg_ExportError"), ex.Message));
            }
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

            try
            {
                var json = AnalysisJson.Serialize(analysis, ViewModel.Risks.SnapshotForExport(), System.DateTimeOffset.Now);
                await Windows.Storage.FileIO.WriteTextAsync(file, json);
            }
            catch (System.Exception ex)
            {
                await ShowInfoAsync(string.Format(RiskFlow.Services.LanguageManager.Get("Msg_ExportError"), ex.Message));
            }
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
                SelectInNav(ViewModel.SelectedAnalysis);
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
