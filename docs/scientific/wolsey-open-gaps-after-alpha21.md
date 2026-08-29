# Wolsey Gaps Remaining after alpha.21

Start-up cost/time are no longer historical representation gaps.

Remaining major dimensions include:

## DLSI / DLS initial stock

Wolsey DLSI contains a variable initial stock `s_0`.
DLS is the version without that variable.

Current LotSizingDataModel initial inventory is input data. It is not a
decision variable and therefore cannot close this distinction.

## SL — additional sales

Wolsey sales means:

mandatory demand `d_t` must still be satisfied, and an **additional** quantity
up to `u_t` may be sold at unit price `c_t`.

LotSizingDataModel already contains `SellingPrice` for deliveries, but does
not yet contain the separate additional-sales upper bound/decision. Mapping
`SellingPrice` to Wolsey `SL` would therefore be wrong.

## Exact count dimensions

- `NK`: exact number of machines;
- `NI`: exact number of items;
- `NT`: exact number of periods;
- `NL`: exact number of levels.

Some underlying counts exist in descriptors, but notation v1 intentionally
does not encode all exact numerical counts.

## Machine/bucket semantics

- `IM` / `VM`: retained historical symbols pending source-safe semantic
  expansion;
- `SB1` / `SB2`: maximum setup count per bucket;
- `BB`: big-bucket semantics.

## Sequence-dependent changes

- `SQT`: sequence-dependent changeover time;
- `SQC`: sequence-dependent changeover cost.

These require the future integrated lot-sizing + scheduling layer.
