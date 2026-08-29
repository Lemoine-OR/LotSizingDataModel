# `lsdm.scientific.*` Reserved Parameter Contract

Reserved `SolutionGenerationMetadata.Parameters` names:

- `lsdm.scientific.schemaVersion`
- `lsdm.scientific.notationSchemeId`
- `lsdm.scientific.notationSchemeVersion`
- `lsdm.scientific.detectedNotation`
- `lsdm.scientific.problemClassCode`
- `lsdm.scientific.problemClassMatchKind`
- `lsdm.scientific.formulationId`
- `lsdm.scientific.formulationFamily`
- `lsdm.scientific.formulationCompatibility`
- `lsdm.scientific.capturedAtUtc`

Applications must not reuse this namespace for solver tuning parameters.

The codec uses existing `SetParameter` replacement semantics, so refreshing
the provenance is idempotent.

An unknown provenance schema version is `Invalid`; it is never guessed as
version 1.
