# Scientific Resolution Plan — alpha.20

alpha.20 introduces an explicit fourth scientific/technical chain:

`problem class -> mathematical formulation -> solution method -> solver backend`

These concepts remain distinct.

## Current executable method

`MILP-GENERAL`

Category:

`MixedIntegerLinearProgramming`

This is the only method family currently connected end-to-end to
LotSizingDataModel.

It applies to the six executable canonical lot-sizing core classes because the
standard scientific formulation profile covers them and the current Solver
stack executes mixed-integer linear models.

## Catalog-only method families

The scientific catalog also records:

- `DP-SI-ULS`: specialized dynamic programming;
- `SP-SI-ULS`: shortest-path/network exact method;
- `LR-CLSP`: Lagrangian relaxation;
- `DW-BP-CLSP`: Dantzig-Wolfe / branch-and-price;
- `HEURISTIC-GENERAL`;
- `METAHEURISTIC-GENERAL`;
- `MATHEURISTIC-GENERAL`.

`CatalogOnly` means the family is scientifically relevant but has no executable
adapter in this repository yet.

It does **not** mean every concrete algorithm in that family supports every
extension of the listed canonical problem class.

Future integrations with ULSAlgorithm, MLLPAlgorithm and
MetaheuristicsPlatform must add concrete algorithm-level applicability
contracts before changing support to Executable.

## Literature basis

Single-item lot-sizing reviews distinguish exact dynamic-programming and
mathematical-programming approaches.

Dynamic lot-sizing solution reviews also distinguish dynamic programming,
cutting/decomposition approaches, Lagrangian relaxation, dedicated heuristics
and metaheuristics.

The method catalog mirrors these algorithmic families without conflating them
with problem classes or formulations.
