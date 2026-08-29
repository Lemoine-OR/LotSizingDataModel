# Problem-Class Alias Policy

Historical and literature acronyms are not guaranteed to identify one unique
canonical class.

## Ambiguous aliases

`ULS` and `ULSP` are deliberately ambiguous between:

- `SI-ULS`;
- `MI-ULS`.

The literature contains both explicitly single-item usage and formulations
indexed over multiple items.

`CLSP` is deliberately ambiguous between:

- `SI-CLSP`;
- `MI-CLSP`.

Single-item CLSP is a recognized polyhedral/modeling subproblem, while a very
common production-planning use of CLSP is the multi-item shared-capacity
problem.

`MLLS` is capacity-ambiguous between:

- `UMLSP`;
- `MLCLSP`.

The alias resolver therefore returns:

- `Unknown`;
- `Unique`;
- `Ambiguous`.

It never silently picks the first matching acronym.

## Unambiguous project aliases

`MLLP` resolves to `UMLSP` under the LotSizingDataModel/MLLPAlgorithm project
convention.

`MCLSP` resolves to `MI-CLSP`.

`CSILSP` resolves to `SI-CLSP`.

## Paradigms are excluded

`MRP` and `DRP` resolve to `Unknown` in the problem-class alias resolver
because they belong to `PlanningParadigm`, not `LotSizingProblemClass`.
