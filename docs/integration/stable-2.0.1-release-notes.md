# LotSizingDataModel 2.0.1

This patch corrects Xpress objective reporting and validates the adapter with FICO Xpress 9.9 / Optimizer 47.01.01. It includes the transport evolution from 2.0.0 and does not change its public data model or XML format.

## Xpress corrections

A pure LP with an optimal objective of 8 previously returned the unset MIP objective sentinel `1E+40`. The adapter now selects LP or MIP objective attributes according to the mathematical model and preserves native MIP bounds. Infeasible and unbounded results no longer expose an incumbent solution or objective merely because Xpress returned a numeric array.

The adapter recognizes the official `xprsdn` .NET SDK assembly in addition to legacy `Optimizer`, and reads the native version through `XPRS.GetVersion`.

## Validation

- Complete 18-project Release build: zero warnings and errors.
- 464 automated tests passed: Core 40, Instance 208, Checker 216; none failed or skipped.
- Seven native Xpress scenarios passed: shared transport capacity/cost (objectives 19 and 37 plus an infeasible case), LP minimization/maximization and column mapping (8 and 12), integer/binary optimization (6), and unbounded status.
- Feasible native solutions were independently checked against bounds, integrality, constraints and the objective.
- Three native CPLEX transport regressions passed.

The optional native suite is available through `dotnet run --project LotSizingDataModel.Solver.Test -c Release -- --xpress-smoke`. It requires the official SDK, native runtime and a valid local license. Proprietary runtimes and license material are not distributed. These small deterministic tests are not performance benchmarks or exhaustive coverage of interruption/time-limit behavior.

## Upgrade and distribution

Replace the 2.0.0 validated binary distribution with 2.0.1 to receive the Xpress corrections. No data migration is needed when upgrading from 2.0.0. Users upgrading from 1.x should also read the [2.0.0 migration notes](https://github.com/Lemoine-OR/LotSizingDataModel/blob/v2.0.0/docs/integration/stable-2.0.0-release-notes.md).

The official workflow builds and validates the merged main commit, then publishes binaries, documentation, metadata/manifests and SHA-256 checksums. The existing v2.0.0 tag and assets remain unchanged. This is a GitHub Release publication, not a NuGet.org publication.
