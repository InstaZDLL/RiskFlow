using Microsoft.UI.Xaml.Controls;
using RiskFlow.Core.Risks;

namespace RiskFlow.Views
{
    /// <summary>Dialog de création d'une analyse : nom + choix du modèle de matrice.</summary>
    public sealed partial class NewAnalysisDialog : ContentDialog
    {
        public NewAnalysisDialog()
        {
            InitializeComponent();
            ModelChoice.ItemsSource = RiskMatrixModels.All;
            ModelChoice.SelectedItem = RiskMatrixModels.Default;
        }

        /// <summary>Nom saisi (vide accepté : un nom par défaut sera généré).</summary>
        public string AnalysisName => NameBox.Text;

        /// <summary>Clé du modèle sélectionné.</summary>
        public string SelectedModelKey =>
            (ModelChoice.SelectedItem as RiskMatrixModel ?? RiskMatrixModels.Default).Key;
    }
}
