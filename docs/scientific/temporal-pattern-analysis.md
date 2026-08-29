# Temporal Pattern Analysis

`v1.2.0-alpha.6` implements the temporal pattern vocabulary used by
Bitran and Yanasse for capacitated single-item lot sizing:

- `Z`: zero;
- `C`: constant over time;
- `NI`: non-increasing over time;
- `ND`: non-decreasing over time;
- `G`: general / no prescribed pattern.

The historical four-position profile is:

`setup cost / holding cost / production cost / capacity`

and remains deliberately separate from the future LotSizingDataModel universal
`alpha | beta | gamma` notation.

## Canonical specificity

The mathematical classes overlap. The analyzer therefore applies:

`Zero > Constant > one-direction monotonicity > General`.

This means an all-zero series is `Z`, not merely `C`, and a non-zero constant
series is `C`, not arbitrarily `NI` or `ND`.

## Numerical tolerance

A configurable absolute and relative tolerance is converted into one effective
absolute tolerance using the maximum absolute value of the complete series.

If tolerance makes a non-constant series appear both non-increasing and
non-decreasing, the analyzer returns `G` rather than choosing an arbitrary
direction.

Empty series and NaN/infinite values are rejected.

## Historical profile

`BitranYanasseProfileAnalyzer` accepts four explicit period series and returns
the exact slash-separated historical code, for example:

`NI/G/NI/ND`

Automatic extraction of the four relevant series from an arbitrary
multi-item/multi-site LotSizingDataModel instance is intentionally deferred.
That projection requires a scientifically explicit mapping to a classical
single-item capacitated lot-sizing subproblem.
