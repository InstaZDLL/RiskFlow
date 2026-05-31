namespace RiskFlow.Core.Risks;

/// <summary>
/// Palette de couleurs des niveaux de risque, partagée entre l'UI WinUI (converters)
/// et l'export PDF. Valeurs alignées sur getRiskLevelColor de TPI-Flow.
/// </summary>
public static class RiskPalette
{
    /// <summary>Composantes ARGB (0-255) de la couleur d'un niveau.</summary>
    public static (byte A, byte R, byte G, byte B) Argb(RiskLevel level) => level switch
    {
        RiskLevel.Low => (255, 16, 185, 129),      // emerald-500
        RiskLevel.Medium => (255, 251, 191, 36),   // amber-400
        RiskLevel.High => (255, 249, 115, 22),     // orange-500
        RiskLevel.Extreme => (255, 220, 38, 38),   // red-600
        _ => (255, 128, 128, 128),
    };

    /// <summary>Couleur hexadécimale « #RRGGBB » (pour QuestPDF).</summary>
    public static string Hex(RiskLevel level)
    {
        var (_, r, g, b) = Argb(level);
        return $"#{r:X2}{g:X2}{b:X2}";
    }
}
