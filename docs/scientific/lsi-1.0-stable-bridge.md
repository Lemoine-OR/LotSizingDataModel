# LSI/1.0 bridge over the stable scientific classification architecture

LSI/1.0 is maintained as a versioned scientific projection:

`LSI/1.0: pi{...} | alpha{...} | beta{...} | gamma{...} @ sigma{...}`

The stable 1.2.x semantic chain remains authoritative:

`SupplyChain -> LotSizingProblemFeatureExtractor -> LotSizingProblemDescriptor
-> UniversalNotationGenerator -> ScientificClassificationEngine`

LSI/1.0 is generated from the typed descriptor and Universal Notation. It does
not independently inspect Core scheduling or cost objects and therefore does
not create a competing semantic truth.

## Intended uses

- benchmark registries;
- scientific tables;
- literature compatibility;
- exact dimension reporting;
- historical family projection;
- cross-version reproducibility.

## Unknown values

LSI keeps unknown information explicit. A missing fact is never silently
interpreted as absence.

## Compatibility

The historical family projection remains a convenience view:

- LS-U;
- LS-C;
- CLSP;
- MLLP;
- MLCLSP.

The richer Universal Notation and Scientific Classification remain the
preferred APIs for machine reasoning inside LotSizingDataModel.
