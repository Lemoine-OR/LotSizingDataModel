# Scientific Classification Engine — alpha.16

`ScientificClassificationEngine` is the UI-independent consolidation point for
the current LotSizingDataModel scientific-analysis stack.

For a typed descriptor it produces:

- detected universal notation;
- declared-notation comparison when a declaration is supplied;
- canonical lot-sizing problem-class matches;
- one primary class only when the match is unique;
- explicit core extensions;
- historical-classification mapping capability;
- structured `LSDM-SCI-*` diagnostics;
- explicit coverage of the scientific axes.

For a `SupplyChain`, it first runs the existing product-structure and feature
extractors and enriches the descriptor with the physical-network analyzer.

For a `LotSizingInstance`, it first composes `InstanceModelChecker`. A blocking
model-check result prevents classification.

## No rule duplication

The engine orchestrates:

- `ProductStructureAnalyzer`;
- `LotSizingProblemFeatureExtractor`;
- `LotSizingProblemDescriptor`;
- `UniversalNotationGenerator`;
- `UniversalNotationMatcher`;
- `LotSizingProblemClassDetector`;
- Bitran-Yanasse applicability;
- existing historical mappers/capabilities.

It does not copy their scientific rules.

## Axis coverage

The engine currently marks:

- Structural properties: `Analyzed`;
- Problem classes: `Analyzed`;
- Historical classifications: `CapabilityOnly`;
- Planning paradigms: `NotInferred`;
- Mathematical formulations: `NotInferred`;
- Solution methods: `NotInferred`.

Therefore a detected `MI-CLSP`, for example, does not imply DRP/MRP, a
particular MILP formulation or a particular solving algorithm.

## Diagnostics

Initial stable namespace:

- `LSDM-SCI-001`: instance validation blocks classification;
- `LSDM-SCI-002`: product-structure analysis warning;
- `LSDM-SCI-003`: product-structure analysis error;
- `LSDM-SCI-010`: invalid declared notation;
- `LSDM-SCI-011`: declared notation is compatible but less specific;
- `LSDM-SCI-012`: declared notation requires unavailable analysis;
- `LSDM-SCI-013`: declared notation contradicts detected semantics;
- `LSDM-SCI-020`: no executable canonical problem class detected;
- `LSDM-SCI-021`: multiple canonical problem classes remain compatible.
