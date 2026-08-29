# Declared Notation vs Detected Notation

LotSizingDataModel distinguishes two fundamentally different concepts.

## DeclaredNotation

Notation supplied by:

- an instance file;
- a benchmark author;
- a publication;
- a user;
- an importer.

It is evidence about what the source **claims**.

The scientific engine keeps the raw trimmed declaration and, if valid, its
canonical parsed specification.

## DetectedNotation

Notation generated from actual typed descriptor semantics and supplied derived
analyses.

It is evidence about what LotSizingDataModel **detects**.

Detected notation is never copied from declared notation.

## Comparison

A valid declaration is compared using the universal matcher:

- `Exact`: canonical equality;
- `Compatible`: the declaration is a satisfied positive partial specification;
- `Incomplete`: the declaration requires an analysis that is not available;
- `Contradiction`: known detected semantics conflict with the declaration.

Malformed declarations are `InvalidDeclaredNotation`, but they do not prevent
LotSizingDataModel from computing detected notation and problem classes.

This separation is important for benchmark auditing: a mislabeled instance can
be preserved faithfully while the detected classification exposes the
disagreement.


## Blocked classification

When `InstanceModelChecker` blocks scientific classification, a source
declaration is retained but marked `NotEvaluated`. It is not mislabeled as an
invalid notation string because parsing/comparison was deliberately not run.
