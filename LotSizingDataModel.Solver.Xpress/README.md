# LotSizingDataModel.Solver.Xpress

Optional FICO Xpress MP adapter for `LotSizingDataModel.Solver`.

The project intentionally avoids a compile-time reference to the Xpress SDK so the public repository and CI remain buildable without a commercial runtime. Runtime discovery is performed only when the adapter is used.

Xpress libraries and license material are not distributed by this repository.

## Xpress 9.9 / Optimizer 47.01.01

The installed SDK provides `FICO.Xpress.XPRSdn` (47.1.1) in `C:/xpressmp/lib/nuget`.
Its managed assembly is `xprsdn.dll`; the adapter supports this identity as well as
legacy `Optimizer.dll`. Consumers can reference the official package, or set
`LOTSIZING_XPRESS_OPTIMIZER_ASSEMBLY` to the full path of its .NET Standard DLL.
Set `XPRESSDIR` to the installation directory and make the native `bin` directory
available on the process PATH. A valid local Xpress license is required.

Run the optional licensed regression suite with:

```powershell
dotnet run --project LotSizingDataModel.Solver.Test -c Release -- --xpress-smoke
```

It checks three shared-resource transport cases (objectives 19 and 37 and an
infeasible capacity case), LP minimization/maximization with column-name mapping,
integer/binary optimization, and unbounded status. Feasible results are independently
checked against variable bounds, integrality, constraints and the objective.
The license-free Checker tests additionally reproduce the LP/MIP objective attributes.

Native testing of the published 2.0.0 binaries passed the transport cases but exposed
an incorrect LP objective: the adapter read an unset MIP attribute (`1E+40`) instead
of the LP objective. This correction selects LP/MIP attributes by model type, omits
incumbent values for infeasible/unbounded results, and reports the SDK version.
The published 2.0.0 release is unchanged; this fix must be included in a subsequent release.
