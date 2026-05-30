namespace RiskFlow.Core.Risks;

/// <summary>
/// Calcule le niveau de risque à partir de la gravité et de la probabilité.
/// Port fidèle de <c>calculateRiskLevel</c> (apps/web/src/lib/risk-logic.ts) de TPI-Flow :
/// la matrice ci-dessous doit rester strictement identique.
/// </summary>
public static class RiskCalculator
{
    // Lignes = Likelihood (Improbable, Possible, Probable)
    // Colonnes = Severity (Acceptable, Tolerable, Undesirable, Intolerable)
    private static readonly RiskLevel[,] Matrix =
    {
        // Acceptable     Tolerable        Undesirable     Intolerable
        { RiskLevel.Low,  RiskLevel.Medium, RiskLevel.Medium, RiskLevel.High },    // Improbable
        { RiskLevel.Low,  RiskLevel.Medium, RiskLevel.High,   RiskLevel.Extreme }, // Possible
        { RiskLevel.Medium, RiskLevel.High, RiskLevel.High,   RiskLevel.Extreme }, // Probable
    };

    public static RiskLevel Calculate(Severity severity, Likelihood likelihood)
        => Matrix[(int)likelihood, (int)severity];
}
