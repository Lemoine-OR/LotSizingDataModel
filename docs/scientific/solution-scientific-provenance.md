# Scientific Solution Provenance — alpha.18

This increment propagates scientific classification/formulation evidence into
the Solution and Checker layers.

## Dependency-safe persistence

`LotSizingDataModel.Solution` must remain independent from Instance and Solver.

Scientific provenance is therefore a neutral snapshot persisted through the
already serializable `SolutionGenerationMetadata.Parameters` collection.

Reserved namespace:

`lsdm.scientific.*`

No mandatory XML element is introduced and legacy solution XML remains
readable.

## Snapshot content

Schema version 1 records:

- universal notation scheme ID/version;
- detected universal notation;
- canonical problem-class code;
- ExactCore / CompatibleExtension match kind;
- formulation ID;
- formulation scientific family;
- formulation scientific compatibility;
- capture timestamp.

## Capture contract

`ScientificSolutionProvenanceMapper` accepts only a successful
`ScientificFormulationSelectionResult`.

It requires:

- unblocked scientific classification;
- detected notation;
- unique primary canonical problem class;
- selected formulation with `Compatible` scientific status.

The solution never determines these values itself.

## Verification states

`SolutionScientificProvenanceChecker` returns:

- `Missing`;
- `Coherent`;
- `Stale`;
- `Incomplete`;
- `Contradiction`;
- `Invalid`.

Scientific traceability remains independent from numerical feasibility.
