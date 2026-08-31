# v1.2.0-alpha.32 — Historical Semantics Completion II

## Scientific rule

Historical acronyms are projections of generic model semantics. They are never
allowed to create semantic evidence by themselves.

Primary source used for the Wolsey dimensions:

Laurence A. Wolsey, *Solving Multi-Item Lot-Sizing Problems with an MIP Solver
using Classification and Reformulation*, CORE Discussion Paper 2002/12, 2002.

## DLSI / DLS

Wolsey defines DLSI as discrete lot sizing with variable initial stock and
zero-or-full-capacity production. DLS is DLSI without the initial-stock
variable.

alpha.32 therefore introduces the generic
`InitialInventoryDecisionMode` with three distinct meanings:

- `FixedParameter`: historical LotSizingDataModel behavior;
- `VariableDecision`: a real non-negative period-zero mathematical variable;
- `AbsentFixedZero`: no period-zero variable and stock fixed to zero.

For `VariableDecision`, the standard MILP now creates an explicit period-zero
inventory variable, injects it into the first inventory balance, applies an
independent unit coefficient to the objective, maps it into
`InventoryDecision.InitialInventoryLevel`, and lets the checker projector
reconstruct it independently.

No DLS/DLSI detection is produced merely from an imported historical label.

## SL / SalesOption

Wolsey's SL extension permits an additional amount, bounded by a period
parameter, to be sold at a unit price in addition to mandatory demand.

alpha.32 introduces a generic `SalesOption` relation with:

- item;
- distribution center;
- `MaximumAdditionalSales[t]`;
- `UnitPrice[t]`.

This is deliberately distinct from shortage, backlog, lost sales and mandatory
demand.

The representation and historical projection are implemented in alpha.32.
Execution is **not** claimed yet: a future formulation must propagate optional
sales through distribution flow, objective and normalized solution without
reusing shortage variables.

## Exact historical counters

The Wolsey descriptor exposes exact lossless counters outside universal
notation v1:

- `NK`: number of work centers/machines;
- `NI`: number of items;
- `NT`: number of planning periods;
- `NL`: exact BOM depth when the logical BOM is acyclic.

This follows the roadmap decision to prefer descriptors over a gratuitous
notation-v2 break.

## SB1 / SB2 / BB

Historical bucket labels are projected only when generic scheduling evidence is
sufficient:

- `SB1`: small-bucket scheduling with a proven maximum of at most one setup per
  period;
- `SB2`: small-bucket scheduling with a proven maximum of at most two setups per
  period;
- `BB`: big-bucket semantics plus a joint work-center capacity shared by more
  than one produced item.

No token is emitted from name similarity alone.

## SET

`SET` is projected exactly from generic production `SetupTime` data. It is not
confused with `StartUpTime` or sequence-dependent changeover time.

## IM / VM

The 2002 classification syntax contains `IM` and `VM`, but the consulted source
does not provide enough generic semantics in the classification section to
justify an automatic interpretation.

alpha.32 therefore preserves `IM`/`VM` only as declared source labels in
`HistoricalSemanticsMetadata`.

They are never inferred. This is an intentional scientific non-closure, not a
missing switch statement.

## Declared versus detected

`HistoricalSemanticsMetadata.OriginalWolseyCode` and
`DeclaredWolseyMachineLabel` are source provenance.

`WolseyHistoricalSemanticsAnalyzer` ignores them when detecting generic model
semantics. The resulting descriptor exposes the declared machine label
separately and reports `MachineLabelWasInferred == false`.

## Scope status

- EXT-HIST-01: implemented for generic representation and standard MILP
  period-zero stock decision.
- EXT-HIST-02: implemented as explicit absence/fixed-zero semantics.
- EXT-HIST-03: generic SalesOption representation + historical projection;
  execution remains open.
- EXT-HIST-04: source-label preservation implemented; semantic interpretation
  remains intentionally open.
- EXT-HIST-05..08: exact NK/NI/NT/NL descriptors implemented.
- EXT-HIST-09: SB1/SB2/BB projection implemented where generic evidence is
  exact.
- EXT-HIST-10: SET projection aligned with generic SetupTime.
- EXT-HIST-11: inverse detection remains conservative and cannot use declared
  codes as detected evidence.
