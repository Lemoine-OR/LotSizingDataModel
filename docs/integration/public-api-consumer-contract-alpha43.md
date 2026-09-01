# Public API & Integration Surface Hardening — alpha.43

## Scope

Alpha.43 deliberately does **not** introduce a graphical user interface.

LotSizingDataModel is treated as a reusable scientific library. User
interfaces, MLLPAlgorithm and other specialized applications are downstream
projects.

The dependency direction is therefore:

`downstream project -> LotSizingDataModel`

and never the reverse.

## Existing high-level entry points are reused

Alpha.43 does not create a second orchestration subsystem.

The project already exposes high-level public services, including the Checker
facade `LotSizingSolutionVerificationService` and campaign services. These
existing entry points are governed as public consumer anchors rather than
wrapped again merely for naming convenience.

## Contract manifest

`governance/PUBLIC-API-CONTRACT.json` is the candidate public integration
contract.

It records:

- contract version;
- critical public types and their source identities;
- consumer profiles;
- forbidden foundational dependencies;
- layer dependency rules.

The alpha.43 contract is `candidate-stable`. Alpha.44 will perform final
stable-release hardening and decide which candidate surfaces are frozen for the
first stable release.

## Consumer profiles

### data

Use Core + Instance + Solution when a consumer only needs to create, read,
transform or persist problem and solution data.

### optimization

Add Solver when mathematical formulation / solving functionality is needed.

A concrete native backend remains an explicit consumer choice.

### verification

Add Checker for independent feasibility/objective verification.

### campaign

Add Checker.Campaign for directory verification and benchmark campaign
reporting.

## Presentation boundary

WPF, Avalonia, WinUI and Windows Forms are deliberately outside the
foundational projects.

A future UI project may depend on LotSizingDataModel, but LotSizingDataModel
must never depend on that UI.

## Downstream algorithm boundary

MLLPAlgorithm is also a downstream project.

LotSizingDataModel must not contain package, project or assembly dependencies
toward MLLPAlgorithm.

Generic algorithms already owned by LotSizingDataModel remain generic; future
MLLP-specific orchestration belongs to MLLPAlgorithm.

## Compatibility policy

Changing or removing a critical public type listed in
`PUBLIC-API-CONTRACT.json` is an API event and requires:

1. explicit contract review;
2. compatibility/migration rationale;
3. updated consumer smoke tests;
4. changelog/API-stability documentation.

Internal namespaces and implementation details are not made stable merely
because they are technically public today. The alpha.43 manifest identifies
the first explicit externally supported anchors.

## Release transition

Alpha.43 establishes public-surface governance.

Alpha.44 will then perform stable-release hardening: compatibility baselines,
XML round-trip/non-regression, package metadata, documentation parity, CI and
release readiness.
