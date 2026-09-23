# TidyFlow Troubleshooting Guide

Most questions are answered by two tools built into TidyFlow:

- **Preview changes** shows every file in the source folder and what will happen to it, including *why* a file stays.
- **The log** records every run. Open it with **Settings > Open log folder** and open `TidyFlow-YYYY-MM.log`.

## Contents

- [Downloading and running TidyFlow](#downloading-and-running-tidyflow)
- [Reading the log](#reading-the-log)
- [Files aren't moved](#files-arent-moved)
- [Files went to the wrong place](#files-went-to-the-wrong-place)
- [Undo](#undo)
- [Scheduled runs](#scheduled-runs)
- [Watch for new files](#watch-for-new-files)
- [Startup and the notification area](#startup-and-the-notification-area)
- [Notifications](#notifications)
- [Settings and saving](#settings-and-saving)
- [Upgrading from 1.x](#upgrading-from-1x)
- [Command-line exit codes](#command-line-exit-codes)
- [Removing TidyFlow completely](#removing-tidyflow-completely)
- [Getting help](#getting-help)

## Downloading and running TidyFlow

**"Windows protected your PC"**

TidyFlow isn't code-signed, so Windows SmartScreen may warn you the first time you run a newly downloaded `TidyFlow.exe`. Select **More info**, then **Run anyway**. Only do this for a copy you downloaded from the official [Releases page](https://github.com/ProfessorMoose74/TidyPackRat/releases).

If there's no **Run anyway** button, your organization may block unsigned apps; ask your IT department. You can also right-click the zip before unzipping, choose **Properties**, tick **Unblock** and select **OK**.

**Antivirus blocked or quarantined TidyFlow.exe**

Some antivirus tools are wary of unsigned, single-file apps like `TidyFlow.exe`, which bundles .NET inside one large file. If yours blocks it:

1. Make sure your copy came from the official [Releases page](https://github.com/ProfessorMoose74/TidyPackRat/releases). If you're unsure, download it again from there.
2. Restore the file from quarantine and add an exception for your TidyFlow folder, following your antivirus's instructions. Consider also reporting it to your antivirus vendor as a false positive.
3. Prefer not to? [Build TidyFlow yourself](../CONTRIBUTING.md#build-test-run) from the source with `.\build.ps1 -Portable`.

**I moved TidyFlow.exe and the schedule stopped**

The scheduled task still points at the old location. Open `TidyFlow.exe` once from its new folder: it repairs the task when it starts. If you use **Start TidyFlow when I sign in to Windows**, turn it off and on again in Settings too.

**Which zip do I need?**

`x64` for almost all Intel and AMD PCs, `arm64` for Windows on ARM. **Settings > System > About > System type** tells you which you have. The x64 build also runs on ARM PCs through emulation, but the ARM64 build is faster.

## Reading the log

Each run writes lines like these, followed by a summary:

```
MOVED [Documents] report.pdf -> C:\Users\you\Documents\report.pdf
SKIPPED setup.exe (no category for this file type)
FAILED video.mp4: The process cannot access the file because it is being used by another process.
```

| Line | Meaning |
|---|---|
| `MOVED` | The file was moved (and can be undone from History). |
| `SKIPPED` | The file stayed; the reason is in brackets. |
| `FAILED` | TidyFlow tried to move it but Windows refused. The message says why (file in use, access denied, and so on). The next run tries again. |
| `Run skipped:` / `Scheduled run skipped:` | A background run didn't start because the settings were never saved, couldn't be read, or aren't valid. |

If the log folder is empty, check that logging hasn't been turned off in `config.json` (`logging.enabled`). See the [Configuration Guide](configuration-guide.md#configjson-reference).

## Files aren't moved

Run **Preview changes** and find the file. The reason tells you what to change:

| Reason | Fix |
|---|---|
| modified too recently | The file changed within **Files changed in the last (hours)** (default 24). Wait, or lower the number (`0` = no limit). |
| smaller than the minimum size | Lower **Files smaller than (KB)** (`0` = no limit). |
| matches an exclude pattern | Remove or change the pattern under **Files matching these patterns**. |
| hidden or system file | Turn off **Leave hidden and system files alone**, or unhide the file. |
| no category for this file type | Add the extension to a category, or turn on the category that has it (Installers and Code are off by default). |
| already exists in *category* | A file with that name is already at the destination and **If the name is already taken** is set to **Leave it where it is**. Switch to **Move it and add a number**. |

The file isn't in the preview at all?

- It's in a **subfolder**. Only files directly in the source folder are organized.
- You're looking at a different folder. Check **Folder to organize** on the Rules tab, or select **Open source folder** on the Dashboard.

## Files went to the wrong place

- **Two categories list the same extension:** the enabled category higher in the list wins. Remove the extension from one of them.
- **Check the destination:** paths with variables such as `%USERPROFILE%\Documents` expand to your own folders. OneDrive may redirect Documents, Pictures and Desktop into your OneDrive folder.
- **Put them back:** open **History** and select **Undo** on the run.

## Undo

Undo puts every file from that run back into the source folder.

| What you see | Why |
|---|---|
| A restored file has `_1` added | A file with the same name appeared in the source folder since. TidyFlow never overwrites it. |
| Some files are reported as not undone | You already moved, renamed or deleted them after TidyFlow moved them. |
| An old run isn't listed | History keeps the last 50 runs. |

## Scheduled runs

**Nothing happens at the scheduled time**

1. On the **Schedule** tab, check that **On a schedule** (or **A minute after I sign in to Windows**) is on and that you selected **Save**. The unsaved-changes bar at the bottom means it isn't saved yet.
2. The tab should show **Next scheduled run: …**. If it doesn't, restart TidyFlow; it recreates the task on startup if it's missing or points to an old location of `TidyFlow.exe`.
3. Check the log for a `Scheduled run skipped:` line (see below).
4. Open Task Scheduler (**Win+R**, `taskschd.msc`), find **TidyFlow-AutoOrganize** in the Task Scheduler Library, and check **Last Run Time** / **Last Run Result**. Right-click > **Run** to test it now. A result of `0x0` is success; `0x1`, `0x2` and `0x3` are the [exit codes](#command-line-exit-codes) below.

**The run happened but nothing moved**

Scheduled runs use exactly the same rules as **Organize now**, so **Preview changes** shows what they would do. The most common reason is the 24-hour **Files changed in the last (hours)** rule.

**"Run skipped: TidyFlow hasn't been set up yet"**

Background runs never use settings you haven't reviewed. Open TidyFlow, check the Rules tab, and select **Save** once.

**"Scheduled run skipped: …" with a settings message**

The saved settings aren't valid any more, for example the source folder was deleted or a category moves files into the source folder. Open TidyFlow, fix what the message says, and select **Save**.

**The PC was off or asleep**

The run happens as soon as possible after the PC is back on. No admin rights or wake timers are involved.

**It ran at a different time than expected**

The time is 24-hour: `02:00` is 2 AM, `14:00` is 2 PM. Weekly runs use the day you picked; monthly runs use the date you picked, or **Last day**.

**A window pops up during scheduled runs**

That was a 1.x problem. In 2.0 scheduled runs start TidyFlow in the background with no window. If you still see a PowerShell window, the old 1.x task is still there: open TidyFlow once and it replaces it.

## Watch for new files

| Problem | Fix |
|---|---|
| New files aren't picked up | TidyFlow must be running (its icon is in the notification area). Turn on **Start TidyFlow when I sign in to Windows** and **Keep running in the notification area when I close the window** in Settings. |
| There's a delay | Expected. Files are moved about 15 seconds after they stop changing and are no longer in use, so half-finished downloads aren't moved. |
| A large download wasn't moved | Files still locked after 30 minutes are given up on. The next manual or scheduled run picks them up. |
| A file matched no rule | The watcher ignores **Files changed in the last (hours)**, but every other rule still applies. Check with **Preview changes**. |
| "Can't watch … because it doesn't exist" or "File watching stopped: …" | The source folder was deleted, renamed or became unreachable (for example a disconnected drive). Pick the folder again on the Rules tab, then turn **Watch for new files** back on (Schedule tab, or **Watch folder for new files** in the notification area menu). |

## Startup and the notification area

**TidyFlow doesn't start when I sign in**

- Check **Start TidyFlow when I sign in to Windows** in Settings.
- If you moved `TidyFlow.exe`, turn that setting off and on again so it points at the new location.
- Check **Windows Settings > Apps > Startup** (or Task Manager > **Startup apps**) and make sure **TidyFlow** isn't turned off there.
- If you built and installed the optional MSIX package, TidyFlow shows *"Windows has TidyFlow's startup entry turned off. Turn it on in Settings > Apps > Startup."* when Windows has disabled its startup task. Open **Windows Settings > Apps > Startup** and turn **TidyFlow** on.
- If your organization manages the PC, a policy may block it.

**I closed the window but TidyFlow is still running**

That's **Keep running in the notification area when I close the window**. Select the TidyFlow icon in the notification area to open it, or right-click and choose **Exit**. The icon may be hidden under the **^** arrow on the taskbar.

**Launching TidyFlow again doesn't open a second window**

Only one TidyFlow runs at a time. Launching it again brings the existing window to the front.

## Notifications

- Check **Notify me when files are organized in the background** in Settings. Notifications are only shown for background runs (schedule and watcher), and only when something moved or failed.
- Check **Windows Settings > System > Notifications** and make sure TidyFlow is allowed and Do not disturb / Focus is off. TidyFlow registers itself for notifications the first time it runs, so it appears in that list after you've opened it once.
- If you ran `TidyFlow.exe --uninstall`, the notification registration was removed. Open TidyFlow again to re-register.
- **Play a sound** controls the notification sound.

## Settings and saving

**"You have unsaved changes to your rules or schedule."**

Rules and Schedule edits need **Save** (Ctrl+S). **Discard** throws them away. Settings tab options apply immediately.

**Save is refused with a message**

See the table in [Saving and validation](configuration-guide.md#saving-and-validation). Common ones: the source folder is a drive root, Windows, Program Files or your user profile folder itself; a category moves files into the source folder; the time isn't in `HH:mm` format.

**My settings were reset / config.json was damaged**

If `config.json` can't be read, TidyFlow automatically uses `config.json.backup` (the previous saved version). If neither can be read, TidyFlow says *"TidyFlow settings couldn't be read"* and starts with its default settings without changing your file; it's only replaced (with a backup) when you select **Save**. Until then, scheduled runs are skipped. Unknown values in an otherwise valid file fall back to their defaults.

**History disappeared**

If `history.json` was damaged, TidyFlow sets it aside as `history.json.corrupt` in the data folder and starts a new history.

**Where is the data folder?**

Use **Settings > Open data folder**. For the portable build it's `%LOCALAPPDATA%\TidyFlow` (or your `TIDYFLOW_DATA_DIR`). If you built and installed the optional MSIX package, it's in private package storage instead. See [Data folder](configuration-guide.md#data-folder).

**Start over with default settings**

Exit TidyFlow (notification area > **Exit**), open the data folder, and delete or rename `config.json` and `config.json.backup`. To reset only the categories, use **Restore defaults** on the Rules tab.

## Upgrading from 1.x

| Question | Answer |
|---|---|
| Were my settings kept? | Yes. They're upgraded automatically. See [Upgrading from 1.x](configuration-guide.md#upgrading-from-1x). |
| My minimum file size behaves differently | 1.x's scheduled runs treated the number as bytes while the app said KB. 2.0 always uses KB, as the screen says. |
| My old logs are gone | 1.x logged to `C:\ProgramData\TidyFlow\logs`. 2.0 logs to its own data folder. The old folder can be deleted. |
| Do I still need to change the PowerShell execution policy? | No. 2.0 doesn't use PowerShell at all. If you changed the policy for 1.x, you can set it back. |
| The old scheduled task is still there | Open TidyFlow once. It replaces a task that still points to the 1.x PowerShell worker. |
| I had 1.x from an MSI or MSIX installer | Uninstall it from **Settings > Apps > Installed apps**. See [Upgrading from TidyFlow 1.x](installation-guide.md#upgrading-from-tidyflow-1x); export your settings first if 1.x was an MSIX package. |

## Command-line exit codes

`TidyFlow.exe --run` (what the scheduled task runs) exits with:

| Code | Meaning | What to do |
|---|---|---|
| `0` | Success (including "nothing to organize") | |
| `1` | Some files failed to move | Look for `FAILED` lines in the log. Usually a file was in use. |
| `2` | Settings invalid, unreadable, or never saved | Open TidyFlow, fix what the log says, and select **Save**. |
| `3` | The run failed, for example the source folder is missing | Check the source folder exists and is reachable, then see the log. |

## Removing TidyFlow completely

The portable build has no entry in **Settings > Apps**. To remove it:

1. Exit TidyFlow (right-click the notification-area icon > **Exit**).
2. Run `TidyFlow.exe --uninstall` (for example from **Win+R**: `"C:\path\to\TidyFlow.exe" --uninstall`) and confirm. This removes the scheduled task `TidyFlow-AutoOrganize`, the start-at-sign-in entry and the notification registration.
3. Delete `%LOCALAPPDATA%\TidyFlow` (or your `TIDYFLOW_DATA_DIR` folder) to remove settings, history and logs.
4. Delete the folder that contains `TidyFlow.exe`.

Already deleted `TidyFlow.exe`? Remove **TidyFlow-AutoOrganize** in Task Scheduler (`taskschd.msc`) and turn TidyFlow off in **Windows Settings > Apps > Startup**, or download TidyFlow again just to run `--uninstall`.

If you built and installed the optional MSIX package, uninstall it from **Settings > Apps > Installed apps** instead (this removes its data too), and delete **TidyFlow-AutoOrganize** in Task Scheduler if you had a schedule.

## Getting help

If you're still stuck, [open an issue](https://github.com/ProfessorMoose74/TidyPackRat/issues/new/choose) with:

- Your Windows version (`winver`) and TidyFlow version (**Settings > About**, including Portable or MSIX package)
- What you expected and what happened
- The relevant lines from the log (remove any file names you'd rather not share)
- For scheduling problems: the **Last Run Result** of **TidyFlow-AutoOrganize** in Task Scheduler

Questions and ideas are welcome as issues too. Found a security problem? Please report it privately as described in [SECURITY.md](../SECURITY.md).
