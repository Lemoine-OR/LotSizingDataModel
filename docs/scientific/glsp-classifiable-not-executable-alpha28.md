# GLSP: Classifiable, not Executable — alpha.28

Current canonical problem-class support:

- Executable: 9 classes (six generic classes + DLSP + CSLP + PLSP);
- Classifiable: GLSP;
- CatalogOnly: none among the current ten classes.

The Standard MILP remains incompatible with GLSP (`LSDM-FORM-010`), and
`MILP-GENERAL` is not made applicable to GLSP.

Executable promotion requires a dedicated GLSP formulation, micro-period
variables and transitions, Solver mapping to `WorkCenterSchedulingDecision`,
checker projection and end-to-end solve evidence.
