# Lot-Sizing Instance notation (LSI/1)

## Purpose

LSI is the instance-oriented semantic notation used by `LotSizingDataModel.Instance`.

Canonical form:

```text
LSI/1.0: pi{...} | alpha{...} | beta{...} | gamma{...} @ sigma{...}
```

The notation is intentionally richer than a single historical family name.

- `pi`: planning context, time and information regime.
- `alpha`: physical/productive system structure.
- `beta`: operational characteristics, constraints and extensions.
- `gamma`: objective semantics when explicitly represented.
- `sigma`: concrete instance dimensions.

Historical names such as `LS-U`, `LS-C`, `CLSP`, `MLLP` and `MLCLSP` are
compatibility projections, not replacements for the complete LSI signature.

## Semantic states

LSI distinguishes the following states:

| State | Canonical token | Meaning |
|---|---|---|
| Present | `1` | Feature explicitly present |
| Absent | `0` | Feature explicitly absent |
| Unknown | `?` | Model cannot determine the feature |
| Not applicable | `NA` | Feature has no meaning in this context |
| Mixed | `MIX` | Heterogeneous regimes coexist |

`Unknown` must never be silently converted to `Absent`.

## Temporal profiles

A numerical parameter may carry a temporal qualifier:

| Token | Meaning |
|---|---|
| `Z` | zero |
| `C` | constant |
| `NI` | non-increasing |
| `ND` | non-decreasing |
| `G` | general |
| `PER` | periodic |
| `MIX` | heterogeneous |
| `?` | unknown |

Example:

```text
SET.C=1~NI
CAP.P=1~ND
```

The vocabulary generalizes the classical Bitran-Yanasse profile idea without
restricting it to the original four parameter positions.

## pi block

Current LSI/1 keys:

```text
H
TM
BK
INF
DEM
DEM.SRC
```

Examples:

```text
H=F
TM=DT
BK=?
INF=DET
DEM=DYN
DEM.SRC=EXO
```

`BK` remains `?` on a Core version that does not explicitly model
big-bucket/small-bucket semantics.

## alpha block

Current keys:

```text
I
LV
PS
NET
ROUT
RES
```

`PS` reuses the product-structure analysis already maintained by
`LotSizingDataModel.Instance`.

## beta block

Stable code families include:

```text
CAP.*     capacity
SET.*     setup/start-up
LOT.*     lot restrictions
LT.*      lead times
INV.*     inventory
SHORT.*   shortage behavior
SRC.*     sourcing
TRANS     transportation
DIST      distribution
NET.*     network extensions
FIN       financial constraints
OBJ.*     multiobjective flags
SCH.*     integrated scheduling vocabulary
```

A code may exist in the LSI vocabulary even when the installed Core cannot
currently populate it. Such semantics remain unknown/not emitted rather than
being invented.

## gamma block

`gamma{?}` means that the objective is not explicitly represented by the
installed Core version.

LSI must not infer an optimization objective merely from the presence of cost
or revenue parameters.

## sigma block

`sigma` stores concrete instance size, independently from mathematical family:

```text
sigma{T=12,I=5,P=1,WC=1,WH=0,SUP=0,DC=0,TR=0,BOM=4,DEPTH=4}
```

This distinction allows two benchmark instances to share the same semantic
problem class while having different dimensions.

## Canonical determinism

The canonical formatter:

- uses invariant formatting;
- fixes field order;
- sorts beta feature codes;
- sorts mixed temporal components;
- produces a stable representation suitable for testing, registries and
  scientific benchmark metadata.

The canonical parser supports exact formatter-parser-formatter round trips.

## Backward compatibility

Existing XML documents without an LSI `<signature>` element remain readable.
The classification object creates an empty/default signature which can later be
recomputed from the `SupplyChain`.

The historical classifier remains authoritative for existing family codes.
LSI adds information; it does not change historical classification rules.
