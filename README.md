<p align="center">
  <img src="docs/assets/doxygen-project-logo.png" alt="LotSizingDataModel" width="360">
</p>

<h1 align="center">LotSizingDataModel</h1>

<p align="center">
  <strong>A modular .NET framework for modeling, solving, validating, and exchanging lot-sizing problems.</strong>
</p>

<p align="center">
  <a href="https://github.com/Lemoine-OR/LotSizingDataModel/actions/workflows/build.yml">
    <img src="https://github.com/Lemoine-OR/LotSizingDataModel/actions/workflows/build.yml/badge.svg?branch=main" alt="Build and Test">
  </a>
  <a href="https://github.com/Lemoine-OR/LotSizingDataModel/actions/workflows/documentation.yml">
    <img src="https://github.com/Lemoine-OR/LotSizingDataModel/actions/workflows/documentation.yml/badge.svg?branch=main" alt="Documentation">
  </a>
  <a href="https://github.com/Lemoine-OR/LotSizingDataModel/releases/latest">
    <img src="https://img.shields.io/github/v/release/Lemoine-OR/LotSizingDataModel?display_name=tag&sort=semver&label=release" alt="Latest release">
  </a>
  <img src="https://img.shields.io/badge/.NET-10.0-512BD4?logo=dotnet&logoColor=white" alt=".NET 10">
  <img src="https://img.shields.io/badge/C%23-239120?logo=csharp&logoColor=white" alt="C#">
</p>

<p align="center">
  <a href="https://github.com/Lemoine-OR/LotSizingDataModel/releases/latest">
    <img src="https://img.shields.io/badge/DOWNLOAD-Latest%20validated%20release-2EA44F?style=for-the-badge&logo=github&logoColor=white" alt="Download latest validated release">
  </a>
  <a href="https://lemoine-or.github.io/LotSizingDataModel/">
    <img src="https://img.shields.io/badge/DOCUMENTATION-Open%20Doxygen%20portal-0969DA?style=for-the-badge&logo=readthedocs&logoColor=white" alt="Open documentation">
  </a>
</p>

<p align="center">
  <a href="https://github.com/Lemoine-OR/LotSizingDataModel/releases">All releases</a>
  ·
  <a href="https://lemoine-or.github.io/LotSizingDataModel/">API documentation</a>
  ·
  <a href="https://github.com/Lemoine-OR/LotSizingDataModel/actions">CI / CD</a>
  ·
  <a href="https://github.com/Lemoine-OR/LotSizingDataModel/issues">Issues</a>
</p>

<p align="center">
  <img src="docs/assets/project-hero.webp" alt="LotSizingDataModel — production planning and lot-sizing" width="900">
</p>

---

## Overview

**LotSizingDataModel** is a modular C#/.NET framework for research and software development around **lot-sizing and production-planning problems**.

The repository separates the data model, problem instances, solution representation, solver abstractions, solver adapters, validation, import/conversion tools, command-line applications, tests, and documentation into dedicated projects.

The architecture is designed so that algorithms and mathematical solvers can share the same domain model and solution representation while remaining independently testable and replaceable.

Developed and maintained by **David Lemoine — Lemoine-OR**.

## Repository architecture

| Component | Purpose |
|---|---|
| `LotSizingDataModel.Core` | Core domain model for lot-sizing and supply-chain data |
| `LotSizingDataModel.Instance` | Problem instances, descriptors, and structural characterization |
| `LotSizingDataModel.Solution` | Solution representation and solution-method metadata |
| `LotSizingDataModel.Solver` | Solver abstractions and common solver contracts |
| `LotSizingDataModel.Solver.Cplex` | IBM ILOG CPLEX solver adapter |
| `LotSizingDataModel.Solver.Console` | Console application for solver execution |
| `LotSizingDataModel.Checker` | Generic solution and feasibility checker |
| `LotSizingDataModel.Checker.Cli` | Command-line checker |
| `LotSizingDataModel.Import` | Import infrastructure |
| `LotSizingDataModel.BatchConverter` | Batch conversion utility |
| `LotSizingDataModel.Checker.Campaign` | Checker campaign tooling |
| `LotSizingDataModel.Checker.Tests` | Automated checker tests |
| `LotSizingDataModel.Solver.Test` | Solver test project |

The public documentation portal focuses on the user-facing libraries and applications while test and campaign projects remain internal to the build/documentation pipeline.

## Documentation

The continuously updated API documentation is published with Doxygen:

### [Open the LotSizingDataModel documentation portal](https://lemoine-or.github.io/LotSizingDataModel/)

Documentation is rebuilt automatically from `main`, validated for broken local links, and published through GitHub Pages.

Each GitHub Release also contains a **version-frozen documentation archive**, so the documentation distributed with a release always remains available independently of the evolving `main` branch.

## Download

The recommended public distribution is the **latest validated GitHub Release**:

