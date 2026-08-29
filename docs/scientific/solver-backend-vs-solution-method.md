# Solver Backend vs Solution Method

A native optimization package is not a scientific solution method.

Current concrete backends:

- IBM ILOG CPLEX;
- Gurobi Optimizer;
- FICO Xpress;
- COIN-OR CBC.

These correspond to `SolverKind.Cplex`, `Gurobi`, `Xpress` and `CoinOrCbc`.

`SolverKind.Automatic` is a selection mode, not a backend.

For the current executable method:

`MILP-GENERAL`

all four concrete backends belong to the compatible backend catalog.

Scientific compatibility says that the backend class can execute the selected
method family. Actual installation/license/runtime availability remains the
responsibility of the existing solver discovery/selection layer.
