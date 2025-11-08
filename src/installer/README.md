# TidyPackRat Installer

This directory contains the WiX installer project for TidyPackRat.

## Prerequisites

- WiX Toolset v3.11 or newer: https://wixtoolset.org/releases/
- Visual Studio 2017 or newer (for building)

## Building the Installer

1. Build the GUI application first (TidyPackRat.csproj)
2. Copy build outputs to the `build` directory structure:
   ```
   build/
   ├── gui/
   │   ├── TidyPackRat.exe
   │   ├── TidyPackRat.exe.config
   │   └── Newtonsoft.Json.dll
   ├── worker/
   │   └── TidyPackRat-Worker.ps1
   └── config/
       └── default-config.json
   ```
3. Build the installer project using MSBuild or Visual Studio

## Command Line Build

```bash
# From the installer directory
msbuild TidyPackRat.Installer.wixproj /p:Configuration=Release
```

## Output

The installer MSI will be created in `bin\Release\TidyPackRat-Setup.msi`
