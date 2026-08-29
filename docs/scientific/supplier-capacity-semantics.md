# Supplier Capacity Semantics — alpha.22

alpha.22 introduces relation-specific supplier procurement capacity on
`SupplierDelivery`.

For supplier `f`, item `i`, destination warehouse `w` and period `t`:

`P[f,i,w,t] <= CapSupplier[f,i,w,t]`

The existing procurement decision is reused; no new Solution decision family
is necessary.

This is not yet supplier-wide shared capacity across multiple items or
destinations. Such a shared physical supplier resource remains a separate
future extension.

Universal notation:

- `Cap:S`;
- `TP:CapS=<pattern>`.

The existing `MathematicalFeasibilityChecker` projects business decisions and
evaluates constraints already contained in the generated model. Therefore no
second supplier-capacity equation is duplicated in the Checker.
