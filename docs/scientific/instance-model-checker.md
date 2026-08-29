# Instance Model Checker

`InstanceModelChecker` answers a different question from structural validation,
mathematical feasibility and solution validation.

| Service | Question |
|---|---|
| `SupplyChainValidator` | Are the domain data structurally and referentially consistent? |
| `InstanceModelChecker` | Does the instance represent a semantically meaningful lot-sizing model? |
| `InstanceFeasibilityAnalyzer` | Does at least one feasible solution exist? |
| `SolutionChecker` | Is a particular candidate solution valid? |

## Layering rule

`InstanceModelChecker` does not duplicate `SupplyChainValidator`. Structural
issues are composed into the result using their existing stable codes (`SC...`,
`BOM...`, etc.). New semantic rules use the `LSDM-SEM-...` namespace.

## Diagnostic contract

Each diagnostic exposes:

- `Code`
- `Severity`
- `Path`
- `Message`
- optional `RelatedPath`
- contextual `Values`
- optional `SuggestedAction`

The severity vocabulary is:

- `Information`
- `Warning`
- `Error`
- `Fatal`

Warnings do not block downstream operations. `Error` and `Fatal` diagnostics
block classification, universal-notation generation, solving and validated
export. Draft saving remains possible so a future MVVM editor can support
incomplete work.

## Initial semantic rules

- `LSDM-SEM-001`: missing stable instance identifier.
- `LSDM-SEM-010`: no external demand record.
- `LSDM-SEM-011`: demand records exist but all quantities are zero.
- `LSDM-SEM-020`: selected best-known-result identifier is dangling.

This is the foundation. Reachability, production/sourcing sufficiency,
descriptor consistency and model-specific semantic rules are added in later
increments as typed descriptors are introduced.
