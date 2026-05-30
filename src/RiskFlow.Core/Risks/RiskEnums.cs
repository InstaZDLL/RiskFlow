namespace RiskFlow.Core.Risks;

/// <summary>Gravité d'un risque (ordre croissant). Miroir de SEVERITY_OPTIONS (TPI-Flow).</summary>
public enum Severity
{
    Acceptable = 0,
    Tolerable = 1,
    Undesirable = 2,
    Intolerable = 3,
}

/// <summary>Probabilité d'occurrence (ordre croissant). Miroir de LIKELIHOOD_OPTIONS.</summary>
public enum Likelihood
{
    Improbable = 0,
    Possible = 1,
    Probable = 2,
}

/// <summary>Niveau de risque résultant. Miroir de RISK_LEVELS.</summary>
public enum RiskLevel
{
    Low = 0,
    Medium = 1,
    High = 2,
    Extreme = 3,
}
