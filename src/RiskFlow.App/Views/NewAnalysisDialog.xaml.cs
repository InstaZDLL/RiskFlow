using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using RiskFlow.Core.Risks;

namespace RiskFlow.Views
{
    /// <summary>
    /// Dialog de création ou d'édition d'une analyse : nom, modèle de matrice et
    /// informations de rapport (auteur, organisation, description). En édition, le
    /// modèle est verrouillé (le changer invaliderait les index des risques).
    /// </summary>
    public sealed partial class NewAnalysisDialog : ContentDialog
    {
        public NewAnalysisDialog()
        {
            InitializeComponent();
            ModelChoice.ItemsSource = RiskMatrixModels.All;
            ModelChoice.SelectedItem = RiskMatrixModels.Default;
        }

        public string AnalysisName => NameBox.Text;
        public string SelectedModelKey =>
            (ModelChoice.SelectedItem as RiskMatrixModel ?? RiskMatrixModels.Default).Key;
        public string Author => AuthorBox.Text;
        public string Organization => OrgBox.Text;
        public string ProjectDescription => DescBox.Text;

        /// <summary>Configure le dialog pour modifier une analyse existante.</summary>
        public void ConfigureForEdit(Analysis analysis)
        {
            Title = RiskFlow.Services.LanguageManager.Get("Dialog_EditTitle");
            PrimaryButtonText = RiskFlow.Services.LanguageManager.Get("Common_Save");

            NameBox.Text = analysis.Name;
            AuthorBox.Text = analysis.Author ?? string.Empty;
            OrgBox.Text = analysis.Organization ?? string.Empty;
            DescBox.Text = analysis.ProjectDescription ?? string.Empty;

            ModelChoice.SelectedItem = analysis.Model;
            ModelChoice.IsEnabled = false;
            ModelLockHint.Visibility = Visibility.Visible;
        }
    }
}
