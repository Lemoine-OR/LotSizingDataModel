# Universal Objective Notation — alpha.23

Gamma objective tokens:

- `Obj:Econ`
- `Obj:Fin`
- `Obj:Sust`
- `Obj:Service`
- `Obj:Multi`
- `Obj:?`

The historical values of existing enum members remain stable; new objective
kinds are appended.

Critical separation:

`Fin` in beta = a financial constraint exists.

`Obj:Fin` in gamma = the optimization objective itself is financial.

An instance may therefore legitimately be:

`... | ...,Fin | Obj:Econ`

which means an economic optimization problem constrained by a financial
operating-expenditure envelope.
