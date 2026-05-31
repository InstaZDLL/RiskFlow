using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using RiskFlow.ViewModels;

namespace RiskFlow.Views
{
    /// <summary>Pop-up des réglages : apparence, matrice, rapport, catégories.</summary>
    public sealed partial class SettingsDialog : ContentDialog
    {
        public SettingsViewModel ViewModel { get; }

        public SettingsDialog(SettingsViewModel viewModel)
        {
            ViewModel = viewModel;
            InitializeComponent();
            Loaded += async (_, _) =>
            {
                try
                {
                    await ViewModel.LoadCategoriesAsync();
                }
                catch (System.Exception ex)
                {
                    // Échec de chargement des catégories : le panneau s'ouvre sans liste plutôt que de crasher.
                    System.Diagnostics.Debug.WriteLine($"[SettingsDialog] Chargement des catégories échoué : {ex}");
                }
            };
        }

        private void OnCategoryRenamed(object sender, RoutedEventArgs e)
        {
            if (sender is FrameworkElement { DataContext: CategoryRowViewModel row })
                ViewModel.RenameCategoryCommand.Execute(row);
        }

        private void OnDeleteCategory(object sender, RoutedEventArgs e)
        {
            if (sender is FrameworkElement { DataContext: CategoryRowViewModel row })
                ViewModel.DeleteCategoryCommand.Execute(row);
        }
    }
}
