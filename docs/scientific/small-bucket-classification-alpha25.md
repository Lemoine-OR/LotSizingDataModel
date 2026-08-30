# Small-Bucket Scheduling Classification — alpha.25

alpha.25 deliberately separates **classifiability** from **solver
executability**.

New support level:

`Classifiable`

means that current Core/Instance semantics are sufficient to assess canonical
problem-class membership, but no executable mathematical formulation or solver
support is claimed.

## Scientific basis

Fleischmann (1990), *The discrete lot-sizing and scheduling problem*,
European Journal of Operational Research 44(3), 337–348,
DOI 10.1016/0377-2217(90)90245-7, describes the DLSP as a single-machine
dynamic-demand scheduling problem on many short periods.

Drexl and Kimms (1997), *Lot sizing and scheduling — Survey and extensions*,
European Journal of Operational Research 99(2), 221–235,
DOI 10.1016/S0377-2217(97)00030-1, distinguish DLSP, CSLP and PLSP.

Drexl and Haase (1995), *Proportional lotsizing and scheduling*,
International Journal of Production Economics 40(1), 73–87,
DOI 10.1016/0925-5273(95)00040-U, introduce PLSP from the limitations of
DLSP/CSLP.

## Canonical alpha.25 rules

Common small-bucket core:

- integrated scheduling;
- small-bucket time structure;
- multiple items;
- single level;
- one scheduling resource;
- deterministic demand;
- production capacity;
- shared production capacity.

DLSP:

- all-or-nothing bucket production;
- at most one produced item per bucket.

CSLP:

- continuous lot quantity up to bucket capacity;
- at most one produced item per bucket.

PLSP:

- continuous lot quantity;
- at most two produced items per bucket;
- at most one setup transition per bucket.

Sequence-dependent changeover time/cost, setup carry-over and initial setup
state are treated as extensions rather than as DLSP/CSLP/PLSP identity.

GLSP remains `CatalogOnly` because macro/micro representation alone is not yet
a complete executable/classifiable GLSP contract.
