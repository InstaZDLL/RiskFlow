using System;
using System.IO;

namespace RiskFlow.Services;

/// <summary>Emplacements de fichiers de l'application (base de données locale).</summary>
public static class AppPaths
{
    /// <summary>Dossier de données : %LOCALAPPDATA%\RiskFlow (créé si absent).</summary>
    public static string DataDirectory
    {
        get
        {
            var dir = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "RiskFlow");
            Directory.CreateDirectory(dir);
            return dir;
        }
    }

    /// <summary>Chemin du fichier SQLite.</summary>
    public static string DatabasePath => Path.Combine(DataDirectory, "riskflow.db");

    /// <summary>Chaîne de connexion SQLite vers la base locale.</summary>
    public static string ConnectionString => $"Data Source={DatabasePath}";
}
