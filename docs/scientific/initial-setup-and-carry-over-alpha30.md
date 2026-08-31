# Initial Setup State and Carry-Over — alpha.30

SetupCarryOverPolicy is now propagated losslessly into classification features/descriptors.

Notation:
- `SCO` = carry-over allowed;
- `SCO:0` = carry-over explicitly forbidden;
- absence = unspecified.

Mathematical setup-start domain keys carry fixed context needed by the independent checker:
- `fromItem=<id>` for the initial predecessor;
- `setupReset=1` for a forbidden carry-over boundary.

These are formulation-domain facts, not duplicated business decisions in LotSizingSolution.

For GLSP an initial item different from the first micro-period item also creates the exact directional initial changeover, so SDCT and SDCC are not lost at the horizon boundary.
