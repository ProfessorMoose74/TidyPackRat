# TidyFlow Installation Guide

TidyFlow is distributed on [GitHub Releases](https://github.com/ProfessorMoose74/TidyPackRat/releases) as a portable app: one `TidyFlow.exe`, no installer. You can also install it with [WinGet](#install-with-winget), Windows' built-in package manager, which fetches the same file. It isn't in the Microsoft Store.

## System requirements

| | |
|---|---|
| **Windows** | Windows 10 version 2004 (build 19041) or later, or Windows 11 |
| **Processor** | x64 or ARM64 |
| **Other software** | None. `TidyFlow.exe` includes the .NET runtime it needs. No PowerShell or admin rights required. |

To check your Windows version, press **Win+R**, type `winver` and press Enter.

## Install

1. Open the [Releases page](https://github.com/ProfessorMoose74/TidyPackRat/releases) and, under the latest release, download:

   | File | For |
   |---|---|
   | `TidyFlow-<version>-x64-portable.zip` | Most PCs (Intel or AMD) |
   | `TidyFlow-<version>-arm64-portable.zip` | Windows on ARM (for example Snapdragon laptops) |

   Not sure? **Settings > System > About** shows **System type**: "x64-based processor" or "ARM-based processor".

2. Unzip it to a folder you'll keep, for example `%LOCALAPPDATA%\Programs\TidyFlow` or `C:\Tools\TidyFlow`. (Don't run it from inside the zip or from a temporary folder.) The zip contains `TidyFlow.exe` (about 80 MB) and `LICENSE`.
3. Run `TidyFlow.exe`.
4. Optional: right-click `TidyFlow.exe` and choose **Pin to Start** or **Pin to taskbar** (on Windows 11, under **Show more options**).

### Install with WinGet

```powershell
winget install ElementalGenius.TidyFlow
```

WinGet downloads the right zip for your PC, puts `TidyFlow.exe` in its own folder and adds a `tidyflow` command, so you can start it by typing `tidyflow` in the Run box or a terminal. Pin it to Start from there if you like.

> **WinGet listing pending:** TidyFlow has been submitted to the WinGet repository. Until Microsoft approves it, `winget install` reports that no package was found; use the download instead.

### "Windows protected your PC"

TidyFlow isn't code-signed, so the first time you run it Windows SmartScreen may show **Windows protected your PC**. Select **More info**, then **Run anyway**. You only need to do this once per downloaded copy.

Some antivirus tools are also cautious about unsigned single-file apps. If yours blocks or quarantines `TidyFlow.exe`, see [Troubleshooting](troubleshooting.md#downloading-and-running-tidyflow). Always get TidyFlow from the official [Releases page](https://github.com/ProfessorMoose74/TidyPackRat/releases); the source code is in the same repository if you'd rather build it yourself.

### Moving TidyFlow later

Moving `TidyFlow.exe` to another folder is fine. The next time you open it from the new location, TidyFlow repairs its scheduled task to point there. If you use **Start TidyFlow when I sign in to Windows**, turn that setting off and on again after moving.

### Where TidyFlow keeps its data

Settings, history, statistics and logs are stored in `%LOCALAPPDATA%\TidyFlow`, not next to the program, so replacing or moving `TidyFlow.exe` never loses them. To keep them somewhere else (for example on a USB stick with the program), set the `TIDYFLOW_DATA_DIR` environment variable to a folder of your choice. See [Data folder](configuration-guide.md#data-folder).

## First launch

1. TidyFlow opens on the **Dashboard**.
2. Go to **Rules** and check the **Folder to organize** (Downloads by default) and the categories.
3. Select **Save** (Ctrl+S). Scheduled and command-line runs only start working once you've saved your settings at least once.
4. Select **Preview changes** to see what would happen, then **Move N files**.

See the [Quick Start](../QUICKSTART.md) and [Configuration Guide](configuration-guide.md) for the rest.

## Upgrading to a new version

TidyFlow doesn't update itself. To upgrade:

1. Download the new zip from the [Releases page](https://github.com/ProfessorMoose74/TidyPackRat/releases).
2. Exit TidyFlow (right-click the notification-area icon > **Exit**).
3. Replace `TidyFlow.exe` in your TidyFlow folder with the new one.
4. Open TidyFlow. Your settings, history and schedule carry over.

If you installed with WinGet, exit TidyFlow and run `winget upgrade ElementalGenius.TidyFlow` instead (or `winget upgrade --all`). WinGet can't replace `TidyFlow.exe` while it's running.

Tip: select **Watch** > **Custom** > **Releases** on the GitHub repository to be notified of new versions.

## Upgrading from TidyFlow 1.x

When 2.0 starts for the first time it:

- Upgrades your 1.x settings in `%LOCALAPPDATA%\TidyFlow` (categories, rules, schedule, preferences). See [Upgrading from 1.x](configuration-guide.md#upgrading-from-1x) for exactly what changes.
- Replaces the old scheduled task, which ran the PowerShell worker, with one that runs `TidyFlow.exe` directly.
- Deletes the leftover 1.x worker files (`TidyFlow-Worker.ps1`, `worker-deployment.json`) from its data folder.

How you get there depends on how 1.x was installed:

| 1.x was installed with | Do this |
|---|---|
| The portable ZIP | Delete the old 1.x folder, then install 2.0 as above. Your settings are picked up automatically. |
| The MSI installer | Uninstall 1.x from **Settings > Apps > Installed apps**, then install 2.0. Settings in `%LOCALAPPDATA%\TidyFlow` survive the MSI uninstall and are picked up automatically. |
| An MSIX package | In 1.x, **Export** your settings to a `.tfconfig` file first: uninstalling an MSIX package removes its private data. Then uninstall 1.x from **Settings > Apps > Installed apps**, install 2.0, select **Settings > Import settings…**, review, and **Save**. |

Optional cleanup: 1.x may have left logs and settings in `C:\ProgramData\TidyFlow`. 2.0 doesn't use that folder, so you can delete it once your settings are in 2.0.

## Checking it works

| What | How |
|---|---|
| The app runs | **Settings > About** shows the version and "Portable". |
| Organizing works | Put a test file (for example an old `.pdf`) in the source folder, then **Preview changes**. Note that files changed in the last 24 hours stay by default. |
| The schedule is set | Schedule tab shows **Next scheduled run: …**. You can also open Task Scheduler (**Win+R**, `taskschd.msc`) and look for **TidyFlow-AutoOrganize** in the Task Scheduler Library. Right-click > **Run** to test it. |
| Runs are logged | **Settings > Open log folder** and open `TidyFlow-YYYY-MM.log`. |

## Uninstalling

1. Exit TidyFlow: right-click the TidyFlow icon in the notification area and choose **Exit**.
2. Run `TidyFlow.exe --uninstall`. For example, press **Win+R** and enter the full path in quotes followed by `--uninstall`:

   ```
   "%LOCALAPPDATA%\Programs\TidyFlow\TidyFlow.exe" --uninstall
   ```

   Confirm when asked. TidyFlow removes its scheduled task (`TidyFlow-AutoOrganize`), the start-at-sign-in entry and its notification registration, then tells you where your settings are.
3. Optional: delete `%LOCALAPPDATA%\TidyFlow` (or your `TIDYFLOW_DATA_DIR` folder) to remove settings, history, statistics and logs. `--uninstall` keeps them in case you come back.
4. Delete the folder that contains `TidyFlow.exe`.

If you installed with WinGet, do steps 1–3 using `tidyflow --uninstall`, then run `winget uninstall ElementalGenius.TidyFlow` instead of deleting the folder. (WinGet removes the program but doesn't know about the scheduled task or startup entry, which is why `--uninstall` comes first.)

Files TidyFlow organized stay in their destination folders.

## Optional: the MSIX package

The repository also contains an optional MSIX packaging project for people who want an installed app (Start menu entry, `tidyflow.exe` command alias, Windows startup task). It isn't published anywhere: you build it and sign it yourself. See [Optional: building an MSIX package](msix-packaging.md).

If you built and installed the optional MSIX package:

- Its data is in `%LOCALAPPDATA%\Packages\ElementalGeniusLLC.TidyFlow_<id>\LocalCache\Local\TidyFlow` (use **Settings > Open data folder**), and **Settings > About** shows "MSIX package".
- Uninstall it from **Settings > Apps > Installed apps**. Windows removes its data folder too. To remove the scheduled task, first turn off **On a schedule** and **A minute after I sign in to Windows** and select **Save**, or delete **TidyFlow-AutoOrganize** in Task Scheduler afterwards.
- Don't use it and the portable build at the same time: they share the scheduled task name (`TidyFlow-AutoOrganize`).

---

**Next:** [Configuration Guide](configuration-guide.md) · [Troubleshooting](troubleshooting.md) · [Open an issue](https://github.com/ProfessorMoose74/TidyPackRat/issues)
