# LotSizingDataModel API Stability Policy

## Status

The public integration surface introduced in alpha.43 and hardened in alpha.44
is a **release-candidate contract** for LotSizingDataModel 1.2.0.

`governance/PUBLIC-API-CONTRACT.json` is the machine-readable authority for the
critical consumer anchors protected during stable promotion.

## Compatibility principles

For the 1.2.x stable line:

- critical public type identities listed in the public API contract are not
  removed, renamed or made non-public in a patch release;
- existing public behavior is not silently reinterpreted;
- additive APIs are preferred to breaking changes;
- lower-level assemblies must not gain upward dependencies that violate the
  documented layer rules;
- UI frameworks and downstream specialized projects remain outside the
  foundational dependency graph.

A breaking public contract change requires an explicit compatibility review,
migration notes and an appropriate versioning decision.

## XML compatibility

The XML compatibility contract is defined in
`governance/XML-COMPATIBILITY-CONTRACT.json`.

Existing `lotSizingInstance` and `lotSizingSolution` roots are protected.

Serialized schema evolution should be additive whenever possible. Removing or
renaming serialized members requires migration documentation and explicit
breaking-change review.

## What is not automatically stable

A CLR type is not part of the supported stable API merely because it is
technically `public`.

The supported compatibility anchors are those explicitly listed in the public
API contract plus contracts explicitly documented as stable elsewhere.

Internal implementation structure, helper classes and non-contracted public
types may continue to evolve, subject to normal semantic-versioning review.

## Downstream projects

MLLPAlgorithm, future UI applications and other specialized projects consume
LotSizingDataModel.

LotSizingDataModel does not depend on those downstream projects.
