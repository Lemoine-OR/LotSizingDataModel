# Bitran-Yanasse Historical Mapping

Reference:

G.R. Bitran and H.H. Yanasse,
**Computational Complexity of the Capacitated Lot Size Problem**,
*Management Science*, 28(10), 1174-1186, 1982.

The historical classification describes four temporal dimensions:

`alpha / beta / gamma / delta`

corresponding to:

1. setup cost;
2. holding cost;
3. production cost;
4. production capacity.

Each dimension uses:

- `Z`: zero;
- `C`: constant;
- `NI`: non-increasing;
- `ND`: non-decreasing;
- `G`: general.

## Mapping policy

LotSizingDataModel does not reinterpret these symbols.

A historical profile such as:

`NI/G/NI/ND`

is preserved exactly by `BitranYanasseTemporalProfile`.

Universal notation scheme v1 now represents the four temporal dimensions
with generic `TP` qualifiers. For example:

`1,SL,Net:UNK | Dem,Det,Prod,Cap:P,TP:SC=NI,TP:HC=G,TP:PC=NI,TP:CapP=ND | Obj:Econ`

The mapping is therefore representationally lossless:

`HistoricalMappingCoverage.Exact`

and `UnrepresentedHistoricalDimensions` is empty.

The universal tokens remain semantic rather than historical:

- historical alpha -> `TP:SC`;
- historical beta -> `TP:HC`;
- historical gamma -> `TP:PC`;
- historical delta -> `TP:CapP`.

The original `NI/G/NI/ND` code is still preserved separately.

## Applicability

The strict historical-domain assessment requires:

- one item;
- single level;
- positive planning horizon;
- demand;
- deterministic demand;
- production;
- production capacity;
- a single economic-objective family.

The mapper distinguishes:

- `Incomplete`;
- `NotApplicable`;
- `ExactHistoricalDomain`;
- `ExtendedButProjectable`.

Extensions such as backlogging, lot-size restrictions, setup times,
production lead times, additional capacity, procurement, transportation,
distribution, safety stock or financial constraints do not silently become
part of the historical class. They are reported explicitly.

## Matching against instances

Exact representation of the historical classification does not imply that an
arbitrary `LotSizingProblemDescriptor` already contains the required temporal
analyses.

`UniversalNotationMatcher` therefore reports a required `TP` qualifier as
`Incomplete` when no actual temporal-pattern analysis is supplied. A caller
that has selected the correct classical projection can provide the four
generic qualifiers produced by `CreateTemporalQualifiers(profile)` and obtain
full semantic comparison.
