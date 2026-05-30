using CommunityToolkit.Mvvm.ComponentModel;
using RiskFlow.Core.Risks;

namespace RiskFlow.ViewModels;

/// <summary>
/// Ligne d'affichage d'un risque dans le registre. Les niveaux Avant/Après sont calculés
/// via le modèle de matrice de l'analyse courante (qui dépend du nombre d'axes).
/// </summary>
public sealed class RiskRowViewModel(Risk risk, RiskMatrixModel matrix) : ObservableObject
{
    /// <summary>Entité sous-jacente (utilisée pour la persistance/suppression).</summary>
    public Risk Model { get; } = risk;

    public int RiskNumber => Model.RiskNumber;
    public string Title => Model.Title;
    public string Category => Model.Category;

    public RiskLevel BeforeLevel => matrix.Level(Model.BeforeSeverityIndex, Model.BeforeLikelihoodIndex);
    public RiskLevel AfterLevel => matrix.Level(Model.AfterSeverityIndex, Model.AfterLikelihoodIndex);
}
