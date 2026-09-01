# LotSizingDataModel consumer reference profiles — alpha.43

Recommended direct project/package references:

| Consumer need | Direct LotSizingDataModel assemblies |
| --- | --- |
| Data model only | Core, Instance, Solution |
| Optimization | Core, Instance, Solution, Solver, selected backend |
| Independent verification | Core, Instance, Solution, Solver, Checker |
| Verification/benchmark campaigns | Core, Instance, Solution, Solver, Checker, Checker.Campaign |

Consumers should not reference vendored implementation artifacts directly.

A UI application and MLLPAlgorithm are downstream consumers and must preserve
the dependency direction toward LotSizingDataModel.
