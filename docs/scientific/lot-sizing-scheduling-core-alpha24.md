# Lot-Sizing + Scheduling Core I — alpha.24

alpha.24 introduces **representation semantics only** for integrated
lot-sizing/scheduling.

It does not make DLSP, CSLP, PLSP or GLSP executable.

## Literature basis

Drexl and Kimms (1997), *Lot sizing and scheduling — Survey and
extensions*, European Journal of Operational Research 99(2), 221–235,
DOI 10.1016/S0377-2217(97)00030-1, distinguish the classical integrated
lot-sizing/scheduling model families.

The small-bucket literature distinguishes:

- DLSP: at most one item in a small bucket, with all-or-nothing production;
- CSLP: at most one item in a small bucket, but production may use only a
  fraction of capacity;
- PLSP: at most one setup change inside a period, so up to two items may be
  produced.

GLSP combines macro-period lot-sizing capacity with explicit micro-period
sequencing.

alpha.24 therefore adds generic facts needed by these families without using
their model names as business data.

## Core model

Scheduling semantics are attached to `WorkCenter` through
`ProductionSchedulingProfile`.

The profile can represent:

- `SchedulingBucketMode`: BigBucket / SmallBucket / MacroMicro;
- `MicroPeriodCount`;
- `MaximumSetupCount`;
- `SetupCarryOverPolicy`;
- an `InitialSetupItemId`;
- directional `ProductionChangeover` definitions;
- sequence-dependent changeover time;
- sequence-dependent changeover cost.

## Important non-claim

A profile describing small buckets does not prove that an instance is DLSP,
CSLP or PLSP.

Those classes additionally require exact production/setup transition rules.
They remain `CatalogOnly`.

Similarly, a macro/micro profile is necessary for GLSP-style semantics but is
not sufficient by itself to classify or solve a GLSP instance.


## alpha.25 continuation

alpha.25 adds the production-quantity and per-bucket item-count semantics that
alpha.24 intentionally did not invent. These new facts are sufficient to
classify DLSP, CSLP and PLSP while preserving a separate non-executable support
state.
