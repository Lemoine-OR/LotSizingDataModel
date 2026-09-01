# LotSizingDataModel benchmark repository layout

Benchmark artifacts are separated by provenance and mutability:

- `raw/` — immutable source instances exactly as acquired;
- `annotated/` — LotSizingDataModel-enriched instances, including provenance,
  detected classifications and known-result metadata;
- `solutions/` — detailed candidate/reference solutions;
- `campaigns/` — reproducible JSON/CSV campaign reports and manifests.

Raw benchmark artifacts must never be overwritten by annotations or solutions.
