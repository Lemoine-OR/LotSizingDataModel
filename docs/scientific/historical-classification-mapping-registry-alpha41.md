# Historical Classification Mapping Registry — alpha.41

## Purpose

Alpha.41 introduces one versioned and auditable registry for historical
classification correspondences.

A rule records a stable identifier, historical family and token, universal
semantic token set, confidence, inverse-detection policy, provenance and notes.

## Confidence

`Exact` means the projection is considered lossless for the encoded semantic
scope. Only exact rules can be invertible.

`Conservative` means the forward projection is useful but intentionally weaker
or conditional. Conservative rules cannot be inverted.

`SourceOnly` means the historical label is preserved without assigning generic
semantics. Its universal projection is empty and inverse detection is disabled.

## Inverse detection

Inverse detection requires all of the following:

1. confidence `Exact`;
2. `AllowsInverse = true`;
3. exact equality of the complete normalized universal-token set;
4. exactly one matching rule.

Subsets, supersets and conservative projections never reconstruct a historical
label.

## Declared versus Detected

HistoricalMappingAuditService compares two independent token sets.

Declared labels are metadata from the source. Detected labels must come from an
independent analysis. Declared labels never become detection evidence.

The audit reports declared-but-not-detected and detected-but-not-declared
tokens separately.

## Wolsey

The default registry centralizes only mappings already consolidated by validated
LotSizingDataModel milestones.

SB1/SB2/BB remain conservative and non-invertible.

IM/VM remain source-only and non-invertible.

## Bitran–Yanasse

The family is supported by the registry architecture, but alpha.41 deliberately
does not populate default rules that have not yet been encoded as source-backed,
lossless mappings.

An empty proven rule set is preferred to invented historical semantics.

## Versioning

Baseline registry version: `1.0-alpha.41`.

Changing the meaning of an established stable RuleId requires explicit registry
versioning and migration evidence.
