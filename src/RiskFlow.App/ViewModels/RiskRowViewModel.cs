using CommunityToolkit.Mvvm.ComponentModel;
using RiskFlow.Core.Risks;

namespace RiskFlow.ViewModels;

/// <summary>
/// Ligne éditable d'un risque. Les niveaux Avant/Après sont calculés en direct via le
/// modèle de matrice de l'analyse et se rafraîchissent dès qu'un index change.
/// </summary>
public partial class RiskRowViewModel : ObservableObject
{
    private readonly RiskMatrixModel _matrix;

    public RiskRowViewModel(Risk risk, RiskMatrixModel matrix)
    {
        Model = risk;
        _matrix = matrix;
        Title = risk.Title;
        Category = risk.Category;
        Description = risk.Description;
        MitigationStrategy = risk.MitigationStrategy;
        CanContinue = risk.CanContinue;
        BeforeSeverityIndex = risk.BeforeSeverityIndex;
        BeforeLikelihoodIndex = risk.BeforeLikelihoodIndex;
        AfterSeverityIndex = risk.AfterSeverityIndex;
        AfterLikelihoodIndex = risk.AfterLikelihoodIndex;
    }

    /// <summary>Entité sous-jacente (persistance/suppression).</summary>
    public Risk Model { get; }

    public int RiskNumber => Model.RiskNumber;

    [ObservableProperty]
    public partial string Title { get; set; }

    [ObservableProperty]
    public partial string Category { get; set; }

    [ObservableProperty]
    public partial string? Description { get; set; }

    [ObservableProperty]
    public partial string? MitigationStrategy { get; set; }

    [ObservableProperty]
    public partial bool CanContinue { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(BeforeLevel))]
    public partial int BeforeSeverityIndex { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(BeforeLevel))]
    public partial int BeforeLikelihoodIndex { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(AfterLevel))]
    public partial int AfterSeverityIndex { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(AfterLevel))]
    public partial int AfterLikelihoodIndex { get; set; }

    public RiskLevel BeforeLevel => _matrix.Level(BeforeSeverityIndex, BeforeLikelihoodIndex);
    public RiskLevel AfterLevel => _matrix.Level(AfterSeverityIndex, AfterLikelihoodIndex);

    /// <summary>Recopie les valeurs éditées dans l'entité avant persistance.</summary>
    public void ApplyToModel()
    {
        Model.Title = Title;
        Model.Category = Category;
        Model.Description = Description;
        Model.MitigationStrategy = MitigationStrategy;
        Model.CanContinue = CanContinue;
        Model.BeforeSeverityIndex = BeforeSeverityIndex;
        Model.BeforeLikelihoodIndex = BeforeLikelihoodIndex;
        Model.AfterSeverityIndex = AfterSeverityIndex;
        Model.AfterLikelihoodIndex = AfterLikelihoodIndex;
    }
}
