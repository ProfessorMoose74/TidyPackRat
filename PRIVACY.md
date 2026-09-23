# TidyFlow Privacy Statement

**Last updated:** 2026-09-23

TidyFlow is an open-source file organizer for Windows, made by Elemental Genius LLC and distributed through [GitHub Releases](https://github.com/ProfessorMoose74/TidyPackRat/releases). This statement explains what TidyFlow does with your data. Because the source code is public, you can check every claim here yourself.

## The short version

**TidyFlow does not collect, send or share any data. Everything stays on your PC.**

- **No network access:** TidyFlow works entirely offline and never connects to the internet.
- **No telemetry:** no usage statistics, crash reports or analytics.
- **No cloud sync:** your settings, history and logs stay on your computer.
- **No account:** TidyFlow never asks for your name, email address or anything else about you.

## What TidyFlow stores on your PC

TidyFlow keeps its data in one folder:

- **Portable build (the normal download):** `%LOCALAPPDATA%\TidyFlow`, or the folder set in the `TIDYFLOW_DATA_DIR` environment variable.
- **If you built and installed the optional MSIX package:** Windows redirects that folder to the package's private storage, `%LOCALAPPDATA%\Packages\ElementalGeniusLLC.TidyFlow_<id>\LocalCache\Local\TidyFlow`.

In the app, **Settings > Open data folder** opens the right location.

| File | Contents | Purpose |
|---|---|---|
| `config.json` (+ `config.json.backup`) | Source folder, categories, rules, schedule | Remember your organization settings; the backup is used if the main file is damaged |
| `preferences.json` | Theme, startup, notification and sound options | Remember app settings |
| `history.json` | The last 50 runs: file names and where each file moved from/to | Let you undo runs |
| `statistics.json` | Counts such as files organized and space tidied | Show the Dashboard |
| `logs\TidyFlow-YYYY-MM.log` | A record of each run: files moved, skipped or failed, with paths and times | Help you see what happened and troubleshoot |

One log is kept per month, and only the last 12 by default.

Depending on the options you turn on, TidyFlow also adds these per-user Windows settings:

| Item | When |
|---|---|
| Task Scheduler task `TidyFlow-AutoOrganize` | You use **On a schedule** or **A minute after I sign in to Windows** |
| Value `TidyFlow` under `HKCU\Software\Microsoft\Windows\CurrentVersion\Run` (portable build) | You turn on **Start TidyFlow when I sign in to Windows** |
| Notification registration for your user account (portable build) | Created the first time TidyFlow runs, so Windows can show its notifications |

## What TidyFlow touches

TidyFlow only works with:

- **The source folder** you choose (default: Downloads). Only files directly in it are organized; subfolders are never touched.
- **The destination folders** you set for each category.
- **Its own data folder**, described above.

TidyFlow doesn't read the contents of your files. It looks only at file names, extensions, sizes, dates and attributes to decide where they go.

## Third parties

TidyFlow doesn't integrate with or send data to any third-party service. Notifications are shown locally by Windows.

GitHub, where you download TidyFlow and can open issues, has its own [privacy statement](https://docs.github.com/site-policy/privacy-policies/github-general-privacy-statement). Anything you post in an issue is public.

## Security

- All data stays on your PC, protected by normal Windows file permissions.
- Settings, history and log files aren't encrypted. They contain file names and folder paths, which may be personal to you, so keep that in mind before sharing logs or exported `.tfconfig` files.

## Children

TidyFlow collects no data from anyone, including children.

## Removing your data

TidyFlow holds nothing about you anywhere except on your own PC. To remove it:

**Portable build**

1. Exit TidyFlow (right-click the notification-area icon > **Exit**).
2. Run `TidyFlow.exe --uninstall` and confirm. This removes the scheduled task, the start-at-sign-in entry and the notification registration.
3. Delete `%LOCALAPPDATA%\TidyFlow` (or your `TIDYFLOW_DATA_DIR` folder) to remove settings, history, statistics and logs.
4. Delete the folder that contains `TidyFlow.exe`.

**Optional MSIX package:** uninstall TidyFlow from **Settings > Apps > Installed apps**. Windows removes its data folder with it.

At any time, **Settings > Reset statistics…** clears the Dashboard numbers (history is kept). Files TidyFlow has already organized stay in their destination folders.

## Changes to this statement

If this statement changes, the "Last updated" date above changes too, and the history is visible in the repository.

## Contact

Questions about privacy? [Open an issue on GitHub](https://github.com/ProfessorMoose74/TidyPackRat/issues).
