using CommunityToolkit.Mvvm.ComponentModel;

namespace RiskFlow.ViewModels;

/// <summary>Ligne éditable d'une catégorie de risque dans les réglages.</summary>
public partial class CategoryRowViewModel(int id, string name) : ObservableObject
{
    public int Id { get; } = id;

    [ObservableProperty]
    public partial string Name { get; set; } = name;
}
