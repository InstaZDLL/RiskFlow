using System.ComponentModel.DataAnnotations.Schema;

namespace RiskFlow.Core.Risks;

/// <summary>
/// Un risque projet, évalué avant et après mitigation.
/// Miroir desktop (mono-utilisateur) de la table <c>risks</c> de TPI-Flow.
/// </summary>
public class Risk
{
    public int Id { get; set; }

    /// <summary>Numéro affiché du risque (R1, R2…), indépendant de l'Id technique.</summary>
    public int RiskNumber { get; set; } = 1;

    public string Title { get; set; } = string.Empty;

    public string Category { get; set; } = "Fonctionnel";

    public string? Description { get; set; }

    // --- Évaluation avant mitigation ---
    public Severity BeforeSeverity { get; set; } = Severity.Acceptable;
    public Likelihood BeforeLikelihood { get; set; } = Likelihood.Improbable;

    /// <summary>Niveau de risque brut, dérivé de la gravité et de la probabilité initiales.</summary>
    [NotMapped]
    public RiskLevel BeforeRiskLevel => RiskCalculator.Calculate(BeforeSeverity, BeforeLikelihood);

    public string? MitigationStrategy { get; set; }

    // --- Évaluation après mitigation ---
    public Severity AfterSeverity { get; set; } = Severity.Acceptable;
    public Likelihood AfterLikelihood { get; set; } = Likelihood.Improbable;

    /// <summary>Niveau de risque résiduel, dérivé de l'évaluation après mitigation.</summary>
    [NotMapped]
    public RiskLevel AfterRiskLevel => RiskCalculator.Calculate(AfterSeverity, AfterLikelihood);

    /// <summary>Le projet peut-il continuer malgré ce risque résiduel ?</summary>
    public bool CanContinue { get; set; } = true;

    public int SortOrder { get; set; }

    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
}
