using System.Linq;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Text;
using RiskFlow.Converters;
using RiskFlow.Services;
using RiskFlow.ViewModels;

namespace RiskFlow
{
    /// <summary>Page du registre des risques + matrice visuelle de l'analyse courante.</summary>
    public sealed partial class MainPage : Page
    {
        private static readonly RiskLevelToBrushConverter LevelBrushConverter = new();
        private readonly SettingsService _settings;
        private bool _useAfter;

        /// <summary>Demande d'édition de l'analyse courante (gérée par la fenêtre).</summary>
        public event System.EventHandler? EditAnalysisRequested;

        /// <summary>Demande d'export PDF de l'analyse courante (gérée par la fenêtre).</summary>
        public event System.EventHandler? ExportPdfRequested;

        /// <summary>Demande d'export Excel de l'analyse courante (gérée par la fenêtre).</summary>
        public event System.EventHandler? ExportExcelRequested;

        /// <summary>Demande d'export JSON de l'analyse courante (gérée par la fenêtre).</summary>
        public event System.EventHandler? ExportJsonRequested;

        public RisksViewModel ViewModel { get; }

        public MainPage(RisksViewModel viewModel, SettingsService settings)
        {
            ViewModel = viewModel;
            _settings = settings;
            InitializeComponent();
            ((BindingProxy)Resources["VmProxy"]).Data = ViewModel;

            // Reconstruit la matrice quand la liste des risques change si elle est affichée.
            ViewModel.Rows.CollectionChanged += (_, _) =>
            {
                if (MatriceView.Visibility == Visibility.Visible)
                    BuildMatrix();
            };

            ApplySettings();
            _settings.Changed += ApplySettings;
        }

        /// <summary>Applique les préférences d'affichage de la matrice (éval, emplacement).</summary>
        private void ApplySettings()
        {
            _useAfter = _settings.Current.MatrixDefaultEvaluation == MatrixEvaluation.After;
            EvalToggle.IsOn = _useAfter;
            ApplyPlacement(_settings.Current.MatrixPlacement);
        }

        private void ApplyPlacement(MatrixPlacement placement)
        {
            if (placement == MatrixPlacement.BelowTable)
            {
                ViewSelector.Visibility = Visibility.Collapsed;
                RegistreView.Visibility = Visibility.Visible;
                Grid.SetRow(MatriceView, 3);
                MatriceView.MinHeight = 380;
                MatriceView.Visibility = Visibility.Visible;
                BuildMatrix();
            }
            else
            {
                ViewSelector.Visibility = Visibility.Visible;
                Grid.SetRow(MatriceView, 2);
                MatriceView.MinHeight = 0;
                ViewSelector.SelectedItem = RegistreTab; // déclenche OnViewChanged
            }
        }

        private void OnViewChanged(SelectorBar sender, SelectorBarSelectionChangedEventArgs args)
        {
            if (_settings.Current.MatrixPlacement == MatrixPlacement.BelowTable)
                return; // en mode empilé, le sélecteur est masqué

            var matrice = ReferenceEquals(sender.SelectedItem, MatriceTab);
            RegistreView.Visibility = matrice ? Visibility.Collapsed : Visibility.Visible;
            MatriceView.Visibility = matrice ? Visibility.Visible : Visibility.Collapsed;

            if (matrice)
            {
                ViewModel.CloseDetailCommand.Execute(null);
                BuildMatrix();
            }
        }

        private void OnEvalToggled(object sender, RoutedEventArgs e)
        {
            _useAfter = EvalToggle.IsOn;
            BuildMatrix();
        }

        private void OnEditAnalysisClick(object sender, RoutedEventArgs e)
            => EditAnalysisRequested?.Invoke(this, System.EventArgs.Empty);

        private void OnExportPdfClick(object sender, RoutedEventArgs e)
            => ExportPdfRequested?.Invoke(this, System.EventArgs.Empty);

