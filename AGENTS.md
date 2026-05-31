# Repository Guidelines

## Project Structure & Module Organization

`RiskFlow.slnx` contains three projects under `src/`. `RiskFlow.App` is the WinUI 3 app with XAML views, ViewModels, services, localization, and app assets. `RiskFlow.Core` is pure `net10.0` domain code for analyses, risks, categories, levels, and matrix scoring. `RiskFlow.Data` contains EF Core SQLite persistence, migrations, seeding, and the design-time DbContext factory. Shared branding assets live in `assets/`; installer packaging is in `installer/`; GitHub Actions are in `.github/workflows/`. Generated outputs such as `publish/`, `installer-output/`, `bin/`, `obj/`, local databases, and certificates are ignored.

## Build, Test, and Development Commands

Always pass `-p:Platform=x64` for local build/run commands because the solution declares explicit Windows platforms and no `AnyCPU`.

```powershell
dotnet tool restore
dotnet build -p:Platform=x64
dotnet run --project src/RiskFlow.App/RiskFlow.csproj -p:Platform=x64
dotnet dotnet-ef migrations add <Name> --project src/RiskFlow.Data/RiskFlow.Data.csproj
dotnet publish src/RiskFlow.App/RiskFlow.csproj -c Release -r win-x64 -p:Platform=x64 -o publish
```

`dotnet tool restore` installs the pinned `dotnet-ef` tool. CI builds `src/RiskFlow.App/RiskFlow.csproj` in Release x64. Publish creates the self-contained Windows app folder used by Inno Setup.

## Coding Style & Naming Conventions

Use standard C# formatting with 4-space indentation, nullable reference types, and implicit usings. Use `PascalCase` for types, public members, and XAML resource keys; `camelCase` for locals and private fields when no existing pattern differs; append `Async` to asynchronous methods. ViewModels use CommunityToolkit.Mvvm generators such as `[ObservableProperty]` and `[RelayCommand]`. Keep `RiskFlow.Core` free of WinUI and EF dependencies. User-facing text belongs in both `.resw` localization files; keep JSON references in sync when practical.

## Testing Guidelines

There is currently no test project and no coverage gate. When adding tests, place them under `tests/` with project names such as `RiskFlow.Core.Tests`, prefer fast unit tests for matrix scoring and ViewModel behavior, and name tests by expected behavior. Until tests exist, validate with `dotnet build -p:Platform=x64` and focused manual checks.

## Commit & Pull Request Guidelines

Recent history uses Conventional Commit prefixes such as `feat:` and `ci:` with concise, imperative summaries. Follow that style, for example `feat: add matrix export summary`. Pull requests should describe the change, list validation performed, link related issues, and include screenshots or short recordings for UI changes. Note migrations, localization additions, installer changes, or release/signing impacts explicitly.

## Security & Configuration Tips

Do not commit `.pfx` files, local SQLite databases, or generated publish artifacts. Code signing is optional in CI and controlled by `PFX_BASE64` and `PFX_PASSWORD` repository secrets. Keep trimming disabled for WinUI publish builds.
