# Lot-Sizing + Scheduling Classes — Catalog Status

The canonical scientific catalog already records the main integrated
small-bucket classes, but alpha.15 does **not** claim executable support.

## DLSP

Discrete lot-sizing and scheduling problem.

Small-bucket semantics restrict production to at most one product per
micro-period and use an all-or-nothing production structure.

## CSLP

Continuous setup lot-sizing problem.

Like DLSP, only one product is produced per small bucket, but the
all-or-nothing/full-capacity production restriction is relaxed.

## PLSP

Proportional lot-sizing and scheduling problem.

The model permits one setup/changeover within a period, allowing up to two
products in a bucket.

## GLSP

General lot-sizing and scheduling problem.

Macro-periods are subdivided into micro-periods so lot sizing and sequencing
can be integrated.

## Why CatalogOnly?

The current LotSizingDataModel still lacks a complete scheduling layer with
the required bucket, setup-state/changeover and sequencing semantics across:

Core -> Instance -> notation -> formulation -> Solution -> checkers.

Creating executable universal specifications now would therefore be nominal
and scientifically unsafe.

References include Drexl & Kimms (1997), *Lot sizing and scheduling — Survey
and extensions*, and the later dynamic capacitated lot-sizing review
literature.
