namespace RiskFlow.Core.Risks;

/// <summary>
/// Libellés français et codes de sérialisation des enums de risque.
/// Les chaînes correspondent exactement à celles de TPI-Flow pour garantir
/// l'interopérabilité de l'import/export (severity/likelihood en libellé FR,
/// niveau de risque en code LOW/MEDIUM/HIGH/EXTREME).
/// </summary>
public static class RiskLabels
{
    public static string ToFr(this Severity value) => value switch
    {
        Severity.Acceptable => "Acceptable",
        Severity.Tolerable => "Tolérable",
        Severity.Undesirable => "Indésirable",
        Severity.Intolerable => "Intolérable",
        _ => value.ToString(),
    };

    public static string ToFr(this Likelihood value) => value switch
    {
        Likelihood.Improbable => "Improbable",
        Likelihood.Possible => "Possible",
        Likelihood.Probable => "Probable",
        _ => value.ToString(),
    };

    /// <summary>Code de sérialisation (LOW/MEDIUM/HIGH/EXTREME), aligné sur TPI-Flow.</summary>
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

    public static Severity ParseSeverity(string fr) => fr switch
    {
        "Acceptable" => Severity.Acceptable,
        "Tolérable" => Severity.Tolerable,
        "Indésirable" => Severity.Undesirable,
        "Intolérable" => Severity.Intolerable,
        _ => throw new ArgumentOutOfRangeException(nameof(fr), fr, "Gravité inconnue"),
    };

    public static Likelihood ParseLikelihood(string fr) => fr switch
    {
        "Improbable" => Likelihood.Improbable,
        "Possible" => Likelihood.Possible,
        "Probable" => Likelihood.Probable,
        _ => throw new ArgumentOutOfRangeException(nameof(fr), fr, "Probabilité inconnue"),
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
