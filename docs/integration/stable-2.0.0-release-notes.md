# LotSizingDataModel 2.0.0

Version 2.0.0 introduces a breaking transport API and XML transport format version 2.

## Transport model and migration

Directed lanes are now central in `SupplyChain.TransportLanes` and unique by ordered origin/destination. Independent `TransportResources` can serve multiple lanes; `TransportAssignments` associate each lane/resource pair with its nonnegative integer delay. Unassigned lanes and resources are permitted during construction; optimization readiness is checked separately. Removal rejects outstanding references instead of silently cascading.

Consumers must replace resource-owned lane construction with central lanes and explicit assignments. Item/resource `TransportCharacteristic` semantics are preserved. Canonical Core and Instance XML serializers migrate legacy embedded lanes in memory, group directed endpoints, generate deterministic lane IDs and transfer assignment delays. Migration is idempotent and rejects conflicting input. Raw `XmlSerializer` is not a legacy migration entry point. XML root names remain unchanged. Keep an original copy of legacy data until the migrated round trip has been verified.

See the [transport specification and worked example](https://github.com/Lemoine-OR/LotSizingDataModel/blob/v2.0.0/docs/scientific/transport-assignments.md) for API examples, migration cases and the complete class/parameter/equation mapping.

## Formulation

The formulation uses lanes L, resources R and assignments A contained in L x R, with delay tau(l,r). Transport variables identify the lane and resource. Each resource shares its capacity across its assigned lanes, and its period fixed cost is charged once. Item extra capacity is also shared across assignments. Activation and setup constraints use valid finite bounds; `TransportActivationBigM` can supply an explicit bound when one cannot be inferred.

Zero-delay transport and the existing open-horizon convention are preserved. Departures arriving after the horizon remain permitted and do not create in-horizon receipts. There is no new initial-transit entity; existing scheduled receipts remain available.

## Validation and distribution

The implementation passed 460 automated tests (Core 40, Instance 208, Checker 212), an 18-project Release build with zero warnings/errors, adapter validation, and three native CPLEX transport smoke scenarios. The official release workflow independently builds, tests, validates documentation and checks release assets from the merged main commit before publication.

The GitHub release distributes validated binaries, generated documentation, build/adapter/release manifests and SHA-256 checksums. Proprietary solver runtimes and licenses are not bundled. This publication does not imply a NuGet.org package publication.

## Remaining limits

Existing scientific non-claims remain in `governance/STABLE-OPEN-GAPS.json`; directed transport does not automatically add closed-loop return routing. Legacy Core analysis helpers retain their existing default-coefficient/setup conventions; use explicit coefficients and the generated mathematical model/checker for the documented formulation. Existing Doxygen diagnostics are documented separately from the zero-warning .NET build.
