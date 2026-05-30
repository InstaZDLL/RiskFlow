namespace RiskFlow.Core.Risks;

/// <summary>Libellés français et code de sérialisation du niveau de risque.</summary>
public static class RiskLabels
{
    /// <summary>Code stable (LOW/MEDIUM/HIGH/EXTREME), aligné sur TPI-Flow.</summary>
    public static string ToCode(this RiskLevel value) => value switch
    {
        RiskLevel.Low => "LOW",
        RiskLevel.Medium => "MEDIUM",
        RiskLevel.High => "HIGH",
        RiskLevel.Extreme => "EXTREME",
        _ => value.ToString().ToUpperInvariant(),
    };

    /// <summary>Libellé d'affichage français du niveau de risque.</summary>
    public static string ToFr(this RiskLevel value) => value switch
    {
        RiskLevel.Low => "Bas",
        RiskLevel.Medium => "Moyen",
        RiskLevel.High => "Élevé",
        RiskLevel.Extreme => "Critique",
        _ => value.ToString(),
    };

    public static RiskLevel ParseRiskLevel(string code) => code switch
    {
        "LOW" => RiskLevel.Low,
        "MEDIUM" => RiskLevel.Medium,
        "HIGH" => RiskLevel.High,
        "EXTREME" => RiskLevel.Extreme,
        _ => throw new ArgumentOutOfRangeException(nameof(code), code, "Niveau de risque inconnu"),
    };
}
