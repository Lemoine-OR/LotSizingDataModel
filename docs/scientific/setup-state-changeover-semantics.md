# Setup-State and Changeover Semantics

alpha.24 distinguishes four concepts:

1. ordinary setup cost/time already associated with production activation;
2. start-up cost/time introduced in alpha.21 for the beginning of a setup
   sequence;
3. setup state carried between buckets;
4. directional sequence-dependent changeover from item A to item B.

A `ProductionChangeover` is ordered:

`A -> B != B -> A`.

It may contain:

- `SequenceDependentChangeoverTime`;
- `SequenceDependentChangeoverCost`.

Universal tokens:

- `InitSetup`;
- `SCO`;
- `SDCT`;
- `SDCC`.

Wolsey machine features are mapped as:

- `SQT -> SDCT`;
- `SQC -> SDCC`.

The mapping does not infer Wolsey machine mode or bucket semantics.
