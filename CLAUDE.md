# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project

RiskFlow is a native Windows desktop app (WinUI 3 / .NET 10) for project risk analysis. Each
risk is scored *before* and *after* mitigation against a *Severity × Likelihood* matrix that
derives a level (Low / Medium / High / Extreme). It is a native, offline port of the
risk-analysis module of TPI-Flow. The UI ships in French and English; data is stored locally
in SQLite at `%LOCALAPPDATA%\RiskFlow\riskflow.db`.

## Commands

All build/run commands **must** pass `-p:Platform=x64` (the projects declare `x86;x64;ARM64`
with no `AnyCPU`; omitting the platform fails to restore/build).

```powershell
dotnet tool restore                       # restores dotnet-ef (pinned in dotnet-tools.json)
dotnet build -p:Platform=x64
dotnet run --project src/RiskFlow.App/RiskFlow.csproj -p:Platform=x64

# EF Core migration (DesignTimeDbContextFactory provides the context out-of-host)
dotnet dotnet-ef migrations add <Name> --project src/RiskFlow.Data/RiskFlow.Data.csproj

# Self-contained publish (~330 MB folder with bundled .NET + Windows App SDK)
dotnet publish src/RiskFlow.App/RiskFlow.csproj -c Release -r win-x64 -p:Platform=x64 -o publish
```

CI (`.github/workflows/ci.yml`) only runs `dotnet build -c Release -p:Platform=x64`. **There is
no test project** — there is no test command to run.

## Architecture

Three projects, strictly layered so the domain stays UI- and persistence-free:

- **`RiskFlow.Core`** — pure `net10.0`, no WinUI/EF dependencies. Domain entities (`Analysis`,
  `Risk`, `RiskCategory`), enums (`RiskLevel`), and the risk-scoring logic. `RiskMatrixModels`
  is the heart: a frozen catalogue of three predefined matrices (`3x4` = explicit TPI-Flow grid;
  `4x4`/`5x5` = multiplicative scoring binned into four levels). Each `RiskMatrixModel`
  precomputes a `[severityIndex, likelihoodIndex] → RiskLevel` grid. Keep this layer testable
  in isolation — do not introduce framework dependencies here.
- **`RiskFlow.Data`** — EF Core + SQLite. `RiskFlowDbContext` (cascade delete from `Analysis` to
  `Risk`; categories are shared across analyses; `SaveChanges` stamps `CreatedAt`/`UpdatedAt`).
  `DbInitializer.InitializeAsync` runs migrations + seeds default categories and a first analysis
  at startup. `DesignTimeDbContextFactory` exists only so `dotnet-ef` can build the context.
- **`RiskFlow.App`** — WinUI 3 UI, MVVM (CommunityToolkit.Mvvm), services, localization, exports.

### App wiring

- DI is a hand-built `ServiceCollection` in `App.ConfigureServices()` (no Generic Host). The
  container is exposed as the static `App.Services`. ViewModels (`ShellViewModel`,
  `RisksViewModel`, `SettingsViewModel`) are **singletons**; `MainWindow`/`MainPage` are transient.
- The DbContext is registered via `AddDbContextFactory`. **Always** create a short-lived context
  per operation (`await using var db = await dbFactory.CreateDbContextAsync()`) — do not hold a
  long-lived context. This is the established pattern across all ViewModels.
- `App.OnLaunched` initializes the DB, calls `ShellViewModel.LoadAsync()`, then shows the window.
- `ShellViewModel` owns the analyses list + selection; selecting an analysis fire-and-forgets
  `RisksViewModel.SetAnalysisAsync`, which reloads rows for that analysis and its matrix model.
- `RiskRowViewModel` wraps a `Risk` entity and recomputes Before/After `RiskLevel` live as the
  severity/likelihood indices change. Edits stay in the row VM until `ApplyToModel()` copies them
  back to the entity before persistence (Save, export, reorder).

### Localization (important & non-obvious)

- **Runtime source of truth = the `.resw` files** (`Strings/{fr-FR,en-US}/Resources.resw`),
  embedded via `PRIResource` and read through the Windows App SDK `ResourceManager`
  (`Services/LanguageManager.Get(key)`).
- `Strings/{fr,en}.json` are **reference sources for regenerating the `.resw`** — they are *not*
  embedded and *not* read at runtime. There is no generation script; if you add a string, edit
  the `.resw` files (and keep the JSON in sync by hand).
- **Domain labels are stored as resource keys, not display text.** `RiskMatrixModels` axis labels
  (`"Sev_Acceptable"`, `"Lik_Possible"`, …) and `RiskLevel` names are keys; the App layer
  translates them via `RiskText` / `LanguageManager` / `LocalizeConverter` at display time. When
  adding levels or categories, add the corresponding key to both `.resw` files.
- Category names are persisted in their **French canonical form** (`"Fonctionnel"`, `"Sécurité"`,
  `"Conformité"`, …) and mapped to resource keys by `RiskText.CategoryKey`. `DbInitializer`
  migrates the legacy `"Conformité/LPD"` value to `"Conformité"`.
- Changing the language requires recreating the window; the app shows a "restart" toast rather
  than re-localizing live.

### Exports

`MainWindow.xaml.cs` handles file pickers (via `WinRT.Interop`) and delegates generation to
stateless services: `RiskReportPdf` (QuestPDF, Community license set in `App`), `RiskReportExcel`
(ClosedXML), and `AnalysisJson` (native import/export DTO `AnalysisExportDto`). All export
generators take a snapshot from `RisksViewModel.SnapshotForExport()` (which flushes pending row
edits first).

## Conventions

- Code comments and user-facing strings are in **French**; match the existing comment density and
  XML-doc style (`<summary>` on public types/members).
- C# 13, nullable enabled, implicit usings. ViewModels use CommunityToolkit source generators
  (`[ObservableProperty]` on `partial` properties, `[RelayCommand]`).
- Trimming is **disabled** (`PublishTrimmed=false`) — it breaks WinUI XAML/reflection. Do not
  re-enable it.
