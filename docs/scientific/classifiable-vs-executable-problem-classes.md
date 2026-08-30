# Classifiable vs Executable Problem Classes

`LotSizingProblemClassSupportLevel` now separates three states.

## Executable

Membership can be assessed and the class belongs to the currently executable
scientific resolution scope.

Current established core classes remain:

- SI-ULS;
- SI-CLSP;
- MI-ULS;
- MI-CLSP;
- UMLSP;
- MLCLSP.

## Classifiable

Membership can be assessed from real data, but an executable formulation is
not yet claimed.

alpha.25:

- DLSP;
- CSLP;
- PLSP.

## CatalogOnly

The class is known scientifically but current semantics are still insufficient
for canonical membership assessment.

alpha.25:

- GLSP.

This prevents "we can recognize it" from being silently interpreted as
"the Solver can solve it".


## Formulation compatibility ordering

`Classifiable` does not imply that a mathematical formulation can assess the
instance as compatible.

The compatibility pipeline is deliberately ordered:

1. a unique canonical problem class must exist;
2. the formulation must support that canonical class;
3. only then are objective and extension capabilities evaluated.

Therefore:

- an incomplete scheduling description with no unique problem class produces
  `Undetermined` and diagnostic `LSDM-FORM-003`;
- a complete CSLP/DLSP/PLSP classification assessed against the current
  standard lot-sizing MILP produces `Incompatible` with `LSDM-FORM-010`,
  because the formulation does not support that scheduling class;
- `KnownUnsupportedExtensions` is not populated after a terminal
  problem-class rejection. Profile-level scheduling-extension support is tested
  independently.

This prevents extension flags from manufacturing a formulation verdict before
the canonical problem itself is known.
