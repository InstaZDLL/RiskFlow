using RiskFlow.Core.Risks;

namespace RiskFlow.Services;

/// <summary>Libellés métier localisés (niveaux, catégories par défaut) — partagés UI + exports.</summary>
public static class RiskText
{
    public static string Level(RiskLevel level) => LanguageManager.Get("Level_" + level);

    public static string Category(string category)
        => CategoryKey(category) is { } key ? LanguageManager.Get(key) : category;

    private static string? CategoryKey(string category) => category switch
    {
        "Fonctionnel" => "Cat_Functional",
        "Technique" => "Cat_Technical",
        "Sécurité" => "Cat_Security",
        "Organisationnel" => "Cat_Organizational",
        "Qualité" => "Cat_Quality",
        "Conformité/LPD" => "Cat_Compliance",
        "Projet" => "Cat_Project",
        _ => null,
    };
}
