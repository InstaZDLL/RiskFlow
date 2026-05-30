using Microsoft.UI.Xaml.Controls;
using RiskFlow.ViewModels;

namespace RiskFlow
{
    /// <summary>Page du registre des risques de l'analyse courante.</summary>
    public sealed partial class MainPage : Page
    {
        public RisksViewModel ViewModel { get; }

        public MainPage(RisksViewModel viewModel)
        {
            ViewModel = viewModel;
            InitializeComponent();
        }
    }
}
