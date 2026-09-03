# LSI literature mapping

This document records the conceptual mapping used to keep LSI compatible with
major lot-sizing classification traditions.

| Literature tradition | LSI location |
|---|---|
| Graham-style environment/characteristics/objective | `alpha`, `beta`, `gamma` |
| Bitran-Yanasse parameter profiles | temporal profile on `beta` entries |
| single-item dynamic lot sizing | `alpha.I=1`, `alpha.LV=1`, capacity and beta qualifiers |
| CLSP | multi-item/single-level + production capacity |
| MLLP | multi-item/multi-level + no production capacity |
| MLCLSP | multi-item/multi-level + production capacity |
| big-bucket/small-bucket distinction | `pi.BK` |
| integrated lot-sizing/scheduling extensions | `beta.SCH.*` vocabulary |
| multiobjective planning | `gamma` aggregation/components when explicitly modeled |
| benchmark dimensions | `sigma` |

## Principle

LSI does not attempt to replace historical terminology. It provides a
machine-readable semantic superset from which historical labels can be derived
when their assumptions are satisfied.
