# MSIX Packaging Guide for TidyPackRat

This guide explains how to package TidyPackRat as an MSIX for Microsoft Store distribution.

## Why MSIX?

Microsoft Store requires MSIX format for desktop app submissions. Benefits include:
- Clean installation and uninstallation
- Automatic updates via Store
- App sandboxing and security
- Digital signature verification

## Prerequisites

1. **Windows 10 SDK** (10.0.17763.0 or later)
2. **MSIX Packaging Tool** (free from Microsoft Store)
3. **Visual Studio 2019/2022** with:
   - .NET desktop development workload
   - Windows application packaging project template
4. **Code signing certificate** (for non-Store testing)

## Option 1: Windows Application Packaging Project (Recommended)

### Step 1: Add Packaging Project

1. In Visual Studio, right-click the solution
2. Add → New Project → Windows Application Packaging Project
3. Name it `TidyPackRat.Package`
4. Set target version: Windows 10 version 1903 (10.0.18362.0)
5. Set minimum version: Windows 10 version 1903 (10.0.18362.0)

### Step 2: Configure Package

1. Right-click the packaging project → Add Reference
2. Select TidyPackRat GUI project
3. In Applications, right-click TidyPackRat → Set as Entry Point

### Step 3: Edit Package.appxmanifest

```xml
<?xml version="1.0" encoding="utf-8"?>
<Package xmlns="http://schemas.microsoft.com/appx/manifest/foundation/windows10"
         xmlns:uap="http://schemas.microsoft.com/appx/manifest/uap/windows10"
         xmlns:rescap="http://schemas.microsoft.com/appx/manifest/foundation/windows10/restrictedcapabilities">

  <Identity Name="TidyPackRat"
            Publisher="CN=YourPublisher"
            Version="1.1.0.0"
            ProcessorArchitecture="x64" />

  <Properties>
    <DisplayName>TidyPackRat</DisplayName>
    <PublisherDisplayName>ProfessorMoose74</PublisherDisplayName>
    <Logo>Assets\StoreLogo.png</Logo>
    <Description>Automatically organize your Downloads folder. Set it once, stay tidy forever.</Description>
  </Properties>

  <Dependencies>
    <TargetDeviceFamily Name="Windows.Desktop" MinVersion="10.0.18362.0" MaxVersionTested="10.0.22621.0" />
  </Dependencies>

  <Resources>
    <Resource Language="en-us" />
  </Resources>

  <Applications>
    <Application Id="TidyPackRat"
                 Executable="TidyPackRat.exe"
                 EntryPoint="Windows.FullTrustApplication">
      <uap:VisualElements
        DisplayName="TidyPackRat"
        Description="Automatic file organizer for Windows"
        BackgroundColor="#2C3E50"
        Square150x150Logo="Assets\Square150x150Logo.png"
        Square44x44Logo="Assets\Square44x44Logo.png">
        <uap:DefaultTile Wide310x150Logo="Assets\Wide310x150Logo.png" Square71x71Logo="Assets\SmallTile.png" Square310x310Logo="Assets\LargeTile.png">
          <uap:ShowNameOnTiles>
            <uap:ShowOn Tile="square150x150Logo" />
            <uap:ShowOn Tile="wide310x150Logo" />
            <uap:ShowOn Tile="square310x310Logo" />
          </uap:ShowNameOnTiles>
        </uap:DefaultTile>
      </uap:VisualElements>
    </Application>
  </Applications>

  <Capabilities>
    <rescap:Capability Name="runFullTrust" />
  </Capabilities>
</Package>
```

### Step 4: Include Required Files

The package needs to include:
- TidyPackRat.exe (GUI)
- Newtonsoft.Json.dll
- TidyPackRat-Worker.ps1
- default-config.json

Add a post-build event or manual copy:
```xml
<ItemGroup>
  <Content Include="..\src\worker\TidyPackRat-Worker.ps1">
    <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
  </Content>
  <Content Include="..\config\default-config.json">
    <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
  </Content>
</ItemGroup>
```

### Step 5: Generate Package

1. Right-click packaging project → Publish → Create App Packages
2. Select "Sideloading" for testing or "Microsoft Store" for submission
3. Choose architectures (x64 recommended, optionally x86 and ARM64)
4. Configure signing certificate
5. Build and validate

## Option 2: MSIX Packaging Tool (Convert Existing MSI)

