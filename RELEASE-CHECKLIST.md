# Official release 2.0.1

1. Align `version.json`, the software version in `CITATION.cff`, and current API/XML/release contracts; retain historical release notes.
2. Review [2.0.1 release notes](docs/integration/stable-2.0.1-release-notes.md), migration guidance and explicit scientific non-claims.
3. Run automation and stable promotion checks, the complete Release build and automated tests, and relevant native solver validation.
4. Push the dedicated branch and require successful PR build/documentation checks for its current commit before merging.
5. After explicit publication authorization and successful main checks, dispatch `.github/workflows/release.yml` from `main`. The workflow performs a fresh public build, tests, documentation and artifact validation before creating the tag and stable release. Never replace an existing tag.
6. Verify the workflow conclusion, tag commit, published stable/latest release, expected assets and SHA-256 checksums.

Publication of 2.0.1 and merging the validated PR have been explicitly authorized by the repository owner for this task. The workflow remains manually dispatched; no automatic publication on ordinary pushes is introduced.

Historical alpha/1.2 preparation scripts are archival; `tools/Test-StablePromotionContract.ps1` checks the current stable release contract.
