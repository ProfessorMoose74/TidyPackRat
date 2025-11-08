# TidyPackRat Tools

This directory contains utility scripts for TidyPackRat development.

## Asset Preparation

### prepare-assets.ps1
Processes the logo image and prepares it for use in the project.

**Usage:**
```powershell
.\prepare-assets.ps1 -LogoPath "C:\path\to\logo.png"
```

**What it does:**
- Copies logo to `assets/logo.png`
- Creates necessary directory structure
- Generates resized versions
- Provides instructions for ICO creation

### update-project-icon.ps1
Updates the GUI project to use the application icon.

**Usage:**
```powershell
.\update-project-icon.ps1
```

**What it does:**
- Checks for icon.ico file
- Updates project file if needed
- Configures icon as project resource

## Quick Start with New Logo

1. Save your logo as PNG
2. Run: `.\tools\prepare-assets.ps1 -LogoPath "path\to\logo.png"`
3. Create ICO file (see instructions in script output)
4. Run: `.\tools\update-project-icon.ps1`
5. Rebuild: `.\build.ps1`

## Creating ICO Files

**Option 1: Online Tool (Easiest)**
- Go to https://convertio.co/png-ico/
- Upload your logo PNG
- Select sizes: 16, 32, 48, 256
- Download and save as `src/gui/Assets/icon.ico`

**Option 2: ImageMagick (If Installed)**
```powershell
magick convert logo.png -define icon:auto-resize=256,64,48,32,16 icon.ico
```

**Option 3: GIMP (Free Software)**
- Open logo PNG in GIMP
- Export As → .ico
- Select multiple sizes in export dialog
