# TidyFlow Configuration Guide

Everything you can change in TidyFlow 2.0, where it lives, and what it does.

## Contents

- [How organizing works](#how-organizing-works)
- [Rules tab](#rules-tab)
- [Default categories](#default-categories)
- [Schedule tab](#schedule-tab)
- [Settings tab](#settings-tab)
- [Saving and validation](#saving-and-validation)
- [Data folder](#data-folder)
- [config.json reference](#configjson-reference)
- [Upgrading from 1.x](#upgrading-from-1x)
- [Tips](#tips)

## How organizing works

**Organize now**, **Preview changes**, scheduled runs and the file watcher all use the same engine, so they follow exactly the same rules. For each file **directly in the source folder** (subfolders are never touched), TidyFlow checks, in this order:

| Check | If it matches, the file stays because… |
|---|---|
| Hidden or system file (when that option is on) | hidden or system file |
| Name matches an exclude pattern | matches an exclude pattern |
| Changed in the last N hours (not for the watcher) | modified too recently |
| Smaller than N KB | smaller than the minimum size |
| No enabled category lists its extension | no category for this file type |
| Name already taken at the destination and set to "Leave it where it is" | already exists in *category* |

Otherwise the file moves to its category's destination folder (created if needed). **Preview changes** shows these reasons for every file, and the log records a `MOVED`, `SKIPPED` or `FAILED` line for each.

## Rules tab

Changes on this tab need **Save** (see [Saving and validation](#saving-and-validation)).

### Folder to organize

The source folder. Default: `%USERPROFILE%\Downloads`. Select **Browse…** or type a path; variables such as `%USERPROFILE%` work.

TidyFlow refuses to organize a drive root (like `C:\`), the Windows folder, Program Files, or your user profile folder itself (`C:\Users\you`). Folders inside your profile, like Desktop or Documents, are fine.

### Categories

Each category has a **Name**, **Extensions**, a **Destination** folder and an on/off switch.

- **Extensions:** type them separated by commas, semicolons or spaces, with or without the dot (`jpg, .png; heic`). Case doesn't matter.
- **Destination:** select **Browse…** or type a path. Variables like `%USERPROFILE%` work. A category can't move files into the source folder itself.
- **Order matters:** if two enabled categories list the same extension, the one higher in the list wins.
- **Add category** adds a new row; the remove button deletes one. **Restore defaults** replaces your categories with the built-in ones (other settings stay as they are).

### What to leave alone

| Setting | Default | Notes |
|---|---|---|
| Files changed in the last (hours) | `24` | Based on the file's last-modified time. `0` = no limit. Doesn't apply to watched-folder moves. |
| Files smaller than (KB) | `0` | `0` = no limit. `1024` = skip files under 1 MB. |
| Files matching these patterns | `*.tmp`, `~*`, `*.crdownload`, `*.part`, `*.partial`, `*.download` | One per line. `*` matches anything, `?` one character. Matched against the file name, ignoring case. `*invoice*` works too. |
| Leave hidden and system files alone (desktop.ini, thumbs.db) | On | |

### If the name is already taken

| Choice | What happens |
|---|---|
| **Move it and add a number (report_1.pdf)** (default) | The file moves with `_1`, `_2`, … added before the extension. |
| **Leave it where it is** | The file stays in the source folder. |

Nothing is ever overwritten.

## Default categories

| Category | Extensions | Moves to | On |
|---|---|---|---|
| Images | .jpg .jpeg .png .gif .bmp .svg .webp .ico .tiff .tif .heic .heif .avif .jxl .dng .cr2 .cr3 .nef .arw .raw | `%USERPROFILE%\Pictures` | Yes |
| Documents | .pdf .docx .doc .txt .rtf .odt .tex .wpd .md .epub .pages .xps .oxps | `%USERPROFILE%\Documents` | Yes |
| Spreadsheets | .xlsx .xls .xlsm .csv .tsv .ods .numbers | `%USERPROFILE%\Documents\Spreadsheets` | Yes |
| Presentations | .pptx .ppt .odp .key | `%USERPROFILE%\Documents\Presentations` | Yes |
| Archives | .zip .rar .7z .tar .gz .tgz .bz2 .xz .zst .iso | `%USERPROFILE%\Documents\Archives` | Yes |
| Videos | .mp4 .avi .mkv .mov .wmv .flv .webm .m4v | `%USERPROFILE%\Videos` | Yes |
| Audio | .mp3 .wav .flac .m4a .ogg .aac .wma .opus .aiff | `%USERPROFILE%\Music` | Yes |
| Installers | .exe .msi .msix .msixbundle .appx .appxbundle .appinstaller | `%USERPROFILE%\Downloads\Installers` | No |
| Code | .py .js .ts .jsx .tsx .vue .html .css .cpp .c .h .cs .java .kt .php .rb .go .rs .swift .json .xml .yaml .yml .toml .sql .ps1 .bat .cmd .sh | `%USERPROFILE%\Documents\Code` | No |

Installers and Code are off by default so programs and scripts aren't moved unexpectedly.

### Custom category ideas

| Name | Extensions |
|---|---|
| E-Books | .epub .mobi .azw .azw3 (remove .epub from Documents, or put E-Books above it) |
| 3D Models | .stl .obj .fbx .blend .3mf |
| Fonts | .ttf .otf .woff .woff2 |
| CAD | .dwg .dxf .step .stp .iges |
| Torrents | .torrent |

## Schedule tab

Schedule changes need **Save**; **Watch for new files** takes effect immediately.

### Organize automatically

| Option | Details |
|---|---|
| **On a schedule** | **Every day**, **Every week** (pick the day) or **Every month** (pick the 1st–28th or **Last day**), at a 24-hour time such as `02:00` or `18:30`. |
| **A minute after I sign in to Windows** | Runs once, a minute after each sign-in. Works with or without the schedule. |

When saved, the tab shows **Next scheduled run: …**, read from Windows Task Scheduler.

How it works:

- TidyFlow creates a per-user Task Scheduler task named **TidyFlow-AutoOrganize** that runs `TidyFlow.exe --run`. No admin rights are needed.
- The portable build points the task at the `TidyFlow.exe` you're running. If you move it, open TidyFlow once from the new location and it updates the task.
- If you built and installed the optional MSIX package, the task uses its app execution alias `%LOCALAPPDATA%\Microsoft\WindowsApps\tidyflow.exe`, which stays the same across package updates.
- Runs happen in the background with no window. If your PC was off or asleep at the scheduled time, the run happens as soon as possible.
- Each scheduled run is recorded in History (so you can undo it), counts in the statistics, is written to the log and, if notifications are on, shows a Windows notification when something moved.
- Turning off both options (and saving) removes the task.
- Each time TidyFlow starts, it repairs the task if it's missing, points to an old location of `TidyFlow.exe`, or still points to the old 1.x PowerShell worker.

### Watch for new files

**Organize new files as soon as they arrive.** New files are moved about 15 seconds after they stop changing and are no longer locked (in other words, finished downloading).

- The **Files changed in the last (hours)** rule is ignored; all other rules apply.
- Works only while TidyFlow is running. To keep it running, turn on **Start TidyFlow when I sign in to Windows** and **Keep running in the notification area when I close the window** on the Settings tab.
- Files still locked after 30 minutes are given up on; the next run picks them up.
- Each batch of watched-folder moves appears in History as one **Watched folder** entry.
- You can also toggle it from the notification area menu (**Watch folder for new files**).

## Settings tab

These settings take effect immediately.

| Setting | Notes |
|---|---|
| **Theme** | Use my Windows setting / Light / Dark |
| **Start TidyFlow when I sign in to Windows (in the notification area)** | Adds a `TidyFlow` entry to your per-user Run key (`HKCU\Software\Microsoft\Windows\CurrentVersion\Run`) that starts `TidyFlow.exe --minimized`. If you move `TidyFlow.exe`, turn this off and on again. If you built and installed the optional MSIX package, it uses the package startup task instead, which you can also switch in **Windows Settings > Apps > Startup**; if it's turned off there, TidyFlow tells you. |
| **Keep running in the notification area when I close the window** | Closing the window hides TidyFlow instead of exiting. Use **Exit** from the notification area menu to quit. |
| **Start minimized to the notification area** | |
| **Notify me when files are organized in the background** | For scheduled and watched-folder runs. |
| **Play a sound** | |
| **Export settings… / Import settings…** | Saves or loads a `.tfconfig` file with your rules, schedule and app settings (logging settings stay as they are on this PC). You can also drag a `.tfconfig` file onto `TidyFlow.exe` (with the optional MSIX package, double-clicking the file works too). Exports from 1.x import fine. Imported rules and schedule still need **Save**. |
| **Open log folder / Open data folder** | Opens the right folder for your version (see [Data folder](#data-folder)). |
| **Reset statistics…** | Clears the Dashboard numbers. History is kept. |

**About** shows the version and whether you're running the portable build or the MSIX package.

### Command line

| Command | What it does |
|---|---|
| `TidyFlow.exe --run` | Organizes once with your saved settings, with no window (what the scheduled task runs). Exit codes: `0` OK, `1` some files failed, `2` settings invalid, unreadable or never saved, `3` the run failed. |
| `TidyFlow.exe --minimized` | Starts in the notification area (used by **Start TidyFlow when I sign in**). |
| `TidyFlow.exe --uninstall` | After asking, removes the scheduled task, the start-at-sign-in entry and the notification registration. Settings, history and logs are kept. See [Uninstalling](installation-guide.md#uninstalling). |
| `TidyFlow.exe path\to\settings.tfconfig` | Opens TidyFlow and offers to import the file. |

## Saving and validation

- Edits on the **Rules** and **Schedule** tabs show a bar at the bottom: *You have unsaved changes to your rules or schedule.* Select **Save** (Ctrl+S) or **Discard**.
- **Organize now** and **Preview changes** save pending edits first.
- Invalid settings are rejected with a message, for example:

| Message | Fix |
|---|---|
| Choose a source folder to organize. | Pick a folder. |
| TidyFlow can't organize *path*. Choose a regular folder such as Downloads. | Drive roots, Windows, Program Files and your profile folder itself aren't allowed. |
| Category '*name*' moves files into the source folder itself. | Pick a different destination. |
| Category '*name*' needs a destination folder. / has no file extensions. / Every category needs a name. | Fill in the missing field or remove the category. |
| '*time*' isn't a valid time. Use 24-hour HH:mm, for example 02:00. | Use `02:00`, `14:30`, and so on (not `2:00 AM`). |

## Data folder

| Version | Location |
|---|---|
| Portable build | `%LOCALAPPDATA%\TidyFlow` |
| Optional MSIX package (if you built and installed it) | `%LOCALAPPDATA%\Packages\ElementalGeniusLLC.TidyFlow_<id>\LocalCache\Local\TidyFlow` (Windows redirects `%LOCALAPPDATA%\TidyFlow` here) |

Use **Settings > Open data folder** rather than typing the path. For the portable build, delete this folder yourself if you want to remove your settings; uninstalling the MSIX package removes it automatically.

| File | Contents |
|---|---|
| `config.json` | Organization settings: rules, categories, schedule |
| `config.json.backup` | The previous version, used automatically if `config.json` is damaged |
| `preferences.json` | App settings from the Settings tab |
| `statistics.json` | Dashboard numbers |
| `history.json` | The last 50 runs, for Undo. A damaged file is set aside as `history.json.corrupt`. |
| `logs\TidyFlow-YYYY-MM.log` | One log per month (last 12 kept). Each run logs `MOVED` / `SKIPPED` / `FAILED` lines and a summary. |

The `TIDYFLOW_DATA_DIR` environment variable overrides the data folder (useful for development, or to keep settings next to the program, for example on a USB stick).

## config.json reference

You rarely need to edit `config.json` by hand; the app covers every setting. If you do, exit TidyFlow first (notification area > **Exit**), keep the JSON valid, and reopen TidyFlow. Unknown or invalid values fall back to defaults. Environment variables like `%USERPROFILE%` work in all paths.

```json
{
  "schemaVersion": 2,
  "sourceFolder": "%USERPROFILE%\\Downloads",
  "fileAgeThreshold": 24,
  "minFileSizeKB": 0,
  "duplicateHandling": "rename",
  "skipHiddenFiles": true,
  "categories": [
    {
      "name": "Images",
      "extensions": [".jpg", ".jpeg", ".png"],
      "destination": "%USERPROFILE%\\Pictures",
      "enabled": true
    }
  ],
  "schedule": {
    "enabled": true,
    "frequency": "weekly",
    "time": "18:00",
    "dayOfWeek": "friday",
    "dayOfMonth": 1,
    "runOnStartup": false
  },
  "excludePatterns": ["*.tmp", "~*", "*.crdownload", "*.part", "*.partial", "*.download"],
  "logging": {
    "enabled": true,
    "logPath": "",
    "logLevel": "info",
    "maxLogFiles": 12
  }
}
```

| Key | Values | Meaning |
|---|---|---|
| `schemaVersion` | `2` | Settings format version. |
| `sourceFolder` | path | Folder to organize. |
| `fileAgeThreshold` | hours, `0` = no limit | Files changed in the last N hours stay. |
| `minFileSizeKB` | KB, `0` = no limit | Files smaller than this stay. |
| `duplicateHandling` | `"rename"` \| `"skip"` | Add a number, or leave it where it is. |
| `skipHiddenFiles` | `true` \| `false` | Leave hidden and system files alone. |
| `categories[]` | `name`, `extensions[]`, `destination`, `enabled` | In priority order. |
| `schedule.enabled` | `true` \| `false` | "On a schedule". |
| `schedule.frequency` | `"daily"` \| `"weekly"` \| `"monthly"` | |
| `schedule.time` | `"HH:mm"` | 24-hour time. |
| `schedule.dayOfWeek` | `"monday"` … `"sunday"` | Used for weekly runs. |
| `schedule.dayOfMonth` | `1`–`28`, `0` = last day | Used for monthly runs. |
| `schedule.runOnStartup` | `true` \| `false` | "A minute after I sign in to Windows". |
| `excludePatterns[]` | wildcard patterns | Files matching these stay. |
| `logging.enabled` | `true` \| `false` | Write run logs. |
| `logging.logPath` | `""` or a folder path | Empty (the default) means the `logs` folder inside TidyFlow's data folder, which follows `TIDYFLOW_DATA_DIR` and the MSIX package's private location. Set a folder path (variables allowed) to log somewhere else. |
| `logging.logLevel` | `"info"` \| `"warn"` \| `"error"` | `info` logs every moved and skipped file; `warn` and `error` log only problems. |
| `logging.maxLogFiles` | number | Monthly log files to keep. |

`preferences.json`, `statistics.json` and `history.json` are managed by the app; don't edit them.

## Upgrading from 1.x

Your 1.x settings are upgraded automatically the first time 2.0 starts:

| 1.x | 2.0 |
|---|---|
| `fileSizeThreshold` | `minFileSizeKB`, treated as KB (what the 1.x screen said; the 1.x scheduled worker wrongly used bytes) |
| `darkMode` preference | `theme` |
| `logging.logPath` in `C:\ProgramData\TidyFlow\logs` | `""` (TidyFlow's own logs folder) |
| Unknown or invalid values | Defaults, instead of failing to load |
| Scheduled task pointing at `TidyFlow-Worker.ps1` | Recreated to run `TidyFlow.exe --run` |
| Leftover `TidyFlow-Worker.ps1`, `worker-deployment.json` | Deleted from the data folder |

Settings files exported from 1.x can be imported with **Import settings…**.

## Tips

- **Preview first.** Use **Preview changes** after any rule change.
- **Everything can be undone.** History keeps the last 50 runs, each with **Undo**.
- **Conservative setup:** Files changed in the last `168` hours (one week) + **Leave it where it is**.
- **Fast setup:** turn on **Watch for new files** so downloads are sorted as soon as they finish.
- **Protect specific files** with patterns such as `important_*` or `*_keep*`.
- **Schedule for a time your PC is usually on.** If it's off, TidyFlow catches up at the next opportunity anyway.

---

**Need more help?** See the [Troubleshooting Guide](troubleshooting.md) or [open an issue](https://github.com/ProfessorMoose74/TidyPackRat/issues).
