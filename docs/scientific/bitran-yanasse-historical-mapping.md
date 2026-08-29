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

Universal notation scheme v1 can currently represent the classical problem
domain:

`1,SL,Net:UNK | Dem,Det,Prod,Cap:P | Obj:Econ`

but it does not yet contain generic parameterized tokens for the four temporal
patterns above.

Therefore `BitranYanasseHistoricalMapping.Coverage` is deliberately:

`Partial`

and the following source dimensions remain explicitly recorded as
unrepresented:

- setupCostPattern;
- holdingCostPattern;
- productionCostPattern;
- capacityPattern.

No information is discarded and no exact-equivalence claim is made.

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

## Next step

To make the mapping exact in universal notation, the universal grammar needs
generic temporal qualifiers independent from Bitran-Yanasse, for example a
typed concept equivalent to:

`Pattern(parameter, Z|C|NI|ND|G)`

Only after that generic capability exists should this historical mapping be
upgraded from `Partial` to `Exact`.
