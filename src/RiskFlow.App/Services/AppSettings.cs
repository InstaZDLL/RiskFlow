namespace RiskFlow.Services;

/// <summary>Thème de l'application.</summary>
public enum ThemeMode { System, Light, Dark }

/// <summary>Langue de l'interface.</summary>
public enum AppLanguage { System, French, English }

/// <summary>Emplacement de la matrice dans l'écran d'analyse.</summary>
public enum MatrixPlacement { Tabs, BelowTable }

/// <summary>Contenu des cellules de la matrice.</summary>
public enum MatrixCellContent { Numbers, Count }

/// <summary>Évaluation représentée par défaut sur la matrice.</summary>
public enum MatrixEvaluation { Before, After }

/// <summary>Préférences utilisateur persistées (settings.json).</summary>
public sealed class AppSettings
{
    public ThemeMode Theme { get; set; } = ThemeMode.System;
    public AppLanguage Language { get; set; } = AppLanguage.System;

    public MatrixPlacement MatrixPlacement { get; set; } = MatrixPlacement.Tabs;
    public MatrixCellContent MatrixCellContent { get; set; } = MatrixCellContent.Numbers;
    public MatrixEvaluation MatrixDefaultEvaluation { get; set; } = MatrixEvaluation.Before;

    public string ReportAuthor { get; set; } = string.Empty;
    public string ReportOrganization { get; set; } = string.Empty;
}
