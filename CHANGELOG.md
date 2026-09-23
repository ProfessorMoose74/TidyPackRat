# Changelog

All notable changes to TidyFlow are documented here.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/), and this project uses [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

### Added

- WinGet package (`winget install ElementalGenius.TidyFlow`), with new releases submitted automatically by the release workflow.

## [2.0.0] - 2026-09-23

A complete rewrite on .NET 10. Your 1.x settings are upgraded automatically. Download it from [GitHub Releases](https://github.com/ProfessorMoose74/TidyPackRat/releases).

### Added

- **Preview changes:** a window that lists every file in the source folder with what will happen (move to a category, or stay with the reason). Nothing moves until you select **Move N files**.
- **History tab** listing every run (Manual run, Scheduled run, Watched folder) with **Undo** per run; the last 50 runs are kept. Scheduled and watched-folder runs can now be undone too.
- Weekly schedules on the day you choose, and monthly schedules on the 1st–28th or the **Last day** of the month.
- **A minute after I sign in to Windows** option that runs independently of the schedule.
- **Next scheduled run** shown on the Schedule tab and Dashboard, read from Task Scheduler.
- **Leave hidden and system files alone (desktop.ini, thumbs.db)** option, on by default.
- `*.partial` and `*.download` default exclude patterns.
- New default file types: camera RAW (.dng, .cr2, .cr3, .nef, .arw, .raw), .pages, .xps/.oxps, .tsv, .numbers, .tgz, .zst, and more code file types (.c, .h, .kt, .swift, .toml, .sql, .sh).
- **Installers** category (.exe, .msi, .msix, .appx, …; off by default).
- Windows 11 Fluent look that follows the Windows light/dark setting, or choose Light or Dark.
- ARM64 support.
- **Portable single-file downloads** for x64 and ARM64 (`TidyFlow-<version>-<arch>-portable.zip`): one self-contained `TidyFlow.exe` that includes .NET, with nothing to install. A release workflow builds, tests and publishes them to GitHub Releases when a version tag is pushed. Build one yourself with `.\build.ps1 -Portable`.
- An optional, self-contained MSIX packaging project for people who want to build and sideload an installed package (`.\build.ps1 -Package`).
- **Start TidyFlow when I sign in to Windows** (the portable build uses your per-user Run registry key; the optional MSIX package uses its startup task, shown in Windows Settings > Apps > Startup).
- Command line: `--run` (organize once without a window, with exit codes 0–3), `--minimized`, and a `.tfconfig` path to import (drag a file onto `TidyFlow.exe`; the MSIX package also opens `.tfconfig` files on double-click).
- `--uninstall` command that removes the scheduled task, the start-at-sign-in entry and the notification registration (after asking), and tells you where your settings are kept.
- Notification area menu item **Watch folder for new files**.
- Unsaved-changes bar with **Save** (Ctrl+S) and **Discard**; **Organize now** (F5) and **Preview changes** save pending edits first.
- Clear validation messages (for example, TidyFlow won't organize a drive root, Windows, Program Files or your user profile folder itself).
- **Open data folder**, **Open log folder** and **Reset statistics…** in Settings; About shows whether this is the portable build or the MSIX package.
- Scheduled runs show a Windows notification when something moved (if notifications are on). The portable build registers itself for notifications the first time it runs.
- Only one TidyFlow window runs at a time; launching it again brings it to the front.
- Unit tests for the engine, settings upgrades, undo, Task Scheduler task and command line, and GitHub Actions CI that builds, tests and uploads portable builds for every push to `main` and every pull request.
- Code of Conduct (Contributor Covenant 2.1), security policy, and issue and pull request templates.

### Changed

- One organizing engine (`TidyFlow.Core`) is shared by Organize now, scheduled runs and the file watcher, so all follow identical rules.
- Distribution is now GitHub Releases only; TidyFlow isn't listed in the Microsoft Store. The MSIX packaging project remains as an optional build.
- Scheduled runs call `TidyFlow.exe --run` directly in the background. The optional MSIX package uses the app execution alias `%LOCALAPPDATA%\Microsoft\WindowsApps\tidyflow.exe`, which stays the same across updates. Missed runs (PC off or asleep) happen as soon as possible.
- On startup TidyFlow repairs the scheduled task if it's missing, still points to the 1.x PowerShell worker, or points to an old location of `TidyFlow.exe`.
- `--run` refuses to organize until settings have been saved at least once, so background runs never use unreviewed defaults.
- Watch for new files now waits until a file has stopped changing for about 15 seconds and is no longer locked, and gives up on files still locked after 30 minutes.
- Settings format is now schema version 2: `fileSizeThreshold` became `minFileSizeKB` (KB), `darkMode` became `theme`, and schedules gained `dayOfWeek` and `dayOfMonth`. Unknown values fall back to defaults instead of failing.
- Logs go to the `logs` folder inside TidyFlow's data folder by default (`logging.logPath` is empty). The 1.x ProgramData log path is migrated.
- Logs are monthly (`TidyFlow-YYYY-MM.log`, last 12 kept) with a `MOVED`, `SKIPPED` or `FAILED` line per file and a summary per run.
- "Executables" category replaced by **Installers**; scripts (.ps1, .bat, .cmd, .sh) moved to the **Code** category.
- Duplicate handling options renamed to **Move it and add a number (report_1.pdf)** and **Leave it where it is**.
- Undo never overwrites: if a same-named file has appeared in the source folder, the restored file gets a `_1` suffix. Files you already moved or deleted are reported.
- Statistics and history writes use a cross-process lock and atomic writes; a damaged `history.json` is set aside as `history.json.corrupt`, and a damaged `config.json` falls back to `config.json.backup`.
- Requires Windows 10 version 2004 (build 19041) or later.
- TidyFlow is published by Elemental Genius LLC. Repository links point to https://github.com/ProfessorMoose74/TidyPackRat.
- Solution is now `TidyFlow.slnx`; version and shared build settings live in `Directory.Build.props`, package versions in `Directory.Packages.props`.

### Fixed

- The PowerShell worker crashes on recent Windows 11 builds (1.2.3–1.2.7) are gone for good: the PowerShell worker was removed and everything now runs inside the app.
- Minimum file size was measured in KB by the watcher but in bytes by scheduled runs.
- Scheduled and manual runs weren't recorded in history, so they couldn't be undone and didn't count in statistics.
- Exclude patterns with a `*` in the middle (for example `*invoice*`) didn't work for the watcher.
- The "Run on startup" checkbox did nothing.
- Weekly runs were always on Monday and monthly runs always on the 1st.
- Scheduled runs popped up a PowerShell window.
- Scheduled tasks broke after every MSIX package update because they used a version-specific path.
- Undo failed when a file with the same name had appeared in the source folder since.
- Undo for watched-folder moves only undid a single file.
- The app and the watcher could corrupt statistics or history when writing at the same time.
- The app's built-in defaults and the default configuration file differed (the built-in defaults lacked newer file types).
- 1.x logged to ProgramData by default, which the MSIX package couldn't read back.
- "Recently organized" on the Dashboard no longer lists files from runs that were undone.

### Removed

- The PowerShell worker script (`TidyFlow-Worker.ps1`). TidyFlow no longer uses PowerShell or changes the execution policy. Leftover worker files are deleted from the data folder.
- The MSI (WiX) installer and the 1.x portable ZIP with `Launch-TidyFlow.bat` (replaced by the single-file portable download).
- .NET Framework 4.8 and Newtonsoft.Json dependencies.
- "Test Run (Dry Run)" (replaced by Preview changes) and the Alt+T / Alt+R / Alt+V / Alt+S shortcuts.
- `tools/prepare-assets.ps1`, `tools/integrate-logos.ps1` and `tools/update-project-icon.ps1` (replaced by `tools/New-AppIcon.ps1`).
- `TidyFlow.sln`, `PROJECT_SUMMARY.md` and `STORE_LISTING.md`.

## [1.2.7] - 2026-01-06

### Fixed

- PowerShell worker crash on Windows 11 Insider build 26200.x: removed `Set-StrictMode` and colored console output.

## [1.2.6] - 2026-01-02

### Fixed

- PowerShell worker crash on Windows 11 24H2 (build 26100.7171) caused by `cmd /c pause`; simplified the key-press wait.

### Changed

- `MaxVersionTested` raised to 10.0.26200.0.

## [1.2.5] - 2026-01-01

### Fixed

- PowerShell crash on Windows 11 Insider builds (26200.x) during Preview Changes.
- File watcher race condition, unhandled file move errors, and a possible endless loop when generating duplicate file names.

## 1.2.0 and earlier

- **1.2.4** (2025-12-29): Fixed PowerShell worker crash on Windows 11 24H2 (full PowerShell path, `-NoProfile`).
- **1.2.3** (2025-12-26): Moved data from ProgramData to `%LOCALAPPDATA%\TidyFlow` to work around MSIX file virtualization; removed the legacy TidyPackRat artifacts and installer.
- **1.2.1–1.2.2** (2025-12-19 to 2025-12-24): Fixed worker script deployment for the Microsoft Store package.
- MSIX packaging project added for Microsoft Store submission (2025-12-18).
- Renamed from TidyPackRat to TidyFlow, with a new logo (2025-12-11).
- **1.2.0** (2025-12-11): Notification area integration, dark mode, statistics dashboard, real-time file watching, undo, custom categories in the app, notifications, sounds, settings export/import, high DPI and accessibility improvements.
- **1.1.0** (2025-12-03): Better input validation and security; minimum file size setting in the app.
- **1.0.x** (2025-11): First release as TidyPackRat: PowerShell worker, WPF configuration app, MSI installer, Task Scheduler integration, logging; portable ZIP in 1.0.1.

[Unreleased]: https://github.com/ProfessorMoose74/TidyPackRat/compare/v2.0.0...HEAD
[2.0.0]: https://github.com/ProfessorMoose74/TidyPackRat/compare/ed2fb65...v2.0.0
[1.2.7]: https://github.com/ProfessorMoose74/TidyPackRat/commit/429e139
[1.2.6]: https://github.com/ProfessorMoose74/TidyPackRat/commit/6514152
[1.2.5]: https://github.com/ProfessorMoose74/TidyPackRat/commit/4d5c3e7
