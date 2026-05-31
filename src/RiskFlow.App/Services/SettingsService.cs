using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace RiskFlow.Services;

/// <summary>
/// Charge et persiste les préférences utilisateur dans settings.json
/// (dossier <see cref="AppPaths.DataDirectory"/>). Notifie les abonnés à chaque sauvegarde.
/// </summary>
public sealed class SettingsService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        Converters = { new JsonStringEnumConverter() },
    };

    private readonly string _path = Path.Combine(AppPaths.DataDirectory, "settings.json");

    public SettingsService()
    {
        Current = Load();
    }

    /// <summary>Préférences courantes (mutables ; appeler <see cref="Save"/> pour persister).</summary>
    public AppSettings Current { get; private set; }

    /// <summary>Déclenché après chaque sauvegarde (thème, options matrice…).</summary>
    public event Action? Changed;

    private AppSettings Load()
    {
        try
        {
            if (File.Exists(_path))
            {
                var json = File.ReadAllText(_path);
                var settings = JsonSerializer.Deserialize<AppSettings>(json, JsonOptions);
                if (settings is not null)
                    return settings;
            }
        }
        catch (Exception ex) when (ex is IOException or JsonException or UnauthorizedAccessException)
        {
            // Fichier illisible ou corrompu : on repart sur les valeurs par défaut.
        }

        return new AppSettings();
    }

    /// <summary>Persiste les préférences courantes et notifie les abonnés.</summary>
    public void Save()
    {
        File.WriteAllText(_path, JsonSerializer.Serialize(Current, JsonOptions));
        Changed?.Invoke();
    }
}
