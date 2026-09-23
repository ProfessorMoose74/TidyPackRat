# TidyFlow Assets

This directory contains visual assets for the TidyFlow project.

## Logos
- `logo.png` - Main project logo (pack rat mascot with glasses). Source for the app icon and MSIX images.
- `TidyFlow-logo.png` - Title logo shown at the top of README.md
- `title-logo.png` - Original "TIDY PACK RAT" title artwork (from before the rename)

## Icons
- `logo.ico` - Mascot icon (multiple sizes)
- The app's own icon is `src/TidyFlow/Assets/icon.ico`, built from `logo.png` by `tools/New-AppIcon.ps1`.
- The tile and splash images for the optional MSIX package in `src/TidyFlow.Package/Images` are built by `tools/Generate-MsixAssets.ps1`.

See [tools/README.md](../tools/README.md) for how to update them after changing the logo.

## Screenshots
Used by README.md and the docs:

| File | Shows |
|---|---|
| `screenshots/dashboard.png` | Dashboard with statistics and recently organized files |
| `screenshots/preview.png` | Preview changes window |
| `screenshots/history.png` | History tab with Undo |
| `screenshots/rules.png` | Rules tab with categories |

When the UI changes, retake them and keep the same file names so the links keep working.

## Credits
Logo designed by Copilot for the TidyFlow project.