        private void OnExportExcelClick(object sender, RoutedEventArgs e)
            => ExportExcelRequested?.Invoke(this, System.EventArgs.Empty);

        private void OnExportJsonClick(object sender, RoutedEventArgs e)
            => ExportJsonRequested?.Invoke(this, System.EventArgs.Empty);

        /// <summary>Construit la grille colorée gravité × probabilité.</summary>
        private void BuildMatrix()
        {
            MatrixHost.Children.Clear();
            MatrixHost.RowDefinitions.Clear();
            MatrixHost.ColumnDefinitions.Clear();

            var model = ViewModel.CurrentModel;
            var nSev = model.SeverityCount;
            var nLik = model.LikelihoodCount;

            MatrixHost.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(96) });
            for (var s = 0; s < nSev; s++)
                MatrixHost.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(132) });

            MatrixHost.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            for (var l = 0; l < nLik; l++)
                MatrixHost.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

            for (var s = 0; s < nSev; s++)
                AddToGrid(HeaderText(Services.LanguageManager.Get(model.SeverityLevels[s]), center: true), row: 0, column: s + 1);

            for (var rowIdx = 1; rowIdx <= nLik; rowIdx++)
            {
                var likIndex = nLik - rowIdx;
                AddToGrid(HeaderText(Services.LanguageManager.Get(model.LikelihoodLevels[likIndex]), center: false), row: rowIdx, column: 0);

                for (var s = 0; s < nSev; s++)
                    AddToGrid(BuildCell(model.Level(s, likIndex), CellText(s, likIndex)), row: rowIdx, column: s + 1);
            }
        }

        /// <summary>Texte d'une cellule : numéros des risques ou compteur, selon les réglages.</summary>
        private string CellText(int severityIndex, int likelihoodIndex)
        {
            var matching = ViewModel.Rows
                .Where(r => (_useAfter ? r.AfterSeverityIndex : r.BeforeSeverityIndex) == severityIndex
                         && (_useAfter ? r.AfterLikelihoodIndex : r.BeforeLikelihoodIndex) == likelihoodIndex)
                .OrderBy(r => r.RiskNumber)
                .ToList();

            if (matching.Count == 0)
                return string.Empty;

            return _settings.Current.MatrixCellContent == MatrixCellContent.Count
                ? matching.Count.ToString()
                : string.Join(", ", matching.Select(r => $"R{r.RiskNumber}"));
        }

        private static Border BuildCell(Core.Risks.RiskLevel level, string content)
        {
            var brush = (Brush)LevelBrushConverter.Convert(level, typeof(Brush), null!, null!);
            return new Border
            {
                Background = brush,
                CornerRadius = new CornerRadius(6),
                Margin = new Thickness(3),
                Padding = new Thickness(8),
                MinHeight = 88,
                Child = new TextBlock
                {
                    Text = content,
                    Foreground = new SolidColorBrush(Microsoft.UI.Colors.White),
                    FontWeight = FontWeights.SemiBold,
                    FontSize = 14,
                    TextWrapping = TextWrapping.Wrap,
                    TextAlignment = TextAlignment.Center,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center,
                },
            };
        }

        private static TextBlock HeaderText(string text, bool center) => new()
        {
            Text = text,
            FontSize = 12,
            FontWeight = FontWeights.SemiBold,
            TextWrapping = TextWrapping.Wrap,
            Margin = new Thickness(6, 4, 6, 4),
            TextAlignment = center ? TextAlignment.Center : TextAlignment.Right,
            HorizontalAlignment = center ? HorizontalAlignment.Center : HorizontalAlignment.Right,
            VerticalAlignment = VerticalAlignment.Center,
            Foreground = (Brush)Application.Current.Resources["TextFillColorSecondaryBrush"],
        };

        private void AddToGrid(FrameworkElement element, int row, int column)
        {
            Grid.SetRow(element, row);
            Grid.SetColumn(element, column);
            MatrixHost.Children.Add(element);
        }
    }
}
