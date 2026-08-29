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
