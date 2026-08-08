# LotSizingDataModel — Automation Pack v1

This pack is merged at the repository root.

It establishes one automated build identity for the complete C# codebase:

- Git-based versioning with Nerdbank.GitVersioning 3.10.70.
- `David Lemoine` / `Lemoine-OR` assembly metadata.
- Common `LotSizingDataModel` product identity.
- Repository URL embedded in build metadata.
- Common branded Win32 icon from `docs/assets/dll-icon.ico`.
- Automatic XML documentation files.
- Automated test/build/package validation.
- Automatic discovery of new `LotSizingDataModel.Solver.*` adapter projects.
- Dynamic Doxygen project discovery and portal cards.
- Exhaustive local Doxygen-link validation.
- GitHub Actions using full Git history so local and CI builds derive the same version.

## IMPORTANT — first installation

Nerdbank.GitVersioning intentionally derives version height from Git history.
Therefore `version.json` must be committed before the first versioned build.

After merging this pack into `D:\Dev\LotSizingDataModel`:

1. Commit and push the automation files **before rebuilding**.
   Suggested commit message:

   `Add automated versioning and build identity`

2. Then run:

```powershell
cd "D:\Dev\LotSizingDataModel"
powershell -ExecutionPolicy Bypass -File ".\build\Build-All.ps1"
```

The build intentionally refuses to create misleading DLL versions when
`version.json` is uncommitted or modified but not committed.

## What changes automatically in Visual Studio?

Every SDK-style project below the repository root imports `Directory.Build.props`.

When Visual Studio builds any project, Nerdbank.GitVersioning stamps:

- AssemblyVersion
- FileVersion
- InformationalVersion
- Product/package version
- Git commit identity
- `LotSizingDataModel.Build.ThisAssembly`

The same central props also stamp:

- Company: `Lemoine-OR`
- Product: `LotSizingDataModel`
- Title: the Visual Studio project name
- Copyright containing `David Lemoine`
- repository metadata
- common Win32 icon

The icon is embedded in the PE resource of generated C# DLL/EXE files.
Windows Explorer reliably uses it for applications; for a `.dll`, Explorer may
still display the shell's generic DLL icon even though the resource is embedded.

## What happens when a new solver is added?

For a conventional project named:

`LotSizingDataModel.Solver.Highs`

nothing has to be added to either GitHub Actions workflow.

The automation will:

1. discover the project;
2. apply the common version and assembly identity;
3. build it in the public CI;
4. verify its generated DLL metadata/icon;
5. add the DLL/XML/PDB to the validated binary package;
6. discover it independently in Doxygen;
7. generate its Doxygen site;
8. add a new solver-adapter card to the documentation portal;
9. validate all local links before Pages deployment.

### Proprietary/external solver SDK exception

A solver adapter that requires a locally installed proprietary SDK cannot be
reliably built on a clean public GitHub runner without that SDK.

Declare only that exceptional project in:

`build/solver-build-profiles.json`

Example already included:

`LotSizingDataModel.Solver.Cplex`

CPLEX source and Doxygen documentation remain public, while its binary is skipped
by public CI. Locally, where IBM ILOG CPLEX is installed, use:

```powershell
powershell -ExecutionPolicy Bypass -File ".\build\Build-All.ps1" -IncludeExternalSolverAdapters
```

Future open/package-restorable adapters need no profile entry.

## Commands

Fast validated build (no Doxygen):

```powershell
powershell -ExecutionPolicy Bypass -File ".\build\Build-Validated.ps1"
```

Everything, including Doxygen:

```powershell
powershell -ExecutionPolicy Bypass -File ".\build\Build-All.ps1"
```

Everything including locally installed external/proprietary adapters:

```powershell
powershell -ExecutionPolicy Bypass -File ".\build\Build-All.ps1" -IncludeExternalSolverAdapters
```

Show the exact computed Git version:

```powershell
powershell -ExecutionPolicy Bypass -File ".\tools\Get-LotSizingVersion.ps1"
```

Generated outputs stay below the already-ignored `Documentation` directory:

- `Documentation/site`
- `Documentation/doxygen`
- `Documentation/test-results`
- `Documentation/artifacts`

## Version evolution

The repository starts with the version line `1.0`.

Git height supplies the automatically increasing build component, so commits
produce unique build/file identities without editing every `.csproj`.

When a functional version boundary is desired, change only root `version.json`
(for example from `1.0` to `1.1`) and commit that change. No project file needs
to be edited.

## GitHub Release layer

The validated ZIP and SHA-256 are already generated automatically under
`Documentation/artifacts`. A tag-driven GitHub Release workflow can therefore be
added later without redesigning versioning, documentation or solver discovery.
