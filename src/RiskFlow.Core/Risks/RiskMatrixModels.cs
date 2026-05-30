namespace RiskFlow.Core.Risks;

/// <summary>Catalogue des modèles de matrice de risque prédéfinis (figés).</summary>
public static class RiskMatrixModels
{
    private const string Key3x4 = "3x4";
    private const string Key4x4 = "4x4";
    private const string Key5x5 = "5x5";

    /// <summary>
    /// Modèle 3×4 historique de TPI-Flow : grille explicite (non multiplicative),
    /// port fidèle de l'ancienne table de <c>risk-logic.ts</c>.
    /// </summary>
    private static readonly RiskMatrixModel Model3x4 = new(
        Key3x4,
        "Standard 3×4",
        severityLevels: ["Acceptable", "Tolérable", "Indésirable", "Intolérable"],
        likelihoodLevels: ["Improbable", "Possible", "Probable"],
        grid: BuildGrid3x4());

    private static readonly RiskMatrixModel Model4x4 = new(
        Key4x4,
        "Étendu 4×4",
        severityLevels: ["Acceptable", "Tolérable", "Indésirable", "Intolérable"],
        likelihoodLevels: ["Improbable", "Possible", "Probable", "Très probable"],
        grid: BuildMultiplicativeGrid(severityCount: 4, likelihoodCount: 4));

    private static readonly RiskMatrixModel Model5x5 = new(
        Key5x5,
        "Détaillé 5×5",
        severityLevels: ["Négligeable", "Mineur", "Modéré", "Majeur", "Critique"],
        likelihoodLevels: ["Très rare", "Rare", "Possible", "Probable", "Très probable"],
        grid: BuildMultiplicativeGrid(severityCount: 5, likelihoodCount: 5));

    public static IReadOnlyList<RiskMatrixModel> All { get; } = [Model3x4, Model4x4, Model5x5];

    public static RiskMatrixModel Default => Model3x4;

    /// <summary>Retourne le modèle correspondant à la clé, ou le modèle par défaut si inconnue.</summary>
    public static RiskMatrixModel Get(string? key)
    {
        foreach (var model in All)
        {
            if (model.Key == key)
                return model;
        }
        return Default;
    }

    // Grille [severityIndex, likelihoodIndex] — Improbable / Possible / Probable.
    private static RiskLevel[,] BuildGrid3x4() => new[,]
    {
        // Improbable      Possible          Probable
        { RiskLevel.Low,    RiskLevel.Low,     RiskLevel.Medium },  // Acceptable
        { RiskLevel.Medium, RiskLevel.Medium,  RiskLevel.High },    // Tolérable
        { RiskLevel.Medium, RiskLevel.High,    RiskLevel.High },    // Indésirable
        { RiskLevel.High,   RiskLevel.Extreme, RiskLevel.Extreme }, // Intolérable
    };

    /// <summary>
    /// Construit une grille multiplicative : score = (gravité+1)·(probabilité+1) rapporté
    /// au score maximal, découpé en quatre paliers (≈0.20 / 0.45 / 0.70). Reproduit la
    /// grille 4×4 de référence.
    /// </summary>
    private static RiskLevel[,] BuildMultiplicativeGrid(int severityCount, int likelihoodCount)
    {
        var grid = new RiskLevel[severityCount, likelihoodCount];
        double max = severityCount * likelihoodCount;

        for (var s = 0; s < severityCount; s++)
        {
            for (var l = 0; l < likelihoodCount; l++)
            {
                var ratio = (s + 1) * (l + 1) / max;
                grid[s, l] = ratio switch
                {
                    <= 0.20 => RiskLevel.Low,
                    <= 0.45 => RiskLevel.Medium,
                    <= 0.70 => RiskLevel.High,
                    _ => RiskLevel.Extreme,
                };
            }
        }

        return grid;
    }
}
