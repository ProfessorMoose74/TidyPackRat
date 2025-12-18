# MSIX Packaging Guide for TidyFlow

This guide explains how to package TidyFlow as an MSIX for Microsoft Store distribution.

## Why MSIX?

Microsoft Store requires MSIX format for desktop app submissions. Benefits include:
- Clean installation and uninstallation
- Automatic updates via Store
- App sandboxing and security
- Digital signature verification

## Prerequisites

1. **Windows 10 SDK** (10.0.26100.0 or later recommended)
2. **Visual Studio 2022** with:
   - .NET desktop development workload
   - Windows application packaging project template
3. **Code signing certificate** (for non-Store testing)

## Quick Build (Command Line)

The project includes a pre-configured packaging project. To build the MSIX:

```bash
# Restore and build the solution
msbuild TidyFlow.sln -p:Configuration=Release -p:Platform=x64 -t:Restore
msbuild TidyFlow.sln -p:Configuration=Release -p:Platform=x64

# Output will be at:
# dist\msix\TidyFlow.Package_1.2.0.0_x64_Test\TidyFlow.Package_1.2.0.0_x64.msix
```

## Project Structure

The MSIX packaging is handled by the `TidyFlow.Package` project:

```
src/TidyFlow.Package/
├── Package.appxmanifest    # App identity, capabilities, visual elements
├── TidyFlow.Package.wapproj # MSBuild packaging project
└── Images/                  # MSIX tile and icon assets
    ├── StoreLogo.png        # 50x50 - Store listing
    ├── Square44x44Logo.png  # App list, taskbar
    ├── Square150x150Logo.png # Medium tile
    ├── Wide310x150Logo.png  # Wide tile
    ├── LargeTile.png        # 310x310 - Large tile
    ├── SmallTile.png        # 71x71 - Small tile
    ├── SplashScreen.png     # 620x300 - Splash screen
    └── BadgeLogo.png        # 24x24 - Badge notifications
```

## Regenerating Image Assets

If you need to regenerate the MSIX image assets from the source logo:

```powershell
.\tools\Generate-MsixAssets.ps1
```

This creates all required sizes from `assets/logo.png`.

## Package Configuration

### Package.appxmanifest

The manifest defines the app identity and capabilities:

```xml
<Identity Name="TidyFlow"
          Publisher="CN=ProfessorMoose74"
          Version="1.2.0.0"
          ProcessorArchitecture="x64" />
```

Key capabilities:
- `runFullTrust` - Required for Task Scheduler access and ProgramData storage
- File type association for `.tfconfig` files
- Startup task registration for "run on startup" feature

### Included Files

The package automatically includes:
- `TidyFlow.exe` - Main GUI application
- `Newtonsoft.Json.dll` - JSON library
- `TidyFlow-Worker.ps1` - PowerShell worker script
- `default-config.json` - Default configuration

## Building in Visual Studio

1. Open `TidyFlow.sln` in Visual Studio 2022
2. Set configuration to **Release** and platform to **x64**
3. Right-click `TidyFlow.Package` → **Publish** → **Create App Packages**
4. Select **Sideloading** for testing or **Microsoft Store** for submission
5. Configure signing (see below)
6. Build and validate

## Code Signing

### For Local Testing (Sideload)

1. Enable Developer Mode in Windows Settings → For developers
2. The package is built without signing (`AppxPackageSigningEnabled=false`)
3. Install using the `Install.ps1` script in the output folder

### For Microsoft Store

1. Associate with your Store identity in Visual Studio
2. The Store will sign the package automatically during submission
3. No local certificate needed

### For Enterprise Distribution

Create a self-signed certificate:
```powershell
New-SelfSignedCertificate -Type Custom -Subject "CN=YourCompany" `
  -KeyUsage DigitalSignature -FriendlyName "TidyFlow Signing" `
  -CertStoreLocation "Cert:\CurrentUser\My" `
  -TextExtension @("2.5.29.37={text}1.3.6.1.5.5.7.3.3")
```

## Testing the MSIX Package

### Local Testing (Sideload)

1. Enable Developer Mode in Windows Settings
2. Navigate to the output folder: `dist\msix\TidyFlow.Package_1.2.0.0_x64_Test\`
3. Run `Install.ps1` or double-click the `.msix` file
4. Or use PowerShell:
   ```powershell
   Add-AppPackage -Path "TidyFlow.Package_1.2.0.0_x64.msix"
   ```

### Verify Installation

1. App appears in Start Menu as "TidyFlow"
2. Settings are saved to `C:\ProgramData\TidyFlow\`
3. Scheduled tasks can be created
4. Worker script runs correctly
5. Logs are written to `C:\ProgramData\TidyFlow\logs\`

### Test Uninstallation

1. Uninstall via Settings → Apps
2. Configuration and logs in ProgramData remain (user data)

## Microsoft Store Submission

### 1. Create Developer Account

- Go to [Partner Center](https://partner.microsoft.com/dashboard)
- One-time registration fee (~$19 for individuals)

### 2. Reserve App Name

- Create new app submission
- Reserve "TidyFlow"

### 3. Associate Package with Store

In Visual Studio:
1. Right-click `TidyFlow.Package` → **Publish** → **Associate App with the Store**
2. Sign in with your Partner Center account
3. Select the reserved app name
4. This updates the manifest with Store identity

### 4. Create Store Package

1. Right-click `TidyFlow.Package` → **Publish** → **Create App Packages**
2. Select **Microsoft Store** (not Sideloading)
3. Choose architectures (x64)
4. Build the package

### 5. Upload and Submit

1. Upload the `.msixupload` file to Partner Center
2. Complete the Store listing:
   - Add screenshots
   - Add description (see `STORE_LISTING.md`)
   - Set pricing: **Free with ads**
   - Link privacy policy (see `PRIVACY.md`)
3. Submit for certification

### 6. Certification

- Microsoft reviews the app
- Typically takes 1-3 business days
- You'll receive email notification of approval or required fixes

## Special Considerations

### Task Scheduler Access

MSIX apps run in a sandboxed environment. Task Scheduler operations work because:
- `runFullTrust` capability is declared
- The app creates tasks using COM interfaces
- Tasks run under the user's context

### ProgramData Access

The app stores config in `C:\ProgramData\TidyFlow\`. This works with `runFullTrust` capability and persists across app updates.

### PowerShell Script Execution

The Worker script is executed with:
```powershell
powershell.exe -ExecutionPolicy Bypass -File "TidyFlow-Worker.ps1"
```

## Troubleshooting

### Package fails to install
- Check Windows version meets minimum requirements (Windows 10 1903+)
- Enable Developer Mode for unsigned packages
- Check Event Viewer → Applications and Services Logs → Microsoft → Windows → AppxDeployment-Server

### App crashes on launch
- Run from command line to see errors
- Check that all dependencies are included in the package
- Verify paths work from packaged location

### Task Scheduler not working
- Ensure `runFullTrust` capability is declared
- Run the app as the user who needs the scheduled task
- Check Task Scheduler for errors

### Store certification fails
- Run Windows App Certification Kit (WACK) locally first
- Check for prohibited API calls
- Ensure privacy policy is accessible

## Resources

- [MSIX Documentation](https://docs.microsoft.com/en-us/windows/msix/)
- [Package a desktop app](https://docs.microsoft.com/en-us/windows/msix/desktop/desktop-to-uwp-packaging-dot-net)
- [Partner Center](https://partner.microsoft.com/dashboard)
- [Windows App Certification Kit](https://docs.microsoft.com/en-us/windows/uwp/debug-test-perf/windows-app-certification-kit)
