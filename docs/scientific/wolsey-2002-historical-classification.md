# Wolsey 2002 Historical Classification

Primary reference:

Laurence A. Wolsey,
**Solving Multi-Item Lot-Sizing Problems with an MIP Solver Using
Classification and Reformulation**,
*Management Science*, 48(12), 1587-1602, 2002.

## Single-item classification

Wolsey defines three fields:

`PROB - CAP - VAR`

### PROB

Exactly one of:

- `LS`: general lot-sizing problem;
- `WW`: LS under the Wagner-Whitin transformed-cost condition;
- `DLSI`: discrete lot-sizing with a variable initial-stock decision and
  production either zero or at full capacity in each period;
- `DLS`: DLSI without the initial-stock variable.

### CAP

Exactly one of:

- `C`: capacities vary over time;
- `CC`: constant capacity;
- `U`: uncapacitated.

### VAR

Any subset of:

- `B`: backlogging;
- `SC`: **start-up costs**;
- `ST`: **start-up times**;
- `ST(C)`: constant start-up times;
- `SL`: **sales**;
- `LB`: minimum production levels;
- `LB(C)`: constant minimum production levels;
- `SS`: safety stocks.

The historical namespace is preserved deliberately. In particular:

- Wolsey `SC` is not LotSizingDataModel `SC` (setup cost);
- Wolsey `SL` is not lost sales;
- Wolsey `ST` is not setup time.

## Multi-item / machine extension

Wolsey gives a minimal extension with the machine block:

`{NK=#,[IM,VM],[LT]*,[SB1,SB2,BB],[SET,ST,SQT,SQC]*}`

and the multi-level block:

`{NL=#,[G,A,S]}`

where the source explicitly defines:

- `NK`: number of machines;
- `LT`: lead times;
- `SB1`: at most one setup per period;
- `SB2`: at most two setups per period;
- `BB`: big bucket with joint capacity;
- `SET`: setup times;
- `ST`: start-up times;
- `SQT`: sequence-dependent changeover times;
- `SQC`: sequence-dependent changeover costs;
- `NL`: number of levels;
- `G`: general product structure;
- `A`: assembly structure;
- `S`: series/linear structure.

Wolsey also mentions `NI` for item count and `NT` for the number of periods.

The `IM` and `VM` source labels are preserved as historical symbols in the
typed representation. This increment deliberately does not expand them into
invented semantics because the available verified source excerpt does not
provide an explicit textual definition suitable for a lossless mapping.

## Universal mapping

The historical representation and universal representation remain separate.

The mapper projects only semantics already representable generically.

Examples:

- `CAP=C` -> `Cap:P,Cap:Var`;
- `CAP=CC` -> `Cap:P,TP:CapP=C`;
- `VAR=B` -> `BL`;
- Wolsey `VAR=SC` -> universal `SU` (start-up cost);
- `VAR=LB` -> `MinLot`;
- `VAR=SS` -> `SS`.

A dimension is never guessed merely because an acronym looks similar.

Current explicitly unrepresented examples include:

- the Wagner-Whitin cost condition;
- zero-or-full-capacity production of DLSI/DLS;
- variable/no-variable initial-stock decision;
- explicit uncapacitated regime `U` under positive-constraint matching;
- start-up times;
- additional sales;
- constant minimum-production-level qualifier;
- exact machine/bucket/count information;
- sequence-dependent changeover time/cost.

`HistoricalMappingCoverage.Exact` is returned only when the classification
contains no such remaining source dimension. Otherwise coverage is `Partial`
and every gap is named.
