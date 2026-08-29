# Physical Supply Network Descriptor

`v1.2.0-alpha.5` separates two directed graphs that must never be confused:

1. product/BOM graph (`ComponentRequirement`);
2. physical supply-flow network.

The forward physical graph is extracted from current Core semantics:

- `SupplierDelivery`: supplier -> warehouse;
- `TransportLane`: warehouse -> warehouse;
- `DistributionCenterSourcing`: warehouse -> distribution center.

Detected properties include topology, physical cycles, sources/sinks,
echelon count for acyclic networks, supplier/DC multi-sourcing,
warehouse transshipment, and a conservative classical-DRP structural
candidate.

A physical cycle is legal data and is represented by `HasCycles = true`.
A BOM cycle remains invalid.

## Closed-loop contract

`SupplyNetworkDescriptor` already distinguishes `ForwardNetwork`, nullable
`ReverseNetwork`, and `NetworkCouplingType`.

Current Core has no reverse-flow relationships. Therefore alpha.5 emits only
`ForwardOnly` with a null `ReverseNetwork`. Reverse and closed-loop values are
activated only when real return/collection/repair/remanufacturing/recycling/
disposal source data and mathematical balances are introduced.
