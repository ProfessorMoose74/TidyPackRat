# TidyFlow tools

Scripts for maintaining TidyFlow's artwork. Both read the mascot logo at `assets/logo.png` by default.

| Script | What it does |
|---|---|
| `New-AppIcon.ps1` | Builds `src/TidyFlow/Assets/icon.ico` (16–256 px) used for the window, taskbar and notification area. |
| `Generate-MsixAssets.ps1` | Builds the tile, splash and Store images in `src/TidyFlow.Package/Images` for the MSIX package. |

## Updating the logo

```powershell
Copy-Item C:\path\to\new-logo.png assets\logo.png
.\tools\New-AppIcon.ps1
.\tools\Generate-MsixAssets.ps1
.\build.ps1
```

Use a square PNG with a transparent background, at least 512×512.
