# Optional: Building an MSIX Package

TidyFlow is released as a portable `TidyFlow.exe` on [GitHub Releases](https://github.com/ProfessorMoose74/TidyPackRat/releases). That's what almost everyone should use.

The repository also contains an MSIX packaging project for people who'd rather have TidyFlow as an installed app, with:

- a Start menu entry and an entry in **Settings > Apps** to uninstall it,
- the `tidyflow.exe` app execution alias, which the scheduled task uses and which stays the same across package updates,
- a Windows startup task (shown in **Settings > Apps > Startup**) for "Start TidyFlow when I sign in",
- double-click to import `.tfconfig` files.

No MSIX package is published anywhere, and CI doesn't build one. You build it, sign it with your own certificate and install it yourself. This is an advanced option.

## Overview

- `src/TidyFlow.Package` is a Windows Application Packaging project (`.wapproj`) that wraps the WPF app in `src/TidyFlow`.
- The app is packaged **self-contained** (the .NET runtime is inside the package).
- `build.ps1 -Package` produces an **x64 + ARM64 bundle**.
- Package identity in the manifest: Name `ElementalGeniusLLC.TidyFlow`, Publisher `CN=29741907-368C-452F-B311-240CE1EF415D`, display name **Elemental Genius LLC**.
- The package is built **unsigned** (`AppxPackageSigningEnabled=false`).

## Prerequisites

| Tool | Why |
|---|---|
| [.NET 10 SDK](https://dot.net) | Builds the app |
| Visual Studio 2022 17.14+ or Visual Studio 2026 with the **Windows application development** workload | Provides the MSIX packaging tools that the `.wapproj` needs. Plain `dotnet build TidyFlow.slnx` skips the packaging project. |
| Windows SDK (installed with that workload) | `signtool.exe`, to sign the package |
| Developer Mode (Windows Settings > System > For developers) | Only for registering the unsigned package for local testing |

## Build the package

```powershell
.\build.ps1 -Package              # build, test, then package
.\build.ps1 -Package -SkipTests
```

The output goes to `dist\msix`. `build.ps1` finds MSBuild with `vswhere` and builds `TidyFlow.slnx` with `AppxBundle=Always` and `AppxBundlePlatforms=x64|arm64`. Look under `dist\msix` for the `.msixbundle` (the build also writes an `.msixupload`, which you can ignore).

## Choose how to install it

| Goal | Use |
|---|---|
| Quick local testing while developing | [Register the unsigned layout](#test-without-signing-developer-mode) (Developer Mode) |
| Install it properly on your own PC(s) | [Sign it with your own certificate](#sign-and-install), then install the `.msixbundle` |

### Test without signing (Developer Mode)

**Visual Studio**

1. Open `TidyFlow.slnx`.
2. Set **TidyFlow.Package** as the startup project and the platform to **x64**.
3. Press **F5**. Visual Studio registers the package and starts it.

**Command line**

```powershell
# Build the package layout (or build TidyFlow.Package as Release|x64 in Visual Studio)
.\build.ps1 -Package -SkipTests

# Register it
Add-AppxPackage -Register src\TidyFlow.Package\bin\x64\Release\AppxManifest.xml

# Remove it again
Get-AppxPackage ElementalGeniusLLC.TidyFlow | Remove-AppxPackage
```

### Sign and install

Windows only installs an `.msix`/`.msixbundle` file if it's signed by a certificate your PC trusts, and **the certificate's subject must exactly match the `Publisher` in `Package.appxmanifest`**. The manifest's current Publisher (`CN=29741907-368C-452F-B311-240CE1EF415D`) isn't a certificate you have, so change it to your own.

1. Create a code-signing certificate. A self-signed one is fine for your own PCs:

   ```powershell
   $cert = New-SelfSignedCertificate -Type Custom -Subject "CN=Your Name" `
       -KeyUsage DigitalSignature -FriendlyName "TidyFlow package signing" `
       -CertStoreLocation "Cert:\CurrentUser\My" `
       -TextExtension @("2.5.29.37={text}1.3.6.1.5.5.7.3.3", "2.5.29.19={text}")
   $password = Read-Host -AsSecureString "PFX password"
   Export-PfxCertificate -Cert $cert -FilePath TidyFlowSigning.pfx -Password $password
   Export-Certificate -Cert $cert -FilePath TidyFlowSigning.cer
   ```

2. In `src/TidyFlow.Package/Package.appxmanifest`, set `Publisher` on the `Identity` element to your certificate's subject, exactly (for example `CN=Your Name`). You can also change `Name` and `PublisherDisplayName` if you like. Don't commit these changes.
3. Build: `.\build.ps1 -Package`.
4. Sign the bundle (from a **Developer PowerShell for VS**, so `signtool` is on the path):

   ```powershell
   signtool sign /fd SHA256 /f TidyFlowSigning.pfx /p <password> dist\msix\<path-to>\TidyFlow.Package_2.0.0.0_x64_arm64.msixbundle
   ```

5. Trust the certificate on each PC where you'll install it: import `TidyFlowSigning.cer` into **Local Machine > Trusted People** (double-click the `.cer` > **Install Certificate** > **Local Machine** > **Trusted People**; needs admin).
6. Double-click the `.msixbundle` and select **Install**, or run `Add-AppxPackage <path>.msixbundle`.

Keep the `.pfx` private: anyone with it can sign packages your PC will trust. To update later, build and sign a higher version with the same certificate.

Don't use the MSIX package and the portable build at the same time: they share the scheduled task name (`TidyFlow-AutoOrganize`). Run `TidyFlow.exe --uninstall` for the portable build first.

## Project structure

```
src/TidyFlow.Package/
├── Package.appxmanifest        # identity, version, capabilities, extensions
├── TidyFlow.Package.wapproj    # packaging project (x64 and ARM64; output to dist\msix)
└── Images/                     # tiles, splash screen and package logo (generated)
```

### Regenerating images

All images are generated from `assets/logo.png`:

```powershell
.\tools\Generate-MsixAssets.ps1   # MSIX images in src/TidyFlow.Package/Images
.\tools\New-AppIcon.ps1           # app icon src/TidyFlow/Assets/icon.ico
```

## Package.appxmanifest

| Element | Purpose |
|---|---|
| `Identity Name` / `Publisher` | `Publisher` must match the subject of the certificate that signs the package. |
| `Identity Version="2.0.0.0"` | Kept in step with `<Version>` in `Directory.Build.props`, with the fourth part `0`. |
| `TargetDeviceFamily MinVersion="10.0.19041.0"` | Windows 10 version 2004 or later. |
| `rescap:Capability runFullTrust` | Required for a packaged desktop (WPF) app. |
| `windows.fileTypeAssociation` (`.tfconfig`) | Double-clicking an exported settings file opens TidyFlow and offers to import it. |
| `windows.appExecutionAlias` (`tidyflow.exe`) | A path that never changes between updates, `%LOCALAPPDATA%\Microsoft\WindowsApps\tidyflow.exe`. The scheduled task runs `tidyflow.exe --run` through it. |
| `windows.startupTask` (`TidyFlowStartup`, off by default) | "Start TidyFlow when I sign in to Windows". Users can also toggle it in Windows Settings > Apps > Startup. |
| `windows.comServer` + `windows.toastNotificationActivation` | Lets Windows start TidyFlow when a notification is clicked. |

Extensions name the executable literally (`TidyFlow\TidyFlow.exe`); the build only substitutes `$targetnametoken$` on `<Application>`.

Maintainers bump `Identity Version` together with `<Version>` at each release (see [Releasing](../CONTRIBUTING.md#releasing-maintainers)), so the package stays in step even though it isn't published.

## What's different in the packaged app

- **Settings > About** says "MSIX package".
- **Settings > Open data folder** opens `%LOCALAPPDATA%\Packages\ElementalGeniusLLC.TidyFlow_<id>\LocalCache\Local\TidyFlow` (the Name part follows your manifest). Uninstalling the package removes this folder.
- Saving a schedule creates **TidyFlow-AutoOrganize** in Task Scheduler, running `%LOCALAPPDATA%\Microsoft\WindowsApps\tidyflow.exe --run`. Right-click > **Run** organizes with no window and adds a **Scheduled run** to History.
- **Start TidyFlow when I sign in** uses the package startup task and appears in Windows Settings > Apps > Startup.
- Double-clicking a `.tfconfig` file opens TidyFlow and offers to import it.
- Notifications appear for background runs, and clicking one opens TidyFlow.
- `--uninstall` just tells you to uninstall from **Settings > Apps**. Uninstalling doesn't remove the scheduled task: turn off the schedule and Save first, or delete the task in Task Scheduler afterwards.

Tip: set `TIDYFLOW_DATA_DIR` only for unpackaged testing; for the packaged app, test with a throwaway source folder instead.

## Troubleshooting

| Problem | Fix |
|---|---|
| `build.ps1 -Package` says Visual Studio is required | Install the **Windows application development** workload (MSIX packaging tools). |
| `dotnet build` doesn't produce a package | Expected; the `.wapproj` needs Visual Studio's MSBuild. Use `build.ps1 -Package`. |
| Installing the `.msixbundle` says it isn't signed or the publisher isn't trusted | Sign it as above, and import the `.cer` into **Local Machine > Trusted People**. |
| `signtool` fails with a publisher mismatch, or install fails with `0x8007000B` | The manifest's `Publisher` doesn't exactly match your certificate's subject. Fix the manifest and rebuild. |
| `Add-AppxPackage -Register` fails | Turn on Developer Mode, and remove any installed copy first (`Get-AppxPackage ElementalGeniusLLC.TidyFlow \| Remove-AppxPackage`). Details are in Event Viewer > Applications and Services Logs > Microsoft > Windows > AppXDeployment-Server. |
| Scheduled task doesn't work in the packaged build | Check the task's action is the `WindowsApps\tidyflow.exe` alias and that `--run` works from a command prompt (`tidyflow.exe --run`, then check `%ERRORLEVEL%`). |

## Resources

- [MSIX documentation](https://learn.microsoft.com/windows/msix/)
- [Package a desktop app with Visual Studio](https://learn.microsoft.com/windows/msix/desktop/desktop-to-uwp-packaging-dot-net)
- [Create a certificate for package signing](https://learn.microsoft.com/windows/msix/package/create-certificate-package-signing)
- [Sign an app package using SignTool](https://learn.microsoft.com/windows/msix/package/sign-app-package-using-signtool)