### [Download the latest LotSizingDataModel release](https://github.com/Lemoine-OR/LotSizingDataModel/releases/latest)

A release contains the validated binary package together with documentation and reproducibility metadata:

```text
LotSizingDataModel-<version>-validated.zip
LotSizingDataModel-<version>-documentation.zip
LotSizingDataModel-<version>-build-info.json
LotSizingDataModel-<version>-solver-adapters.json
LotSizingDataModel-<version>-release-manifest.json
SHA256SUMS.txt
```

`validated.zip` is the main binary distribution. `SHA256SUMS.txt` can be used to verify the integrity of the published assets.

## Continuous validation

Every push and pull request to `main` runs the automated validation pipeline:

```text
versioning
    ↓
restore / build
    ↓
automated tests
    ↓
solver-adapter discovery
    ↓
DLL metadata and version validation
    ↓
validated binary package
```

Documentation is handled by a parallel pipeline:

```text
Doxygen 1.17
    ↓
dynamic project discovery
    ↓
version injection
    ↓
HTML generation
    ↓
local-link validation
    ↓
GitHub Pages
```

The release workflow reruns the complete build, tests, binary checks, and documentation generation before creating a public Git tag and GitHub Release.

## Build from source

### Requirements

For the validated library/test build:

- .NET 10 SDK
- PowerShell

Run:

```powershell
git clone https://github.com/Lemoine-OR/LotSizingDataModel.git
cd LotSizingDataModel

powershell -ExecutionPolicy Bypass `
  -File ".\build\Build-Validated.ps1"
```

For the complete local build including documentation, Graphviz and Doxygen are also required:

```powershell
powershell -ExecutionPolicy Bypass `
  -File ".\build\Build-All.ps1"
```

Generated test results, validated packages, and documentation are placed under `Documentation/`, which is excluded from source control.

## Solver adapters

Solver adapters follow the naming convention:

```text
LotSizingDataModel.Solver.<SolverName>
```

The automation discovers compatible solver-adapter projects dynamically. A future public adapter that can build on the standard GitHub runner can therefore participate automatically in:

- CI build validation;
- binary packaging;
- Doxygen documentation;
- GitHub Releases.

Adapters requiring proprietary or external SDK installations are controlled through:

```text
build/solver-build-profiles.json
```

For example, the CPLEX adapter source and documentation can remain public while IBM proprietary runtime binaries are not redistributed by this repository.

When the required external SDK is installed locally, the complete build can include external adapters with:

```powershell
powershell -ExecutionPolicy Bypass `
  -File ".\build\Build-All.ps1" `
  -IncludeExternalSolverAdapters
```

## Versioning

The repository uses **Nerdbank.GitVersioning** with a single root:

```text
version.json
```

All assemblies therefore share one repository-wide version identity.

The automated version is propagated to:

- assembly version metadata;
- file/product version metadata;
- validated packages;
- Doxygen documentation;
- release manifests;
- Git tags;
- GitHub Releases.

This keeps a published release traceable to the exact Git commit from which it was produced.

## Release integrity and provenance

Public releases are generated only through the automated release workflow.

Before publication, the pipeline verifies:

- the public NBGV version;
- the exact Git commit;
- first-party DLL versions;
- assembly company/product metadata;
- embedded Win32 icon resources;
- solver-adapter manifest;
- Doxygen documentation;
- documentation links;
- ZIP readability and expected contents;
- SHA-256 checksums.

The release manifest provides a machine-readable record of the resulting distribution.

## Project links

| Resource | Link |
|---|---|
| Latest release | [github.com/Lemoine-OR/LotSizingDataModel/releases/latest](https://github.com/Lemoine-OR/LotSizingDataModel/releases/latest) |
| All releases | [github.com/Lemoine-OR/LotSizingDataModel/releases](https://github.com/Lemoine-OR/LotSizingDataModel/releases) |
| Documentation | [lemoine-or.github.io/LotSizingDataModel](https://lemoine-or.github.io/LotSizingDataModel/) |
| Build & test workflow | [Actions / Build and Test](https://github.com/Lemoine-OR/LotSizingDataModel/actions/workflows/build.yml) |
| Documentation workflow | [Actions / Build Documentation](https://github.com/Lemoine-OR/LotSizingDataModel/actions/workflows/documentation.yml) |
| Release workflow | [Actions / Create Release](https://github.com/Lemoine-OR/LotSizingDataModel/actions/workflows/release.yml) |
| Issues | [github.com/Lemoine-OR/LotSizingDataModel/issues](https://github.com/Lemoine-OR/LotSizingDataModel/issues) |

---

<p align="center">
  <strong>LotSizingDataModel</strong><br>
  Modeling · Optimization · Validation · Reproducible Releases
</p>

<p align="center">
  David Lemoine · Lemoine-OR
</p>
