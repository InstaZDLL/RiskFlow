namespace RiskFlow.Core.Risks;

/// <summary>
/// Modèle de matrice de risque prédéfini : définit les libellés des axes
/// (gravité × probabilité) et la grille de niveaux de risque associée,
/// précalculée à la construction.
/// </summary>
public sealed class RiskMatrixModel
{
    private readonly RiskLevel[,] _grid; // [severityIndex, likelihoodIndex]

    /// <param name="grid">Grille [gravité, probabilité] de niveaux de risque.</param>
    public RiskMatrixModel(
        string key,
        string name,
        IReadOnlyList<string> severityLevels,
        IReadOnlyList<string> likelihoodLevels,
        RiskLevel[,] grid)
    {
        Key = key;
        Name = name;
        SeverityLevels = severityLevels;
        LikelihoodLevels = likelihoodLevels;
        _grid = grid;
    }

    /// <summary>Identifiant stable du modèle (persisté sur l'analyse) : "3x4", "4x4", "5x5".</summary>
    public string Key { get; }

    /// <summary>Nom affiché du modèle, ex. « Standard 3×4 ».</summary>
    public string Name { get; }

    /// <summary>Libellés de gravité, du moins au plus grave (index 0..n-1).</summary>
    public IReadOnlyList<string> SeverityLevels { get; }

    /// <summary>Libellés de probabilité, du moins au plus probable (index 0..m-1).</summary>
    public IReadOnlyList<string> LikelihoodLevels { get; }

    public int SeverityCount => SeverityLevels.Count;
    public int LikelihoodCount => LikelihoodLevels.Count;

    /// <summary>Dimensions affichées, ex. « 3×4 » (probabilités × gravités).</summary>
    public string Dimensions => $"{LikelihoodCount}×{SeverityCount}";

    /// <summary>Niveau de risque pour un couple (gravité, probabilité), bornes incluses.</summary>
    public RiskLevel Level(int severityIndex, int likelihoodIndex)
    {
        var s = Math.Clamp(severityIndex, 0, SeverityCount - 1);
        var l = Math.Clamp(likelihoodIndex, 0, LikelihoodCount - 1);
        return _grid[s, l];
    }
}
