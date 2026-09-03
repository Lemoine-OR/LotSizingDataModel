# LSI/1 reference and Core coverage

## Current local implementation

The LSI implementation is layered under the existing problem classification:

```text
SupplyChain
  -> ProductStructureAnalyzer
  -> LotSizingProblemFeatureExtractor
  -> LotSizingInstanceSignatureExtractor
  -> LSI canonical/compact notation
  -> legacy family projection
```

## Compatibility projection

| LSI structure | Capacity | Historical code |
|---|---:|---|
| single item, single level | absent | `LS-U` |
| single item, single level | present | `LS-C` |
| multiple items, single level | present | `CLSP` |
| multiple items, multiple levels | absent | `MLLP` |
| multiple items, multiple levels | present | `MLCLSP` |

The projection is intentionally limited to the historical families already
supported by the existing standard catalog.

## Core support matrix

| LSI concept | Local status |
|---|---|
| finite horizon | supported |
| discrete periods | supported |
| deterministic demand | supported |
| time-varying demand | supported |
| item/BOM structure | supported |
| production capacity | supported |
| shared production capacity | supported |
| setup cost/time | supported |
| production lead time | supported |
| minimum lot size | supported |
| lot-size multiple | supported |
| grouping constraint | supported |
| additional capacities | supported where represented |
| purchasing | supported |
| supplier lead time | supported |
| transportation | supported |
| transport lead time/capacity | supported where represented |
| warehouse capacity | supported |
| multi-site | supported |
| stochastic/robust/fuzzy demand | not represented in local Core |
| routing-environment taxonomy | requires dedicated analyzer |
| explicit objective algebra | not represented in local Core |
| big/small-bucket scheduling | vocabulary reserved; local Core does not expose it |
| sequence-dependent setup | vocabulary reserved; local Core does not expose it |
| setup carry-over | vocabulary reserved; local Core does not expose it |

## Scientific lineage

LSI follows the spirit of Graham-style decomposition by separating system
structure, characteristics and objective, while adding a planning-context
block (`pi`) and an explicit instance-size block (`sigma`) because these are
central to lot-sizing benchmark identification.

The temporal-profile layer generalizes the classical Bitran-Yanasse
zero/constant/monotone/general parameter characterization.

The historical names used by the codebase remain projections for
compatibility with the established lot-sizing literature.

## Evolution rule

Future Core extensions must populate existing LSI fields whenever possible.
The grammar should only be versioned when semantics cannot be expressed
without changing the meaning of an existing token.
