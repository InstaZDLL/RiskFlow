<div align="center">

<img src="assets/logo.png" alt="Logo RiskFlow" width="128" />

# RiskFlow

**Analyse et cartographie des risques projet — application Windows native.**

[![.NET 10](https://img.shields.io/badge/.NET-10-512BD4?logo=dotnet&logoColor=white)](https://dotnet.microsoft.com/)
[![WinUI 3](https://img.shields.io/badge/WinUI-3-0078D4?logo=windows&logoColor=white)](https://learn.microsoft.com/windows/apps/winui/winui3/)
[![Windows](https://img.shields.io/badge/Windows-10%2F11-0078D4?logo=windows11&logoColor=white)](#)
[![EF Core + SQLite](https://img.shields.io/badge/EF%20Core-SQLite-003B57?logo=sqlite&logoColor=white)](https://learn.microsoft.com/ef/core/)
[![C#](https://img.shields.io/badge/C%23-13-239120?logo=csharp&logoColor=white)](#)
[![Statut](https://img.shields.io/badge/statut-en%20d%C3%A9veloppement-orange)](#)

</div>

---

## Présentation

**RiskFlow** est une application de bureau Windows **100 % autonome** (runtime .NET et Windows
App SDK embarqués : aucune installation préalable requise) dédiée à l'**analyse des risques
projet**. Elle reprend et étend le module d'analyse des risques de
[TPI-Flow](https://tpiflow.ch) sous la forme d'un exécutable natif, hors-ligne et local.

Chaque risque est évalué **avant** et **après** mitigation selon une matrice
*Gravité × Probabilité*, qui en déduit automatiquement un niveau
(**Bas / Moyen / Élevé / Critique**).

## Fonctionnalités

- 📋 **Registre des risques** classés par catégorie (Fonctionnel, Technique, Sécurité,
  Organisationnel, Qualité, Conformité/LPD, Projet)
- 🎯 **Double évaluation** avant / après mitigation (gravité × probabilité → niveau calculé)
- 🟩🟥 **Niveaux de risque** colorés, calculés via la même matrice que TPI-Flow
- 💾 **Stockage local** SQLite (`%LOCALAPPDATA%\RiskFlow\riskflow.db`)
- 🔌 **Interopérabilité** prévue avec l'import/export de TPI-Flow

> 🚧 En cours de développement. Déjà disponible : registre des risques (liste, ajout,
> suppression) avec calcul automatique des niveaux avant/après.

## Stack technique

| Couche | Technologie |
|--------|-------------|
| Interface | WinUI 3 (Windows App SDK) + MVVM (CommunityToolkit.Mvvm) |
| Injection de dépendances | Microsoft.Extensions.DependencyInjection |
| Runtime | .NET 10 — `net10.0-windows10.0.19041.0` |
| Données | EF Core + SQLite |
| Packaging | Exécutable **non packagé** (`.exe`), runtime **bundlé** (self-contained) |

## Structure du dépôt

```text
RiskFlow.slnx                Solution (3 projets)
src/
  RiskFlow.App/              Application WinUI 3 (UI, ViewModels, DI)
  RiskFlow.Core/             Domaine métier (enums, matrice, entités) — net10.0 pur
  RiskFlow.Data/             EF Core + SQLite (DbContext, migrations, seed)
```

`RiskFlow.Core` ne dépend ni de WinUI ni d'EF Core : la logique métier (notamment
`RiskCalculator`, port fidèle de `risk-logic.ts`) reste testable de façon isolée.

## Prérequis

- Windows 10 (1809+) ou Windows 11
- [.NET SDK 10](https://dotnet.microsoft.com/download/dotnet/10.0)

## Développement

```powershell
# Restaurer les outils (dotnet-ef) et compiler
dotnet tool restore
dotnet build -p:Platform=x64

# Lancer l'application
dotnet run --project src/RiskFlow.App/RiskFlow.csproj -p:Platform=x64
```

### Base de données (EF Core)

```powershell
# Ajouter une migration
dotnet dotnet-ef migrations add <Nom> --project src/RiskFlow.Data/RiskFlow.Data.csproj
```

Les migrations sont appliquées automatiquement au démarrage (`DbInitializer`).

## Publication (exécutable autonome)

Produit un dossier xcopy-déployable contenant `RiskFlow.exe` et tout le runtime :

```powershell
dotnet publish src/RiskFlow.App/RiskFlow.csproj -c Release -r win-x64 -p:Platform=x64
# → bin/x64/Release/net10.0-windows10.0.19041.0/win-x64/publish/RiskFlow.exe
```

## Licence

À définir.
