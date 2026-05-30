namespace RiskFlow.Core.Risks;

/// <summary>Catégorie de classement des risques. Miroir de la table <c>risk_categories</c>.</summary>
public class RiskCategory
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public int SortOrder { get; set; }

    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }

    /// <summary>Catégories par défaut, identiques à DEFAULT_RISK_CATEGORIES de TPI-Flow.</summary>
    public static readonly IReadOnlyList<string> Defaults =
    [
        "Fonctionnel",
        "Technique",
        "Sécurité",
        "Organisationnel",
        "Qualité",
        "Conformité/LPD",
        "Projet",
    ];
}
