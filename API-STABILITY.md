# Public API stability: 2.0.x

Version 2.0.0 establishes the stable 2.0.x API line. Its intentional breaking change replaces resource-owned transport lanes with central directed lanes and explicit lane/resource assignments. See [release notes](docs/integration/stable-2.0.0-release-notes.md) and [migration/API mapping](docs/scientific/transport-assignments.md).

Critical type identities are recorded in `governance/PUBLIC-API-CONTRACT.json`. Patch releases preserve these identities and the documented 2.0 transport contract. Additive evolution is preferred; further breaking changes require explicit review, migration documentation and a suitable major version.

The instance and solution XML root names remain protected. Canonical serializers migrate the legacy transport representation to transport format version 2 without silently choosing between conflicting data. XML compatibility rules are in `governance/XML-COMPATIBILITY-CONTRACT.json`.

UI and algorithm applications remain separate downstream projects. Dependencies point from consumers to LotSizingDataModel.
