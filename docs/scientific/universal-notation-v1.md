# Universal Lot-Sizing Notation — Scheme v1

The LotSizingDataModel universal notation is a versioned three-field scheme:

`alpha | beta | gamma`

It is inspired by the role played by Graham-style notation in scheduling, but
is designed for lot-sizing, lot-sizing+scheduling and supply-chain extensions.

## Alpha — structure/environment

Canonical examples:

- `1` or `m`: single or multiple items;
- `SL`: single-level;
- `ML:SER`, `ML:ASM`, `ML:ARB`, `ML:GEN`: multi-level BOM structure;
- `Net:IND`, `Net:SER`, `Net:CONV`, `Net:DIV`, `Net:TREE`, `Net:GEN`;
- network modifiers `E<n>`, `CY`, `MS`, `TS`.

Closed-loop syntax is already reserved:

`Net:CL(F:DIV;R:CONV):E3`

Generation of a reverse network remains disabled until Core contains real
reverse-flow semantics.

## Beta — constraints/extensions

Scheme v1 defines stable canonical tokens for characteristics already
represented by typed descriptors. Examples include:

`Dem,Det,DVar,Prod,Uncap:P,Cap:P,Cap:Var,SC,ST,MinLot,BL,Buy,Tr,Dist,Fin`

`Uncap:P` explicitly requires production without a production-capacity constraint. Token omission remains unconstrained under partial-specification matching.

The renderer orders tokens canonically. The parser accepts arbitrary beta-token
order and normalizes it.

## Gamma — objective

Scheme v1 supports:

- `Obj:Econ`: current standard economic objective family;
- `Obj:Multi`: multiple objectives are declared;
- `Obj:?`: unspecified objective.

The AST is intentionally extensible toward objective expressions such as
weighted sums, lexicographic objectives and epsilon-constraint policies.

## Versioning and round trip

The text itself remains compact. The AST carries `SchemeVersion = "1"`.

The core invariant is:

`Render(Parse(Render(Generate(descriptor)))) == Render(Generate(descriptor))`

## Historical notations

Bitran-Yanasse `alpha/beta/gamma/delta` slash notation is not merged into this
grammar. Historical classifications are projections/aliases over the universal
descriptor/notation system and retain their own exact semantics.


## Generic derived conditions

The beta field may also contain positive semantic conditions backed by an
explicit analysis:

- `Cost:NS`: non-speculative adjacent production/holding costs;
- `ProdMode:0F`: positive production is forced to full capacity.

When used as a specification these tokens are not inferred from omission.
Their actual state is supplied through `UniversalDerivedSemantics`.


## alpha.21 start-up semantics

Start-up semantics are distinct from ordinary setup semantics.

New generic beta token:

- `SUT`: start-up time, i.e. capacity consumed when a new sequence of
  production setups starts.

Existing generic token:

- `SU`: start-up cost.

The historical Wolsey acronym `ST` means **start-up time** and therefore maps
to generic `SUT`; it must never be confused with universal `ST`, which means
ordinary setup time.

Start-up time can be temporally qualified. Example:

`TP:SUT=C`

means constant start-up time.

Existing numeric values of previously published `UniversalNotationFeature`
members are preserved; `StartUpTime` is appended as a new semantic value.
