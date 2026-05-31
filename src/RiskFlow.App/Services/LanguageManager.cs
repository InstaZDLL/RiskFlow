using System;
using System.Globalization;
using Microsoft.Windows.ApplicationModel.Resources;

namespace RiskFlow.Services;

/// <summary>
/// Localisation native (.resw via le ResourceManager du Windows App SDK). La langue est
/// appliquée au chargement des écrans ; un changement de langue recrée la fenêtre.
/// </summary>
public static class LanguageManager
{
    private static readonly ResourceManager Manager = new();
    private static string _code = "fr-FR";

    /// <summary>Tag de langue courant (« fr-FR » / « en-US »), aligné sur les fichiers .resw.</summary>
    public static string CurrentCode => _code;

    /// <summary>Sélectionne la langue (Système → langue de Windows, sinon fr/en).</summary>
    public static void SetLanguage(AppLanguage language)
    {
        _code = language switch
        {
            AppLanguage.French => "fr-FR",
            AppLanguage.English => "en-US",
            _ => CultureInfo.CurrentUICulture.TwoLetterISOLanguageName.Equals("fr", StringComparison.OrdinalIgnoreCase) ? "fr-FR" : "en-US",
        };

        try
        {
            Windows.Globalization.ApplicationLanguages.PrimaryLanguageOverride = _code;
        }
        catch
        {
            // Sans impact sur la lecture des ressources (contexte explicite ci-dessous).
        }
    }

    /// <summary>Chaîne traduite pour la clé (retourne la clé brute si absente).</summary>
    public static string Get(string key)
    {
        if (string.IsNullOrEmpty(key))
            return string.Empty;

        try
        {
            var context = Manager.CreateResourceContext();
            context.QualifierValues["Language"] = _code;
            var candidate = Manager.MainResourceMap.TryGetValue("Resources/" + key, context);
            return candidate?.ValueAsString ?? key;
        }
        catch
        {
            return key;
        }
    }
}
