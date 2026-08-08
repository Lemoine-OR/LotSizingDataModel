# Review of the supplied `Doxyfile.common`

The uploaded file was generated from Doxygen 1.17.0, but it contained project-specific values and two malformed assignments that should not be in a shared configuration:

- `OUTPUT_DIRECTORY = "OUTPUT_DIRECTORY = Documentation"`
- `INPUT = "INPUT = LotSizingDataModel.Core"`

Those values would make Doxygen use the literal strings `OUTPUT_DIRECTORY = Documentation` and `INPUT = LotSizingDataModel.Core` as paths.

The previous GitHub workflow also installed Doxygen 1.10.0 through Chocolatey while the configuration was produced by 1.17.0. That version mismatch was the source of the earlier "unsupported tag" warnings.

This pack fixes the architecture as follows:

1. `docs/doxygen/Doxyfile.common` is now a curated shared configuration with no project-specific input/output path.
2. Every Visual Studio project has its own Doxyfile.
3. Every project has its own main page.
4. Doxygen 1.17.0 is downloaded from the official Doxygen site in CI and its SHA-256 is checked before execution.
5. A first Doxygen pass generates tag files for all projects.
6. A second pass generates HTML with cross-project links.
7. Generated HTML is assembled into one GitHub Pages portal.
8. Generated documentation remains outside Git because `Documentation/` and `dist/` are build outputs.
9. The project logo and icon are supplied in Doxygen-compatible sizes.
