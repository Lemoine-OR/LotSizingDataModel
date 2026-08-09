# Solver adapters — public release policy

The public repository distinguishes **LotSizingDataModel managed adapters** from **third-party solver runtimes**.

| Component | Public CI build | LotSizingDataModel release | Third-party runtime bundled |
|---|---:|---:|---:|
| `LotSizingDataModel.Solver.Gurobi.dll` | Yes | Yes | No |
| `LotSizingDataModel.Solver.Xpress.dll` | Yes | Yes | No |
| `LotSizingDataModel.Solver.CoinOrCbc.dll` | Yes | Yes | No |
| `LotSizingDataModel.Solver.Cplex.dll` | Existing repository policy | Existing repository policy | No |

Do not commit or publish Gurobi/Xpress/CPLEX license files or vendor binaries as LotSizingDataModel release assets. CBC can be obtained separately from COIN-OR; the managed adapter does not require it at compile time.

All three new adapter projects deliberately omit local `Version`, `AssemblyVersion`, and `FileVersion` properties so the repository-level `version.json` + `Directory.Build.props` / `Directory.Build.targets` versioning remains authoritative.
