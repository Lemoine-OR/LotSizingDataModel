# Scientific vs Technical Formulation Selection

Two independent questions must remain distinct.

## Scientific question

Can this formulation represent every semantic characteristic that has been
classified for the instance?

Answered by:

`ScientificFormulationCompatibilityService`

## Technical question

Can this concrete formulation object build this concrete instance right now?

Answered by:

`IMathematicalModelFormulation.CanBuild(instance)`

## Selection contract

For automatic selection from a concrete instance:

`ScientificCompatible && CanBuild(instance)`

is required.

A formulation that is scientifically `Undetermined` is not auto-selected.

A formulation that is scientifically compatible but technically rejects the
instance is also not selected.

This prevents the old `planningHorizon > 0` technical check of the standard
formulation from being mistaken for a complete scientific support contract.
