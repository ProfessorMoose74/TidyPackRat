# Contributing to TidyFlow

Thanks for considering a contribution! Bug reports, ideas, docs fixes and code are all welcome.

## Code of conduct

This project follows the [Contributor Covenant](CODE_OF_CONDUCT.md). By taking part, you agree to uphold it.

## Security issues

Please don't report vulnerabilities in public issues. See [SECURITY.md](SECURITY.md) for how to report them privately.

## Reporting bugs

Check [existing issues](https://github.com/ProfessorMoose74/TidyPackRat/issues) first, then [open a new one](https://github.com/ProfessorMoose74/TidyPackRat/issues/new/choose) using the **Bug report** form. A good report includes:

- A clear title and exact steps to reproduce
- What you expected and what happened (screenshots help)
- Your Windows version and TidyFlow version (Settings > About shows the version and whether it's the portable build or the MSIX package)
- The relevant lines from the log (Settings > **Open log folder**, file `TidyFlow-YYYY-MM.log`). Remove any file names you'd rather not share.

## Suggesting enhancements

Open an issue with the **Feature request** form, describing the idea, why it would be useful and, if relevant, how similar apps handle it.

## Pull requests

1. Fork the repo and branch from `main`.
2. Make your change, following the conventions below.
3. Add or update tests for engine changes.
4. Make sure `.\build.ps1` passes (build with no warnings, all tests green).
5. Update the docs, and add an entry to [CHANGELOG.md](CHANGELOG.md) under `## [Unreleased]` for user-facing changes.
6. Open the pull request and fill in the template. CI builds and tests every pull request and attaches portable builds you can try.

## Development setup

### Prerequisites

- [.NET 10 SDK](https://dot.net) (`global.json` pins 10.0.100 or a later feature band)
- Git
- Optional, only for the optional MSIX package: Visual Studio 2022 17.14+ or Visual Studio 2026 with the **Windows application development** workload

Any editor works for the app and engine (Visual Studio, VS Code with C# Dev Kit, Rider).

### Build, test, run

```powershell
git clone https://github.com/YOUR-USERNAME/TidyPackRat.git
cd TidyPackRat

dotnet build TidyFlow.slnx
dotnet test --solution TidyFlow.slnx
dotnet run --project src/TidyFlow

.\build.ps1                              # build + test in one go
.\build.ps1 -Portable                    # also publish dist\portable\win-x64\TidyFlow.exe and a release zip
.\build.ps1 -Portable -Runtime win-arm64 # the same for ARM64
```

`dotnet test` runs both test projects. `dotnet build` skips the packaging project; see [docs/msix-packaging.md](docs/msix-packaging.md) if you want to build the optional MSIX package.

### Test safely

Point TidyFlow at a scratch data folder so development never touches your real settings, history or schedule, and use a test source folder:

```powershell
$env:TIDYFLOW_DATA_DIR = "$env:TEMP\TidyFlowDev"
dotnet run --project src/TidyFlow
```

Then set **Folder to organize** to a test folder full of sample files. Headless runs can be tried with `dotnet run --project src/TidyFlow -- --run` (save settings in the app once first). Note that the Schedule tab creates a real Task Scheduler task named `TidyFlow-AutoOrganize`; turn the schedule off again when you're done, or run `dotnet run --project src/TidyFlow -- --uninstall` to remove the task, the start-at-sign-in entry and the notification registration in one go.

## Project structure

```
TidyPackRat/
├── src/
│   ├── TidyFlow.Core/        # net10.0 class library: no UI
│   │   ├── Models/           # config, preferences, history, statistics
│   │   ├── Storage/          # config/preferences/activity stores (locking, atomic writes, upgrades)
│   │   ├── Organizing/       # Organizer engine, OrganizeRunner
│   │   ├── Logging/          # RunLogger
│   │   └── default-config.json
│   ├── TidyFlow/             # WPF app (net10.0-windows10.0.19041.0), MVVM
│   │   ├── Views/            # MainWindow, PreviewWindow
│   │   ├── ViewModels/
│   │   └── Services/         # TaskSchedulerService, StartupService, NotificationService,
│   │                         # FileWatcherService, HeadlessRunner, TrayIcon, SingleInstance, ...
│   └── TidyFlow.Package/     # optional MSIX packaging project (.wapproj, Package.appxmanifest, Images)
├── tests/                    # xUnit v3 on Microsoft Testing Platform
│   ├── TidyFlow.Core.Tests/  # engine, settings upgrades, history and undo
│   └── TidyFlow.Tests/       # net10.0-windows: Task Scheduler task XML, command-line parsing
├── tools/                    # New-AppIcon.ps1, Generate-MsixAssets.ps1
├── docs/
├── assets/                   # logos and screenshots
├── .github/                  # CI and release workflows, issue and PR templates
├── Directory.Build.props     # shared settings and the version number
├── Directory.Packages.props  # central NuGet package versions
├── global.json
└── TidyFlow.slnx
```

## Coding conventions

- **Nullable reference types** are on. Don't suppress warnings without a good reason.
- **Warnings are errors**, with the `latest-recommended` analyzers. Fix them rather than disabling them.
- **File-scoped namespaces**, and **XML doc comments** on public members.
- **Engine logic goes in `TidyFlow.Core`** with unit tests in `tests/TidyFlow.Core.Tests`. The app, scheduled runs and the watcher all use the same engine, so a rule change belongs there, once. Testable app logic (scheduling, command line) is covered in `tests/TidyFlow.Tests`.
- **UI logic goes in view models** (CommunityToolkit.Mvvm), not in code-behind.
- Package versions are managed centrally in `Directory.Packages.props`; reference packages without a `Version` in project files.
- UI text should be plain and friendly ("Files changed in the last (hours)", not "Age threshold").

Example:

```csharp
namespace TidyFlow.Core.Organizing;

/// <summary>Decides what happens to each file in the source folder.</summary>
public sealed class Example
{
    /// <summary>Returns true when <paramref name="fileName"/> matches an exclude pattern.</summary>
    public bool IsExcluded(string fileName) => false;
}
```

## Commit messages

We follow [Conventional Commits](https://www.conventionalcommits.org/):

```
feat(schedule): let monthly runs use the last day of the month

fix(watcher): wait until downloads are unlocked before moving
```

Types: `feat`, `fix`, `docs`, `style`, `refactor`, `test`, `chore`. Useful scopes: `core`, `ui`, `schedule`, `watcher`, `package`, `docs`.

## Releasing (maintainers)

TidyFlow is released on [GitHub Releases](https://github.com/ProfessorMoose74/TidyPackRat/releases) only. Pushing a version tag does the rest.

1. In `CHANGELOG.md`, rename `## [Unreleased]` to `## [X.Y.Z] - YYYY-MM-DD`, add a fresh empty `## [Unreleased]` above it, and add the compare link at the bottom.
2. Bump `<Version>` in `Directory.Build.props` to `X.Y.Z`.
3. Bump `Identity Version` in `src/TidyFlow.Package/Package.appxmanifest` to `X.Y.Z.0`, so the optional MSIX package stays in step.
4. Commit and push to `main`, and wait for CI to pass.
5. Tag and push the tag:

   ```powershell
   git tag vX.Y.Z
   git push origin vX.Y.Z
   ```

The tag must match `<Version>`. The release workflow (`.github/workflows/release.yml`) then builds and tests, publishes `TidyFlow-X.Y.Z-x64-portable.zip` and `TidyFlow-X.Y.Z-arm64-portable.zip`, and creates the GitHub release with that version's CHANGELOG section as the release notes.

CI (`.github/workflows/ci.yml`) builds, tests and uploads the portable zips as artifacts on every push to `main` and every pull request. The MSIX package isn't built by CI.

## Areas where help is welcome

- Tests for edge cases in the engine (unusual file names, long paths, locked files)
- Accessibility (Narrator, keyboard navigation, high contrast)
- Localization
- Additional file types for the default categories

## Getting help

Stuck or unsure where to start? [Open an issue](https://github.com/ProfessorMoose74/TidyPackRat/issues) and ask.

## License

By contributing, you agree that your contributions are licensed under the MIT License.
