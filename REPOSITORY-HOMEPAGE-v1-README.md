# Repository homepage patch v1

This patch replaces the repository root `README.md` with a professional,
release-aware project homepage.

## Dynamic elements

Nothing is hard-coded to a specific release number.

The page uses:

- the native GitHub Actions badge for `build.yml`;
- the native GitHub Actions badge for `documentation.yml`;
- a dynamic latest-release badge;
- a latest-release link;
- the existing GitHub Pages documentation URL;
- the existing project logo and hero image in `docs/assets`.

Future releases therefore update the visible release badge automatically.

## Merge

Extract at:

`D:\Dev\LotSizingDataModel`

Then review the rendered Markdown locally or directly on GitHub.

Suggested commit:

`Create professional repository homepage`

After push, the repository root page will be updated automatically.
