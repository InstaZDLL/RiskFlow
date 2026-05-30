namespace RiskFlow.Core.Risks;

/// <summary>
/// Un risque projet, évalué avant et après mitigation. La gravité et la probabilité
/// sont stockées sous forme d'index relatifs au modèle de matrice de l'analyse parente
/// (le niveau de risque en est dérivé via <see cref="RiskMatrixModel.Level"/>).
/// </summary>
public class Risk
{
    public int Id { get; set; }

    /// <summary>Analyse parente.</summary>
    public int AnalysisId { get; set; }
    public Analysis? Analysis { get; set; }

    /// <summary>Numéro affiché du risque (R1, R2…), indépendant de l'Id technique.</summary>
    public int RiskNumber { get; set; } = 1;

    public string Title { get; set; } = string.Empty;

    public string Category { get; set; } = "Fonctionnel";

    public string? Description { get; set; }

    // --- Évaluation avant mitigation (index dans le modèle de l'analyse) ---
    public int BeforeSeverityIndex { get; set; }
    public int BeforeLikelihoodIndex { get; set; }

    public string? MitigationStrategy { get; set; }

    // --- Évaluation après mitigation ---
    public int AfterSeverityIndex { get; set; }
    public int AfterLikelihoodIndex { get; set; }

    /// <summary>Le projet peut-il continuer malgré ce risque résiduel ?</summary>
    public bool CanContinue { get; set; } = true;

    public int SortOrder { get; set; }

    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
}
