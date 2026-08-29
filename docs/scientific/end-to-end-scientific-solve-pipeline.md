# End-to-End Scientific Solve Pipeline — alpha.19

`ScientificLotSizingSolvePipeline` is the first orchestration layer that closes
the current scientific propagation chain around an actual solve.

Workflow:

1. validate the normalized `SolverRequest`;
2. run `ScientificClassificationEngine` on the request instance;
3. run scientific formulation selection;
4. require the selected formulation's technical `CanBuild(instance)`;
5. create a separate delegated `SolverRequest` with the scientific formulation
   ID pinned into `FormulationName`;
6. delegate all actual model building, solver execution and normalized solution
   mapping to the existing `ILotSizingSolverService`;
7. verify that `SolverRunResult.FormulationName` equals the scientifically
   selected formulation;
8. when a normalized solution exists, capture scientific solution provenance;
9. optionally run the existing independent numerical solution verification;
10. optionally run independent scientific provenance verification.

The pipeline does not reimplement mathematical model construction or native
solver execution.

## Critical formulation identity invariant

For an automatic scientific solve:

`ScientificSelectedFormulationId == DelegatedSolverRequest.FormulationName`

and after execution:

`ScientificSelectedFormulationId == SolverRunResult.FormulationName`

A mismatch after execution is `FormulationDrift` and scientific provenance is
not captured.

## Caller request immutability

The caller's `SolverRequest.FormulationName` is not modified.

A delegated request is created. This matters especially when the caller used
an empty formulation name to request automatic selection.

Solver parameters are reused by reference because the scientific wrapper does
not mutate them; progress observers are copied to the delegated request.

## Existing technical workflow retained

`LotSizingSolverService` already:
- selects/builds the mathematical formulation;
- selects/executes a solver adapter;
- maps the mathematical solution to `LotSizingSolution`;
- independently recomputes the mathematical objective.

alpha.19 constrains that pipeline scientifically rather than replacing it.
