# Benchmark / BKS / Campaign Consolidation — alpha.42

## Native BKS status is reused

`KnownResult` already contains the benchmark result data needed for scientific
traceability: objective value, detailed solution when available, method,
bibliographic source, URI/DOI and `KnownResultVerificationStatus`.

Alpha.42 does not create a competing BKS-status enum.

The benchmark audit service interprets the native statuses conservatively:

- `AutomaticallyVerified` and `IndependentlyVerified` are eligible by default;
- `Reproduced` is eligible only when an explicit selection policy allows it;
- `SourceReported`, `NotVerified`, `Disputed` and `Invalidated` are not
  automatically eligible reference BKS values.

A disputed numerically better result therefore cannot silently replace an
independently verified reference.

## Source evidence

Reference selection can require bibliographic/documentary evidence.

Source evidence is satisfied by at least one of source title, source reference,
source URI or DOI.

Missing source evidence is retained as an explicit BKS diagnostic.

## Repository layout

Benchmark artifacts are separated into:

- `benchmarks/raw`;
- `benchmarks/annotated`;
- `benchmarks/solutions`;
- `benchmarks/campaigns`.

Raw files are immutable source evidence. Annotation never overwrites raw data,
and solutions never need to be embedded into raw source instances.

## Reproducible run provenance

Every benchmark run records:

- formulation ID;
- method ID and version;
- backend ID and version;
- stochastic/non-stochastic flag;
- explicit seed for every stochastic run;
- parameters sorted by ordinal key.

A stochastic run without a seed is invalid.

## Campaign records

Each campaign record can contain:

- instance ID and fingerprint;
- complete run provenance;
- objective and feasibility/optimality flags;
- elapsed time;
- selected BKS ID/value/native verification status;
- relative deterioration to BKS;
- alpha.41 historical Declared-vs-Detected audit snapshot.

For minimization:

`gap = max(0, candidate - BKS) / max(1, |BKS|)`.

For maximization the numerator is reversed.

The gap is a benchmark comparison metric; it is not an optimality proof unless
the BKS itself is a proven optimum.

## Deterministic report artifacts

`BenchmarkCampaignReportWriter` orders runs and emits:

- UTF-8 JSON;
- UTF-8 CSV;
- SHA-256 manifest.

Parameter keys are emitted in stable ordinal order.

This allows campaign artifacts to be versioned and compared without incidental
dictionary-order or locale differences.
