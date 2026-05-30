using System.Linq;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Text;
using Microsoft.UI.Xaml.Documents;
using RiskFlow.Converters;
using RiskFlow.ViewModels;

namespace RiskFlow
{
    /// <summary>Page du registre des risques + matrice visuelle de l'analyse courante.</summary>
    public sealed partial class MainPage : Page
    {
        private static readonly RiskLevelToBrushConverter LevelBrushConverter = new();
        private bool _useAfter;

        public RisksViewModel ViewModel { get; }

        public MainPage(RisksViewModel viewModel)
        {
            ViewModel = viewModel;
            InitializeComponent();

            // Reconstruit la matrice quand la liste des risques change (ajout, suppression,
            // changement d'analyse) si la vue Matrice est affichée.
            ViewModel.Rows.CollectionChanged += (_, _) =>
            {
                if (MatriceView.Visibility == Visibility.Visible)
                    BuildMatrix();
            };
        }

        private void OnViewChanged(SelectorBar sender, SelectorBarSelectionChangedEventArgs args)
        {
            var matrice = ReferenceEquals(sender.SelectedItem, MatriceTab);
            RegistreView.Visibility = matrice ? Visibility.Collapsed : Visibility.Visible;
            MatriceView.Visibility = matrice ? Visibility.Visible : Visibility.Collapsed;

            if (matrice)
            {
                ViewModel.CloseDetailCommand.Execute(null); // ferme le panneau de détail
                BuildMatrix();
            }
        }

        private void OnEvalToggled(object sender, RoutedEventArgs e)
        {
            _useAfter = EvalToggle.IsOn;
            BuildMatrix();
        }

        /// <summary>Construit la grille colorée gravité × probabilité avec les numéros de risques.</summary>
        private void BuildMatrix()
        {
            MatrixHost.Children.Clear();
            MatrixHost.RowDefinitions.Clear();
            MatrixHost.ColumnDefinitions.Clear();

            var model = ViewModel.CurrentModel;
            var nSev = model.SeverityCount;
            var nLik = model.LikelihoodCount;

            // Colonne 0 = libellés de probabilité ; colonnes 1..nSev = gravités.
            MatrixHost.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(96) });
            for (var s = 0; s < nSev; s++)
                MatrixHost.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(132) });

            // Ligne 0 = libellés de gravité ; lignes 1..nLik = probabilités.
            MatrixHost.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            for (var l = 0; l < nLik; l++)
                MatrixHost.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

            // En-têtes de gravité (haut).
            for (var s = 0; s < nSev; s++)
                AddToGrid(HeaderText(model.SeverityLevels[s], center: true), row: 0, column: s + 1);

            // Lignes : probabilité la plus forte en haut.
            for (var rowIdx = 1; rowIdx <= nLik; rowIdx++)
            {
                var likIndex = nLik - rowIdx;
                AddToGrid(HeaderText(model.LikelihoodLevels[likIndex], center: false), row: rowIdx, column: 0);

                for (var s = 0; s < nSev; s++)
                    AddToGrid(BuildCell(model.Level(s, likIndex), CellRiskNumbers(s, likIndex)), row: rowIdx, column: s + 1);
            }
        }

        /// <summary>Numéros (« R1, R2 ») des risques tombant sur une cellule gravité × probabilité.</summary>
        private string CellRiskNumbers(int severityIndex, int likelihoodIndex)
        {
            var numbers = ViewModel.Rows
                .Where(r => (_useAfter ? r.AfterSeverityIndex : r.BeforeSeverityIndex) == severityIndex
                         && (_useAfter ? r.AfterLikelihoodIndex : r.BeforeLikelihoodIndex) == likelihoodIndex)
                .OrderBy(r => r.RiskNumber)
                .Select(r => $"R{r.RiskNumber}");

            return string.Join(", ", numbers);
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
