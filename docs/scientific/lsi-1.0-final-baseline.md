# LSI 1.0 final baseline

The LSI 1.0 baseline is:

`LSI/1.0: pi{...} | alpha{...} | beta{...} | gamma{...} @ sigma{...}`

The baseline includes:
- canonical notation, parser and XML persistence;
- planning and objective semantics;
- operational extensions including maximum lot size and supplier capacity;
- production setup families;
- setup carry-over and sequence-dependent setup time/cost classification;
- executable local scheduling substrate with micro-period setup states and
  changeover variables;
- explicit formulation admission guards;
- checker-side scheduling semantics;
- legacy LS-U / LS-C / CLSP / MLLP / MLCLSP projection.

Pack 10 introduces no new scientific semantics. It closes the engineering
baseline with warning cleanup, permanent guards and full fresh validation.
