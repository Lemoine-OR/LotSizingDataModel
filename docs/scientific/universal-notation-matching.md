# Universal Notation Matching

`v1.2.0-alpha.8` gives semantic meaning to parsed universal notation by
interpreting it as a `UniversalProblemSpecification`.

The matcher returns exactly one of four states:

- `Exact`
- `Compatible`
- `Incomplete`
- `Contradiction`

## Exact

The specification's canonical text is exactly equal to the complete notation
generated from the descriptor.

## Compatible

Every explicit requirement is satisfied, but the descriptor contains
additional characteristics that the specification did not require.

A specification is intentionally a positive constraint set:

- a beta token that appears is required;
- a beta token that is absent is unconstrained;
- `CY`, `MS`, `TS` are required only when explicitly present;
- an omitted echelon count is unconstrained;
- `?`, `Level:?`, `Net:UNK`, and `Obj:?` act as unknown/wildcard positions.

This avoids treating a useful partial scientific query as a false
closed-world description.

## Incomplete

The specification requests a known value but the descriptor can only provide
`Unknown`. No contradiction is asserted.

Example:

`1,SL,Net:DIV | Dem | Obj:Econ`

matched against a descriptor whose physical topology has not yet been
analyzed.

## Contradiction

At least one known descriptor characteristic conflicts with an explicit
requirement.

Examples:

- expected `Net:DIV`, actual `Net:CONV`;
- expected `MS`, actual no multisourcing;
- required beta feature absent;
- expected `Obj:Multi`, actual `Obj:Econ`.

Contradiction has precedence over incomplete information.

## Stable diagnostics

Matcher explanations use the `LSDM-MATCH-*` namespace and expose:

- code;
- logical path;
- expected value;
- actual value;
- message;
- whether the issue is a proven contradiction.

## Core invariant

For every descriptor:

`Match(descriptor, Generate(descriptor)) == Exact`

The next historical-mapping layer can therefore project literature
classifications to universal specifications and use the same matcher to test
whether an arbitrary instance belongs to, extends, or contradicts the
historical class.
