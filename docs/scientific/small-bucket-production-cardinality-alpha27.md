# Period-dependent Produced-Item Cardinality — alpha.27

`MaximumProducedItemCount` is business data, not merely a classification hint.

alpha.27 introduces the mathematical-only positive-production activation
`q[r,t]` in DLSP, CSLP and PLSP and enforces:

`sum_r q[r,t] <= MaximumProducedItemCount[t]`.

Links:

- DLSP: `a*x = U*q`, `q <= y`;
- CSLP: `a*x <= U*q`, `q <= y`;
- PLSP: `a*x <= U*q`, `q <= y[t-1]+y[t]`.

The normalized solution does not persist `q`. The independent checker derives
it exactly as `1` for strictly positive production and `0` otherwise.

Therefore a period-specific limit of zero is now enforced end-to-end without
adding redundant solution data.
