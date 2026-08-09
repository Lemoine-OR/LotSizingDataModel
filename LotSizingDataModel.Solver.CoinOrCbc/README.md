# LotSizingDataModel.Solver.CoinOrCbc

Optional COIN-OR CBC adapter for `LotSizingDataModel.Solver`.

The adapter uses the standalone `cbc` executable at runtime and has no native-DLL compile-time dependency. This keeps the managed adapter buildable on public CI even when CBC is not installed.

Runtime discovery checks `LOTSIZING_CBC_EXECUTABLE`, `CBC_HOME`, `COINOR_HOME`, and `PATH`. The CBC runtime is installed/distributed separately from the managed adapter.
