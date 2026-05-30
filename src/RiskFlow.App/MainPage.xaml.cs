using Microsoft.UI.Xaml.Controls;
using RiskFlow.ViewModels;

namespace RiskFlow
{
    /// <summary>Page principale : registre des risques.</summary>
    public sealed partial class MainPage : Page
    {
        public RisksViewModel ViewModel { get; }

        public MainPage(RisksViewModel viewModel)
        {
            ViewModel = viewModel;
            InitializeComponent();
            ViewModel.LoadCommand.Execute(null);
        }
    }
}
