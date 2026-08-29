# Scientific Solve Evidence Channels

The end-to-end result intentionally keeps several evidence channels separate.

## Scientific classification

What problem does the current instance represent?

`ScientificClassificationResult`

## Scientific formulation selection

Which registered formulation has verified semantic coverage?

`ScientificFormulationSelectionResult`

## Technical solver run

What actually happened during model construction, native solve and solution
mapping?

`SolverRunResult`

## Numerical verification

Does the normalized solution independently satisfy the requested structural,
domain, feasibility and objective checks?

`LotSizingSolutionVerificationResult`

## Scientific provenance verification

Does the solution's recorded classification/formulation provenance agree with
the current scientific stack?

`SolutionScientificProvenanceCheckResult`

No one channel substitutes for another.

`ScientificSolvePipelineResult.IsEndToEndCoherent` requires pipeline completion,
a normalized solution, and success of every verification channel that was
requested.
