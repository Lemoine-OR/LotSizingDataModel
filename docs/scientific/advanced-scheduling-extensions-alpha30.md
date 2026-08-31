# Advanced Scheduling Extensions II — alpha.30

alpha.30 is the first macro-pack after the executable DLSP/CSLP/PLSP/GLSP core.

| Extension | DLSP | CSLP | PLSP | GLSP |
|---|---|---|---|---|
| Initial setup state | supported | supported | supported | supported |
| FixedSetupCost on actual setup start | supported | supported | supported | supported |
| SetupTime on actual setup start | deliberately unsupported | supported | supported | supported |
| Additional production capacity | deliberately unsupported | supported | supported | supported |
| MaximumSetupCount | supported | supported | core/supported | supported |
| MaximumProducedItemCount | core | core | core | supported |
| GroupingConstraint | supported | supported | supported | supported |
| carry-over Allowed | supported | supported | supported | supported |
| carry-over Forbidden | supported | supported | deliberately unsupported | supported |

DLSP remains all-or-nothing against regular bucket capacity. Setup time and acquired capacity are not inserted into that equality without a separately proven formulation.

PLSP production uses the incoming setup state inside a bucket. A forbidden-carry-over PLSP requires a distinct incoming-state representation and is not approximated.

SetupTime and FixedSetupCost are attached to actual setup-start occurrences, not persistent setup states.

GroupingConstraint keeps its permanent meaning: a setup occurrence at t with value g forbids another occurrence of the same routing in t+1..t+g-1 and allows one again at t+g. GLSP keeps the macro planning period as the spacing unit.

CSLP/PLSP capacity is `sum a*x + sum SetupTime*u - AdditionalCapacity <= RegularCapacity`.
GLSP capacity adds both generic setup-start time and directional SDCT changeover time.

The existing WorkCenterAdditionalCapacity business decision, cost, solver mapping and checker projection are reused. No scheduling-specific duplicate business decision is introduced.
