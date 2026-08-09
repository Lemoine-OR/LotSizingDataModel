# LotSizingDataModel.Solver.Gurobi

Optional Gurobi adapter for `LotSizingDataModel.Solver`.

The project has **no compile-time dependency on the Gurobi SDK**. It uses the official `gurobi_cl` executable at runtime and can therefore be built by public CI runners on which Gurobi is not installed.

Runtime discovery checks `LOTSIZING_GUROBI_EXECUTABLE`, `GUROBI_HOME`, and `PATH`. The Gurobi executable, libraries and license files are not distributed by this repository.
