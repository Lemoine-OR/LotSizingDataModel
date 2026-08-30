# Small-Bucket Production Semantics

alpha.24 represented bucket structure but could not distinguish DLSP from
CSLP. alpha.25 adds the missing generic semantics.

## SmallBucketProductionMode

`AllOrNothing`

Production in a selected small bucket is either zero or exactly full bucket
capacity.

`Continuous`

Any quantity from zero to available bucket capacity may be produced.

## MaximumProducedItemCount

Period-dependent upper bound on the number of distinct items produced in one
bucket.

It is different from `MaximumSetupCount`.

## Canonical notation

- `SchedRes:1` — one scheduling resource;
- `SBProd:0F` — all-or-nothing small-bucket production;
- `SBProd:Cont` — continuous small-bucket production quantity;
- `BucketItems:1` — at most one produced item per bucket;
- `BucketItems:2` — at most two produced items per bucket;
- `SetupTrans:1` — at most one setup transition per bucket.

These are generic semantics, not historical model names.