### Step 1: Install MSIX Packaging Tool

Download from Microsoft Store (free)

### Step 2: Prepare Clean VM

- Create a clean Windows 10/11 VM or restore a snapshot
- Don't install TidyPackRat on this machine yet

### Step 3: Convert MSI to MSIX

1. Launch MSIX Packaging Tool
2. Select "Application package"
3. Choose "Create package on this computer"
4. Browse to TidyPackRat-Setup.msi
5. Enter package information:
   - Package name: TidyPackRat
   - Publisher: CN=YourPublisher
   - Version: 1.1.0.0
6. Let the tool capture the installation
7. Review captured files and registry entries
8. Generate the MSIX package

## Required Package Assets

Create these images from the logo:

| Asset | Size | Purpose |
|-------|------|---------|
| Square44x44Logo.png | 44x44 | App list, taskbar |
| Square44x44Logo.targetsize-24.png | 24x24 | Small icon |
| Square44x44Logo.targetsize-48.png | 48x48 | Medium icon |
| Square44x44Logo.altform-unplated.png | 44x44 | Unplated icon |
| Square71x71Logo.png | 71x71 | Small tile |
| Square150x150Logo.png | 150x150 | Medium tile |
| Wide310x150Logo.png | 310x150 | Wide tile |
| Square310x310Logo.png | 310x310 | Large tile |
| StoreLogo.png | 50x50 | Store listing |
| SplashScreen.png | 620x300 | Splash screen (optional) |

**Tip:** Use the [Windows App Assets Generator](https://www.microsoft.com/store/apps/9nblggh4r3c1) to generate all sizes from a single source image.

## Special Considerations for TidyPackRat

### Task Scheduler Access

MSIX apps run in a sandboxed environment. Task Scheduler operations may require:
1. Use `runFullTrust` capability (already included)
2. Create scheduled task using PowerShell from within the package context

### ProgramData Access

The app stores config in `C:\ProgramData\TidyPackRat\`. This works with `runFullTrust` capability.

### PowerShell Script Execution

The Worker script requires PowerShell. Ensure execution policy allows running scripts, or use:
```powershell
powershell.exe -ExecutionPolicy Bypass -File "TidyPackRat-Worker.ps1"
```

## Testing the MSIX Package

### Local Testing (Sideload)

1. Enable Developer Mode in Windows Settings
2. Double-click the .msix file to install
3. Or use PowerShell:
   ```powershell
   Add-AppPackage -Path "TidyPackRat_1.1.0.0_x64.msix"
   ```

### Verify Installation

1. App appears in Start Menu
2. Settings are saved to ProgramData
3. Scheduled tasks can be created
4. Worker script runs correctly
5. Logs are written properly

### Test Uninstallation

1. Uninstall via Settings → Apps
2. Verify ProgramData folder handling (may need cleanup note for users)

## Store Submission Process

1. **Create Developer Account**
   - Go to [Partner Center](https://partner.microsoft.com/dashboard)
   - One-time registration fee (~$19 for individuals)

2. **Reserve App Name**
   - Create new app submission
   - Reserve "TidyPackRat"

3. **Upload Package**
   - Upload the signed MSIX
   - Store validates the package

4. **Complete Listing**
   - Add screenshots (use STORE_LISTING.md content)
   - Add description and keywords
   - Set pricing (Free)
   - Link privacy policy

5. **Submit for Certification**
   - Microsoft reviews the app
   - Typically takes 1-3 business days
   - May require fixes if issues found

## Troubleshooting

### Package fails to install
- Check Windows version meets minimum requirements
- Verify certificate is trusted or enable Developer Mode
- Check Event Viewer for detailed errors

### App crashes on launch
- Run from command line to see errors
- Check that all dependencies are included
- Verify paths work from packaged location

### Task Scheduler not working
- Ensure `runFullTrust` capability is declared
- Check task runs under correct user context
- Verify PowerShell execution policy

## Resources

- [MSIX Documentation](https://docs.microsoft.com/en-us/windows/msix/)
- [Package a desktop app from source code](https://docs.microsoft.com/en-us/windows/msix/desktop/desktop-to-uwp-packaging-dot-net)
- [MSIX Packaging Tool](https://docs.microsoft.com/en-us/windows/msix/packaging-tool/tool-overview)
- [Partner Center](https://partner.microsoft.com/dashboard)
