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
            Loaded += async (_, _) => await ViewModel.LoadCategoriesAsync();
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
