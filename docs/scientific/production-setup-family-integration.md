# Production setup families on the stable 1.2.x architecture

A production setup family is a work-center-level shared setup activation.

For family `f`, member item/routing setup `y[r,t]` and family activation
`w[f,t]`:

`y[r,t] <= w[f,t]`

for every member routing on the family work center.

Optional `ProductionFamilySetupTime[f,t]` consumes the same work-center
capacity as production and item-level setup time.

No family-level setup cost is introduced by this integration. The mathematical
family activation is auxiliary and is not persisted because it is derivable
from item-level setup decisions under the present semantics.

This concept is distinct from:
- commercial product families;
- BOM grouping;
- GroupingConstraint;
- item-level setup;
- start-up state;
- sequence-dependent changeovers.
