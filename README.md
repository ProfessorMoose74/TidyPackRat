<p align="center">
  <img src="assets/title-logo.png" alt="TidyPackRat" width="600"/>
</p>

<p align="center">
  <b>Sorting your files, to clean up your mess.</b>
</p>

<p align="center">
  <img src="assets/logo.png" alt="TidyPackRat Mascot" width="150"/>
</p>

[![License: MIT](https://img.shields.io/badge/License-MIT-blue.svg)](LICENSE)
[![Version](https://img.shields.io/badge/version-1.0.0-green.svg)]()
[![Platform](https://img.shields.io/badge/platform-Windows%2010%2B-lightgrey.svg)]()

TidyPackRat is an open-source automated file management system for Windows that intelligently organizes files from your Downloads folder (or any source folder) into categorized destination folders based on file type. Set it up once, and let TidyPackRat keep your files tidy!

## Features

- **Automatic File Organization**: Moves files from a source folder to categorized destinations based on file extensions
- **Smart Rules**: Configure file age thresholds, size filters, and exclude patterns
- **Flexible Scheduling**: Run manually, on a schedule (daily/weekly/monthly), or on system startup
- **Safe Operation**: Test mode (dry run) to preview what will be moved before committing
- **Duplicate Handling**: Configurable strategies for handling duplicate files
- **Comprehensive Logging**: Track every file movement with detailed logs
- **User-Friendly GUI**: Easy-to-use WPF configuration interface
- **Windows Integration**: MSI installer with Task Scheduler integration
- **Input Validation**: Validates time formats, paths, and thresholds before saving
- **Configuration Backup**: Automatic backup of your settings before each save

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

- Windows 10 or later
- PowerShell 5.1 or later (included in Windows 10)
- .NET Framework 4.8 (included in Windows 10 version 1903+)

### Install Steps

1. Download the latest `TidyPackRat-Setup.msi` from the [Releases](https://github.com/ProfessorMoose74/TidyPackRat/releases) page
2. Run the installer and follow the setup wizard
3. Launch "TidyPackRat Configuration" from the Start Menu
4. Configure your file organization preferences
5. Click "Save Configuration" to apply settings

## Quick Start

1. **Launch the Configuration Tool**: Find "TidyPackRat Configuration" in your Start Menu

2. **Set Your Source Folder**: The default is your Downloads folder, but you can choose any folder

3. **Review File Categories**: Each category (Images, Documents, Videos, etc.) has:
   - A list of file extensions
   - A destination folder
   - An enable/disable toggle

4. **Configure Rules**:
   - **File Age**: Skip files newer than X hours (default: 24 hours)
   - **File Size**: Skip files smaller than X KB (default: 0 = no minimum)
   - **Duplicates**: Choose to rename or skip duplicate files
   - **Exclude Patterns**: Wildcards to skip certain files (e.g., `*.tmp`)

5. **Test First**: Click "Test Run (Dry Run)" to see what would be moved without actually moving files

6. **Run or Schedule**:
   - Click "Run Now" to organize files immediately
   - Enable scheduling to run automatically

## Configuration

### Source Folder

The folder TidyPackRat monitors for files to organize. Default is your Downloads folder:
```
C:\Users\YourName\Downloads
```

### File Categories

TidyPackRat comes with pre-configured categories. You can:
- Enable/disable any category
- Change destination folders
- Add custom extensions (by editing the config file)

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

Enable automatic scheduling to keep your folders organized without manual intervention:

- **Frequency**: Daily, Weekly, or Monthly
- **Time**: Specify what time to run (e.g., 2:00 AM)
- **Run on Startup**: Optionally run when you log in to Windows

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

### GUI Configuration Tool

The WPF application provides an intuitive interface for:
- Configuring source and destination folders
- Enabling/disabling file categories
- Setting organization rules
- Managing schedules
- Running test/actual operations
- Viewing logs

### PowerShell Worker Script

The worker script can also be run directly from PowerShell:

```powershell
# Standard run
.\TidyPackRat-Worker.ps1 -ConfigPath "C:\ProgramData\TidyPackRat\config.json"

# Dry run (test mode)
.\TidyPackRat-Worker.ps1 -ConfigPath "C:\ProgramData\TidyPackRat\config.json" -DryRun -VerboseLogging

# Custom config file
.\TidyPackRat-Worker.ps1 -ConfigPath "C:\path\to\my-config.json"
```

### Task Scheduler Integration

When scheduling is enabled, TidyPackRat creates a Windows scheduled task named `TidyPackRat-AutoOrganize`. You can also manage this task directly through Windows Task Scheduler if needed.

## Documentation

Detailed documentation is available in the [`docs`](docs/) directory:

- [Installation Guide](docs/installation-guide.md) - Step-by-step installation instructions
- [Configuration Guide](docs/configuration-guide.md) - Detailed configuration options
- [Troubleshooting](docs/troubleshooting.md) - Common issues and solutions

## Building from Source

### Prerequisites

- Visual Studio 2017 or newer
- .NET Framework 4.8 SDK
- WiX Toolset v3.11+
- PowerShell 5.1+

### Build Steps

1. Clone the repository:
   ```bash
   git clone https://github.com/ProfessorMoose74/TidyPackRat.git
   cd TidyPackRat
   ```

2. Build the GUI application:
   ```bash
   msbuild src\gui\TidyPackRat.csproj /p:Configuration=Release
   ```

3. Prepare build directory:
   ```bash
   mkdir build\gui build\worker build\config
   copy src\gui\bin\Release\* build\gui\
   copy src\worker\*.ps1 build\worker\
   copy config\*.json build\config\
   ```

4. Build the installer:
   ```bash
   msbuild src\installer\TidyPackRat.Installer.wixproj /p:Configuration=Release
   ```

5. Find the installer at:
   ```
   src\installer\bin\Release\TidyPackRat-Setup.msi
   ```

## Contributing

We welcome contributions! Please see [CONTRIBUTING.md](CONTRIBUTING.md) for details on:

- Code of conduct
- How to submit issues
- Pull request process
- Coding standards

## Roadmap

### Phase 1: Windows Individual Users (Current - v1.0.0)
- [x] Core file organization engine
- [x] WPF configuration GUI
- [x] MSI installer
- [x] Task Scheduler integration
- [x] Comprehensive logging

### Phase 2: Enhanced Features (v1.1.0 - v1.2.0)
- [ ] Custom category creation in GUI
- [ ] Advanced filtering (regex patterns)
- [ ] Undo/rollback functionality
- [ ] File preview before moving
- [ ] Statistics and reporting dashboard

### Phase 3: Linux Version (v2.0.0)
- [ ] Bash/Python worker script
- [ ] GTK or Qt GUI
- [ ] Cron integration
- [ ] Package for major distros (deb, rpm)

### Phase 4: Enterprise Features (v3.0.0)
- [ ] Multi-user deployment
- [ ] Centralized configuration management
- [ ] Network share support
- [ ] Active Directory integration
- [ ] Group Policy templates

### Phase 5: Cloud Integration (v4.0.0)
- [ ] OneDrive integration
- [ ] Google Drive integration
- [ ] Dropbox support
- [ ] Custom cloud storage providers

## Uninstallation

To uninstall TidyPackRat:

1. Open Windows Settings → Apps → Apps & features
2. Find "TidyPackRat File Organizer"
3. Click Uninstall

Or use the Start Menu shortcut: Start → TidyPackRat → Uninstall TidyPackRat

**Note**: Your configuration file and logs will be removed. Files that have already been organized will remain in their destination folders.

## Troubleshooting

### Files aren't being moved

- Check that the category is enabled
- Verify the file extension is in the category's list
- Check the file age threshold setting
- Review logs in `C:\ProgramData\TidyPackRat\logs`

### Scheduled task isn't running

- Ensure you saved the configuration after enabling scheduling
- Verify the task exists in Task Scheduler
- Check Task Scheduler logs
- Run the configuration tool as Administrator

For more help, see [Troubleshooting Guide](docs/troubleshooting.md) or [open an issue](https://github.com/ProfessorMoose74/TidyPackRat/issues).

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## Acknowledgments

- Built with ❤️ for the community
- Inspired by the need to keep Downloads folders tidy
- Thanks to all contributors and users

## Contact

- **GitHub Issues**: [Report bugs or request features](https://github.com/ProfessorMoose74/TidyPackRat/issues)
- **Discussions**: [Ask questions and share ideas](https://github.com/ProfessorMoose74/TidyPackRat/discussions)

---

<p align="center">
  <strong>TidyPackRat - Sorting your files, to clean up your mess.</strong><br>
  Made with ❤️ for organized file systems
</p>
