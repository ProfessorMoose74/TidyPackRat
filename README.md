<p align="center">
  <img src="assets/TidyFlow-logo.png" alt="TidyFlow" width="600"/>
</p>

<p align="center">
  <b>Sorting your files, to clean up your mess.</b>
</p>

<p align="center">
  <img src="assets/logo.png" alt="TidyFlow Mascot" width="150"/>
</p>

[![License: MIT](https://img.shields.io/badge/License-MIT-blue.svg)](LICENSE)
[![Version](https://img.shields.io/badge/version-1.2.0-green.svg)]()
[![Platform](https://img.shields.io/badge/platform-Windows%2010%2B-lightgrey.svg)]()

TidyFlow is an open-source automated file management system for Windows that intelligently organizes files from your Downloads folder (or any source folder) into categorized destination folders based on file type. Set it up once, and let TidyFlow keep your files tidy!

## What's New in v1.2.0

- **System Tray Integration** - Minimize to tray, run from tray menu
- **Dark Mode** - Toggle between light and dark themes
- **Statistics Dashboard** - Track files organized, space saved, and more
- **Real-time File Watching** - Auto-organize files as they appear
- **Undo/Rollback** - Reverse recent file moves with one click
- **Custom Categories** - Create your own file categories in the GUI
- **Toast Notifications** - Get notified when files are organized
- **Sound Effects** - Optional audio feedback
- **Export/Import Settings** - Share configurations between computers
- **High DPI Support** - Crisp display on all monitors
- **Accessibility** - Screen reader support and keyboard shortcuts

## Features

### Core Features
- **Automatic File Organization**: Moves files from a source folder to categorized destinations based on file extensions
- **Smart Rules**: Configure file age thresholds, size filters, and exclude patterns
- **Flexible Scheduling**: Run manually, on a schedule (daily/weekly/monthly), or on system startup
- **Safe Operation**: Test mode (dry run) to preview what will be moved before committing
- **Duplicate Handling**: Configurable strategies for handling duplicate files (rename or skip)
- **Comprehensive Logging**: Track every file movement with detailed logs

### v1.2.0 Features
- **System Tray**: Runs in the background, accessible from the system tray
- **Dark Mode**: Modern dark theme for comfortable viewing
- **Statistics Dashboard**: See how many files you've organized and space saved
- **Real-time Watching**: Automatically organize new files as they arrive
- **Undo Functionality**: Made a mistake? Undo recent file moves instantly
- **Custom Categories**: Add your own file types and destinations
- **Notifications**: Windows toast notifications when files are organized
- **Sound Effects**: Satisfying audio feedback (can be disabled)
- **Settings Export/Import**: Backup and share your configuration

## Table of Contents

- [Installation](#installation)
- [Quick Start](#quick-start)
- [Configuration](#configuration)
- [Default File Categories](#default-file-categories)
- [Usage](#usage)
- [Documentation](#documentation)
- [Building from Source](#building-from-source)
- [Contributing](#contributing)
- [Roadmap](#roadmap)
- [License](#license)

## Installation

### Prerequisites

- Windows 10 or later (Windows 11 recommended)
- PowerShell 5.1 or later (included in Windows 10)
- .NET Framework 4.8 (included in Windows 10 version 1903+)

### Install Steps

1. Download the latest `TidyFlow-Setup.msi` from the [Releases](https://github.com/ProfessorMoose74/TidyFlow/releases) page
2. Run the installer and follow the setup wizard
3. Launch "TidyFlow" from the Start Menu
4. Configure your file organization preferences
5. Click "Save Configuration" to apply settings

### Portable Version

A portable ZIP version is also available for users who prefer not to install:
1. Download `TidyFlow-Portable.zip`
2. Extract to any folder
3. Run `Launch-TidyFlow.bat`

## Quick Start

1. **Launch TidyFlow**: Find it in your Start Menu or system tray

2. **Check the Dashboard**: See your organization statistics at a glance

3. **Go to Organization Tab**: Configure your source folder and categories

4. **Set Your Source Folder**: The default is your Downloads folder

5. **Review File Categories**: Each category has:
   - A list of file extensions
   - A destination folder
   - An enable/disable toggle

6. **Configure Rules**:
   - **File Age**: Skip files newer than X hours (default: 24 hours)
   - **File Size**: Skip files smaller than X KB (default: 0 = no minimum)
   - **Duplicates**: Choose to rename or skip duplicate files
   - **Exclude Patterns**: Wildcards to skip certain files (e.g., `*.tmp`)

7. **Test First**: Click "Test Run (Dry Run)" to preview changes

8. **Enable Real-time Watching**: Toggle in the Schedule tab for automatic organization

## Configuration

### Tabbed Interface

TidyFlow v1.2.0 features a modern tabbed interface:

- **Dashboard**: Statistics, quick actions, recent activity
- **Organization**: Source folder, file categories, rules
- **Schedule**: Scheduling options, real-time file watching
- **Settings**: Theme, notifications, sounds, export/import

### Source Folder

The folder TidyFlow monitors for files to organize. Default is your Downloads folder:
```
C:\Users\YourName\Downloads
```

### File Categories

TidyFlow comes with 9 pre-configured categories. You can:
- Enable/disable any category
- Change destination folders
- **NEW**: Add custom categories via the GUI

### Organization Rules

- **File Age Threshold**: Only move files older than X hours (prevents moving files still being downloaded)
- **File Size Threshold**: Skip files smaller than X KB (useful for ignoring tiny temp files)
- **Duplicate Handling**:
  - `Rename`: Adds a number to the filename (e.g., `photo.jpg` → `photo_1.jpg`)
  - `Skip`: Leaves the file in the source folder
- **Exclude Patterns**: Use wildcards to skip files:
  - `*.tmp` - Skip temporary files
  - `~*` - Skip files starting with ~
  - `*.part` - Skip partial downloads

### Scheduling

Enable automatic scheduling to keep your folders organized:

- **Frequency**: Daily, Weekly, or Monthly
- **Time**: Specify what time to run (e.g., 2:00 AM)
- **Run on Startup**: Optionally run when you log in to Windows
- **Real-time Watching**: Organize files immediately as they appear

## Default File Categories

| Category | Extensions | Default Destination |
|----------|-----------|---------------------|
| **Images** | .jpg, .jpeg, .png, .gif, .bmp, .svg, .webp, .ico, .tiff | Pictures |
| **Documents** | .pdf, .docx, .doc, .txt, .rtf, .odt, .tex | Documents |
| **Spreadsheets** | .xlsx, .xls, .csv, .ods, .xlsm | Documents\Spreadsheets |
| **Presentations** | .pptx, .ppt, .odp, .key | Documents\Presentations |
| **Archives** | .zip, .rar, .7z, .tar, .gz, .bz2, .xz, .iso | Documents\Archives |
| **Videos** | .mp4, .avi, .mkv, .mov, .wmv, .flv, .webm | Videos |
| **Audio** | .mp3, .wav, .flac, .m4a, .ogg, .aac, .wma | Music |
| **Executables** | .exe, .msi, .bat, .cmd, .ps1 | Downloads\Executables |
| **Code** | .py, .js, .html, .css, .cpp, .cs, .java, .php, .rb, .go, .ts, .jsx, .json, .xml, .yaml | Documents\Code |

**Note**: Executables and Code categories are disabled by default for safety.

## Usage

### GUI Application

The WPF application provides an intuitive interface for:
- Viewing organization statistics
- Configuring source and destination folders
- Enabling/disabling file categories
- Creating custom categories
- Setting organization rules
- Managing schedules and real-time watching
- Running test/actual operations
- Undoing recent moves
- Viewing logs

### Keyboard Shortcuts

- `Alt+T` - Test Run (Dry Run)
- `Alt+R` - Run Now
- `Alt+V` - View Log
- `Alt+S` - Save Configuration

### System Tray

When minimized to tray, right-click the TidyFlow icon for quick access to:
- Open TidyFlow
- Run Now
- Test Run
- Exit

### PowerShell Worker Script

The worker script can also be run directly from PowerShell:

```powershell
# Standard run
.\TidyFlow-Worker.ps1 -ConfigPath "C:\ProgramData\TidyFlow\config.json"

# Dry run (test mode)
.\TidyFlow-Worker.ps1 -ConfigPath "C:\ProgramData\TidyFlow\config.json" -DryRun -VerboseLogging

# Custom config file
.\TidyFlow-Worker.ps1 -ConfigPath "C:\path\to\my-config.json"
```

### Task Scheduler Integration

When scheduling is enabled, TidyFlow creates a Windows scheduled task named `TidyFlow-AutoOrganize`. You can also manage this task directly through Windows Task Scheduler if needed.

## Documentation

Detailed documentation is available in the [`docs`](docs/) directory:

- [Installation Guide](docs/installation-guide.md) - Step-by-step installation instructions
- [Configuration Guide](docs/configuration-guide.md) - Detailed configuration options
- [Troubleshooting](docs/troubleshooting.md) - Common issues and solutions
- [MSIX Packaging](docs/msix-packaging.md) - Microsoft Store packaging guide

## Building from Source

### Prerequisites

- Visual Studio 2019 or newer
- .NET Framework 4.8 SDK
- WiX Toolset v3.11+ (for installer)
- PowerShell 5.1+

### Build Steps

1. Clone the repository:
   ```bash
   git clone https://github.com/ProfessorMoose74/tidy-pack-rat.git
   cd tidy-pack-rat
   ```

2. Build the GUI application:
   ```bash
   msbuild src\gui\TidyFlow.csproj /p:Configuration=Release
   ```

3. Prepare build directory:
   ```bash
   mkdir build\gui build\worker build\config
   copy src\gui\bin\Release\* build\gui\
   copy src\worker\*.ps1 build\worker\
   copy config\*.json build\config\
   ```

4. Build the installer (optional):
   ```bash
   msbuild src\installer\TidyFlow.Installer.wixproj /p:Configuration=Release
   ```

5. Find the installer at:
   ```
   src\installer\bin\Release\TidyFlow-Setup.msi
   ```

## Contributing

We welcome contributions! Please see [CONTRIBUTING.md](CONTRIBUTING.md) for details on:

- Code of conduct
- How to submit issues
- Pull request process
- Coding standards

## Roadmap

### Phase 1: Windows Individual Users (v1.0.0) ✅
- [x] Core file organization engine
- [x] WPF configuration GUI
- [x] MSI installer
- [x] Task Scheduler integration
- [x] Comprehensive logging

### Phase 2: Enhanced Features (v1.1.0 - v1.2.0) ✅
- [x] Custom category creation in GUI
- [x] Undo/rollback functionality
- [x] Statistics and reporting dashboard
- [x] System tray integration
- [x] Dark mode theme
- [x] Real-time file watching
- [x] Toast notifications
- [x] Export/import settings
- [x] High DPI and accessibility support

### Phase 3: Microsoft Store & Polish (v1.3.0)
- [ ] MSIX packaging for Microsoft Store
- [ ] First-run setup wizard
- [ ] File preview before moving
- [ ] Advanced filtering (regex patterns)
- [ ] Multi-folder source support

### Phase 4: Linux Version (v2.0.0)
- [ ] Bash/Python worker script
- [ ] GTK or Qt GUI
- [ ] Cron integration
- [ ] Package for major distros (deb, rpm)

### Phase 5: Enterprise Features (v3.0.0)
- [ ] Multi-user deployment
- [ ] Centralized configuration management
- [ ] Network share support
- [ ] Active Directory integration
- [ ] Group Policy templates

## Privacy

TidyFlow does not collect any data. No telemetry, no analytics, no cloud sync. Your files and settings stay on your computer. See [PRIVACY.md](PRIVACY.md) for details.

## Uninstallation

To uninstall TidyFlow:

1. Open Windows Settings → Apps → Apps & features
2. Find "TidyFlow File Organizer"
3. Click Uninstall

Or use the Start Menu shortcut: Start → TidyFlow → Uninstall TidyFlow

**Note**: Your configuration file and logs will be removed. Files that have already been organized will remain in their destination folders.

## Troubleshooting

### Files aren't being moved

- Check that the category is enabled
- Verify the file extension is in the category's list
- Check the file age threshold setting
- Review logs in `C:\ProgramData\TidyFlow\logs`

### Scheduled task isn't running

- Ensure you saved the configuration after enabling scheduling
- Verify the task exists in Task Scheduler
- Check Task Scheduler logs
- Run the configuration tool as Administrator

### Real-time watching not working

- Ensure the source folder exists
- Check that file watching is enabled in the Schedule tab
- Look for errors in the Recent Activity panel

For more help, see [Troubleshooting Guide](docs/troubleshooting.md) or [open an issue](https://github.com/ProfessorMoose74/tidy-pack-rat/issues).

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## Acknowledgments

- Built with love for the community
- Inspired by the need to keep Downloads folders tidy
- Thanks to all contributors and users

## Contact

- **GitHub Issues**: [Report bugs or request features](https://github.com/ProfessorMoose74/tidy-pack-rat/issues)
- **Discussions**: [Ask questions and share ideas](https://github.com/ProfessorMoose74/tidy-pack-rat/discussions)

---

<p align="center">
  <strong>TidyFlow - Sorting your files, to clean up your mess.</strong><br>
  Made with love for organized file systems
</p>
