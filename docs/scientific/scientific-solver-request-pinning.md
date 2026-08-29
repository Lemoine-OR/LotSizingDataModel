# Scientific Solver-Request Pinning

The ordinary `SolverRequest` supports an empty `FormulationName`, meaning the
technical formulation service may select a default formulation.

After alpha.17, scientific formulation selection is stricter than technical
`CanBuild`.

alpha.19 therefore converts:

`FormulationName = ""`

into a delegated request such as:

`FormulationName = "standard"`

only after scientific compatibility has been verified.

The original request remains unchanged.

If the caller explicitly requests a formulation, the scientific layer does
not silently fall back to another formulation. This mirrors the existing
technical `LotSizingSolverService` behavior, where a non-empty requested
formulation disables fallback.

After solver execution, the formulation reported by `SolverRunResult` must
equal the pinned scientific selection. This prevents provenance from recording
one formulation while another was actually executed.
