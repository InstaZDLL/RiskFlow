<div align="center">

<img src="assets/logo.png" alt="RiskFlow logo" width="128" />

# RiskFlow

**Native Windows app for project risk analysis and mapping.**

[![.NET 10](https://img.shields.io/badge/.NET-10-512BD4?logo=dotnet&logoColor=white&style=for-the-badge)](https://dotnet.microsoft.com/)
[![WinUI 3](https://img.shields.io/badge/WinUI-3-0078D4?logo=windows&logoColor=white&style=for-the-badge)](https://learn.microsoft.com/windows/apps/winui/winui3/)
[![Windows](https://img.shields.io/badge/Windows-10%2F11-0078D4?logo=windows11&logoColor=white&style=for-the-badge)](#)
[![EF Core + SQLite](https://img.shields.io/badge/EF%20Core-SQLite-003B57?logo=sqlite&logoColor=white&style=for-the-badge)](https://learn.microsoft.com/ef/core/)
[![C#](https://img.shields.io/badge/C%23-13-239120?logo=csharp&logoColor=white&style=for-the-badge)](#)
[![Status](https://img.shields.io/badge/status-stable-brightgreen?style=for-the-badge)](#)
[![Latest release](https://img.shields.io/github/v/release/InstaZDLL/RiskFlow?style=for-the-badge)](https://github.com/InstaZDLL/RiskFlow/releases/latest)
[![GitHub Downloads (all assets, all releases)](https://img.shields.io/github/downloads/InstaZDLL/RiskFlow/total?style=for-the-badge)](https://github.com/InstaZDLL/RiskFlow/releases)
[![License: Apache 2.0](https://img.shields.io/badge/License-Apache%202.0-blue?style=for-the-badge)](LICENSE)

</div>

---

## Overview

**RiskFlow** is a **fully self-contained** Windows desktop application (the .NET runtime and
the Windows App SDK are bundled — no prior installation required) dedicated to **project risk
analysis**. It reimagines and extends the risk-analysis module of
[TPI-Flow](https://tpiflow.ch) as a native, offline, local executable.

Each risk is assessed **before** and **after** mitigation against a *Severity × Likelihood*
matrix, which automatically derives a level (**Low / Medium / High / Critical**). The UI is in
French (the app targets Swiss TPI projects).

## Features

- 🗂️ **Multiple analyses** — manage several risk registers and switch between them from a
  collapsible sidebar
- 📐 **Predefined matrix models** — 3×4 (TPI-Flow table), 4×4 and 5×5 (multiplicative scoring)
- 📋 **Risk register** grouped by category (Functional, Technical, Security, Organizational,
  Quality, Compliance, Project)
- ✏️ **Detail panel** to edit a risk with **live level recalculation** (before/after mitigation,
  mitigation strategy, "can continue" blocker)
- 🟩🟥 **Visual risk matrix** (Severity × Likelihood) with risk numbers or counts per cell, and a
  before/after toggle
- 📊 **Summary cards** — total, critical, high and non-continuable risks
- 📄 **PDF export** (QuestPDF) — header, detailed table and before/after matrices
- ⚙️ **Settings** — theme (system/light/dark), matrix display options, report identity, category
  management
- 💾 **Local storage** with SQLite (`%LOCALAPPDATA%\RiskFlow\riskflow.db`)

## Tech stack

| Layer | Technology |
|-------|------------|
| UI | WinUI 3 (Windows App SDK) + MVVM (CommunityToolkit.Mvvm) |
| Dependency injection | Microsoft.Extensions.DependencyInjection |
| Runtime | .NET 10 — `net10.0-windows10.0.19041.0` |
| Data | EF Core + SQLite |
| PDF | QuestPDF |
| Packaging | **Unpackaged** executable (`.exe`), **bundled** runtime (self-contained) |

## Repository layout

```text
RiskFlow.slnx                Solution (3 projects)
src/
  RiskFlow.App/              WinUI 3 application (UI, ViewModels, DI)
  RiskFlow.Core/             Domain (enums, matrix models, entities) — pure net10.0
  RiskFlow.Data/             EF Core + SQLite (DbContext, migrations, seed)
```

`RiskFlow.Core` depends on neither WinUI nor EF Core: the business logic (notably
`RiskMatrixModels`, a faithful port of `risk-logic.ts`) stays testable in isolation.

## Requirements

- Windows 10 (1809+) or Windows 11
- [.NET SDK 10](https://dotnet.microsoft.com/download/dotnet/10.0)

## Development

```powershell
# Restore tools (dotnet-ef) and build
dotnet tool restore
dotnet build -p:Platform=x64

# Run the app
dotnet run --project src/RiskFlow.App/RiskFlow.csproj -p:Platform=x64
```

### Database (EF Core)

```powershell
# Add a migration
dotnet dotnet-ef migrations add <Name> --project src/RiskFlow.Data/RiskFlow.Data.csproj
```

Migrations are applied automatically at startup (`DbInitializer`).

## Publishing (standalone executable)

Produces an xcopy-deployable folder containing `RiskFlow.exe` and the whole runtime:

```powershell
dotnet publish src/RiskFlow.App/RiskFlow.csproj -c Release -r win-x64 -p:Platform=x64 -o publish
# → publish/RiskFlow.exe (self-contained, ~330 MB)
```

### Installer

An [Inno Setup](https://jrsoftware.org/isinfo.php) script (`installer/RiskFlow.iss`) wraps the
published folder into `RiskFlow-Setup.exe`:

```powershell
& "${env:ProgramFiles(x86)}\Inno Setup 6\ISCC.exe" /DMyAppVersion=1.0.0 installer\RiskFlow.iss
# → installer-output/RiskFlow-Setup.exe
```

## CI/CD

- **CI** (`.github/workflows/ci.yml`) builds the app on every push/PR to `main`.
- **Release** (`.github/workflows/release.yml`) runs on a `v*` tag (or manually): publishes the
  self-contained app, optionally signs it, builds the installer, and attaches `RiskFlow-Setup.exe`
  to a GitHub Release.

Trigger a release:

```powershell
git tag v1.0.0
git push origin v1.0.0
```

### Code signing (optional)

To sign `RiskFlow.exe` and the installer, add two repository secrets — the build skips signing if
they are absent:

| Secret | Value |
|--------|-------|
| `PFX_BASE64` | the `.pfx` certificate, Base64-encoded |
| `PFX_PASSWORD` | the certificate password |

Encode and upload the certificate (PowerShell + GitHub CLI):

```powershell
$b64 = [Convert]::ToBase64String([IO.File]::ReadAllBytes("path\to\cert.pfx"))
$b64 | gh secret set PFX_BASE64 --repo InstaZDLL/RiskFlow
gh secret set PFX_PASSWORD --repo InstaZDLL/RiskFlow   # prompts for the password
```

## License

Copyright 2026 InstaZDLL

Licensed under the Apache License, Version 2.0 (the "License");
you may not use this file except in compliance with the License.
You may obtain a copy of the License at

    http://www.apache.org/licenses/LICENSE-2.0

Unless required by applicable law or agreed to in writing, software
distributed under the License is distributed on an "AS IS" BASIS,
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
See the License for the specific language governing permissions and
limitations under the License.

See the [LICENSE](LICENSE) file for the full text.
