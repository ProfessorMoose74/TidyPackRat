# TidyFlow Quick Start

Get tidy in five minutes.

## 1. Install

TidyFlow needs Windows 10 version 2004 or later, or Windows 11. Nothing else is needed.

1. Go to the [Releases page](https://github.com/ProfessorMoose74/TidyPackRat/releases) and download `TidyFlow-<version>-x64-portable.zip` (or `-arm64-portable.zip` for Windows on ARM).
2. Unzip it to a folder you'll keep, for example `%LOCALAPPDATA%\Programs\TidyFlow`.
3. Run `TidyFlow.exe`. If Windows shows **Windows protected your PC**, select **More info** > **Run anyway** (TidyFlow isn't code-signed).

Optional: right-click `TidyFlow.exe` to pin it to Start or the taskbar. Details in the [Installation Guide](docs/installation-guide.md).

## 2. Check your rules

Open TidyFlow and go to the **Rules** tab.

1. **Folder to organize** defaults to your Downloads folder. Select **Browse…** to change it.
2. Review the **Categories**. Each has a name, extensions and a destination. Turn off any you don't want.
3. Under **What to leave alone**, the defaults skip files changed in the last 24 hours, temporary and partial downloads, and hidden/system files.
4. Select **Save** (or press Ctrl+S).

## 3. Preview, then organize

1. On the **Dashboard**, select **Preview changes**.
2. The list shows every file and what will happen: where it moves, or why it stays.
3. Select **Move N files** to go ahead, or close the window to change nothing.

Next time you can just select **Organize now** (F5).

## 4. Undo if needed

Open **History** and select **Undo** on any run. Files go back to where they were. The Dashboard also has **Undo last run**.

## 5. Keep it tidy automatically (optional)

On the **Schedule** tab:

- **On a schedule:** Every day, Every week (pick the day) or Every month (pick the date or Last day), at a 24-hour time such as `02:00`. Select **Save**.
- **A minute after I sign in to Windows:** runs once each time you sign in. Select **Save**.
- **Watch for new files:** organizes new files about 15 seconds after they finish downloading. Takes effect immediately, while TidyFlow is running.

To keep the watcher going, turn on **Start TidyFlow when I sign in to Windows** and **Keep running in the notification area when I close the window** on the **Settings** tab.

Scheduled runs don't need TidyFlow to be open.

---

## Common setups

| Goal | How |
|---|---|
| Tidy the Desktop instead | Rules > Folder to organize > Browse… > Desktop > Save |
| Add your own file type | Rules > **Add category**, enter a name, extensions (for example `.stl, .obj`) and a destination > Save |
| Only move old files | Set **Files changed in the last (hours)** to `168` (one week) |
| Never overwrite or rename | Set **If the name is already taken** to **Leave it where it is** |
| Copy your setup to another PC | Settings > **Export settings…**, then **Import settings…** (or drag the `.tfconfig` file onto `TidyFlow.exe`) on the other PC, then Save |
| Remove TidyFlow | Exit it, run `TidyFlow.exe --uninstall`, then delete its folder (see [Uninstalling](docs/installation-guide.md#uninstalling)) |

## Something not working?

- **A file didn't move:** Preview changes tells you why (too new, too small, excluded, no matching category…).
- **See what happened:** Settings > **Open log folder**.
- More help: [Troubleshooting Guide](docs/troubleshooting.md) or [open an issue](https://github.com/ProfessorMoose74/TidyPackRat/issues).

## For developers

```powershell
git clone https://github.com/ProfessorMoose74/TidyPackRat.git
cd TidyPackRat
.\build.ps1                          # needs the .NET 10 SDK
dotnet run --project src/TidyFlow
.\build.ps1 -Portable                # your own TidyFlow.exe in dist\portable\win-x64
```

See [CONTRIBUTING.md](CONTRIBUTING.md).

**Happy organizing!**
