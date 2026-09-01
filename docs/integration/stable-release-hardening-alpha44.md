# Stable Release Hardening — alpha.44

Alpha.44 is intentionally not a scientific feature milestone.

Its purpose is to prove that the project can be promoted to a stable 1.2.0
release without changing its public consumer anchors or serialized roots.

## Hardened contracts

- `governance/PUBLIC-API-CONTRACT.json`
- `governance/XML-COMPATIBILITY-CONTRACT.json`
- `governance/STABLE-RELEASE-CONTRACT.json`
- `API-STABILITY.md`
- `RELEASE-CHECKLIST.md`
- `CITATION.cff`

## XML compatibility smoke

The targeted alpha.44 smoke constructs minimal Instance and Solution objects,
serializes them using the canonical serializers, verifies the protected XML
root elements and deserializes them again.

The test deliberately checks identity and round-trip behavior rather than
comparing formatting bytes.

## Public API continuity

The alpha.43 critical public anchors remain the release-candidate public
surface.

Alpha.44 upgrades the contract status from candidate-stable to
release-candidate. No public anchor is removed by this milestone.

## Stable promotion

Alpha.44 does not push, tag or publish anything.

After alpha.44 is validated, the next step is a dedicated stable-promotion
candidate changing the repository version from `1.2.0-alpha.44` to `1.2.0`,
updating release identity metadata, rerunning every hardening gate and only
then performing push/tag/release if explicitly authorized.
