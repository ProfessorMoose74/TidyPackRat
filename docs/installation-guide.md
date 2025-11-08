# TidyPackRat Installation Guide

This guide provides detailed instructions for installing TidyPackRat on Windows systems.

## Table of Contents

- [System Requirements](#system-requirements)
- [Pre-Installation Checklist](#pre-installation-checklist)
- [Installation Methods](#installation-methods)
- [Post-Installation Setup](#post-installation-setup)
- [Verification](#verification)
- [Troubleshooting](#troubleshooting)

## System Requirements

### Minimum Requirements

- **Operating System**: Windows 10 (version 1903 or later)
- **RAM**: 2 GB
- **Disk Space**: 50 MB for installation
- **PowerShell**: Version 5.1 or later (included in Windows 10)
- **.NET Framework**: 4.8 (included in Windows 10 version 1903+)

### Recommended Requirements

- **Operating System**: Windows 10 (version 21H1 or later) or Windows 11
- **RAM**: 4 GB or more
- **Disk Space**: 100 MB (including space for logs)
- **Permissions**: Administrator access for installation

## Pre-Installation Checklist

Before installing TidyPackRat, ensure:

1. **Check Windows Version**
   ```powershell
   # Run in PowerShell
   [System.Environment]::OSVersion.Version
   ```
   Should show version 10.0.18362 or higher

2. **Check PowerShell Version**
   ```powershell
   $PSVersionTable.PSVersion
   ```
   Should show version 5.1 or higher

3. **Check .NET Framework Version**
   ```powershell
   # Run in PowerShell
   (Get-ItemProperty "HKLM:\SOFTWARE\Microsoft\NET Framework Setup\NDP\v4\Full").Release -ge 528040
   ```
   Should return `True`

4. **Close Running Applications**
   - Close any file managers viewing your Downloads folder
   - Close any applications that might access files you want to organize

## Installation Methods

### Method 1: MSI Installer (Recommended)

1. **Download the Installer**
   - Go to [Releases](https://github.com/ProfessorMoose74/TidyPackRat/releases)
   - Download `TidyPackRat-Setup.msi` (latest version)

2. **Run the Installer**
   - Double-click `TidyPackRat-Setup.msi`
   - If prompted by User Account Control, click "Yes"

3. **Follow the Installation Wizard**

   **Welcome Screen**
   - Click "Next" to continue

   **License Agreement**
   - Read the MIT License
   - Check "I accept the terms in the License Agreement"
   - Click "Next"

   **Installation Folder**
   - Default: `C:\Program Files\TidyPackRat`
   - Click "Change" to select a different location (not recommended)
   - Click "Next"

   **Ready to Install**
   - Review your choices
   - Click "Install"

   **Installation Progress**
   - Wait for files to be copied and configured
   - The installer will:
     - Copy program files
     - Create configuration directory
     - Add Start Menu shortcuts
     - Optionally create desktop shortcut

   **Completion**
   - Click "Finish"
   - Optionally check "Launch TidyPackRat Configuration"

### Method 2: Manual Installation (Advanced)

For developers or advanced users who want to install from source:

1. **Clone the Repository**
   ```bash
   git clone https://github.com/ProfessorMoose74/TidyPackRat.git
   cd TidyPackRat
   ```

2. **Build the Application**
   ```bash
   # Build the GUI
   msbuild src\gui\TidyPackRat.csproj /p:Configuration=Release

   # The binaries will be in src\gui\bin\Release\
   ```

3. **Manual File Placement**
   ```bash
   # Create directories
   mkdir "C:\Program Files\TidyPackRat"
   mkdir "C:\Program Files\TidyPackRat\GUI"
   mkdir "C:\Program Files\TidyPackRat\Worker"
   mkdir "C:\ProgramData\TidyPackRat"

   # Copy files
   copy src\gui\bin\Release\* "C:\Program Files\TidyPackRat\GUI\"
   copy src\worker\*.ps1 "C:\Program Files\TidyPackRat\Worker\"
   copy config\default-config.json "C:\ProgramData\TidyPackRat\config.json"
   ```

4. **Create Shortcuts** (optional)
   - Right-click on `C:\Program Files\TidyPackRat\GUI\TidyPackRat.exe`
   - Select "Create shortcut"
   - Move shortcut to Start Menu or Desktop

## Post-Installation Setup

### First Launch

1. **Launch TidyPackRat**
   - Start Menu → TidyPackRat → TidyPackRat Configuration
   - Or double-click the desktop shortcut if created

2. **Initial Configuration Wizard** (if enabled)
   - Welcome screen
   - Source folder selection
   - Destination mapping
   - Basic preferences

3. **Review Default Settings**
   - Source folder: `C:\Users\YourName\Downloads`
   - File categories: Images, Documents, Videos, etc.
   - File age threshold: 24 hours
   - Duplicate handling: Rename

### Customize Your Configuration

1. **Adjust File Categories**
   - Enable/disable categories as needed
   - Change destination folders
   - Click "Browse" next to each category

2. **Set Organization Rules**
   - Adjust file age threshold if needed
   - Choose duplicate handling strategy
   - Add exclude patterns

3. **Configure Scheduling** (optional)
   - Check "Enable automatic scheduling"
   - Select frequency (Daily/Weekly/Monthly)
   - Set time to run
   - Optionally enable "Run on startup"

4. **Save Configuration**
   - Click "Save Configuration"
   - If scheduling is enabled, you may be prompted for admin rights

### Test Your Configuration

Before running automatically, test your setup:

1. **Create Test Files** (optional)
   ```powershell
   # Create some test files in Downloads
   echo "test" > $env:USERPROFILE\Downloads\test.txt
   echo "test" > $env:USERPROFILE\Downloads\test.jpg
   ```

2. **Run Dry Run**
   - Click "Test Run (Dry Run)" button
   - Review the PowerShell window output
   - Check what files would be moved

3. **Review Results**
   - No files are actually moved in dry run mode
   - Verify the proposed moves are correct

4. **Run Actual Operation**
   - Click "Run Now" to actually move files
   - Check the log file for confirmation

## Verification

### Verify Installation Files

Check that all files were installed correctly:

```powershell
# Program files
Test-Path "C:\Program Files\TidyPackRat\GUI\TidyPackRat.exe"
Test-Path "C:\Program Files\TidyPackRat\Worker\TidyPackRat-Worker.ps1"

# Configuration
Test-Path "C:\ProgramData\TidyPackRat\config.json"

# Start Menu shortcut
Test-Path "$env:APPDATA\Microsoft\Windows\Start Menu\Programs\TidyPackRat\TidyPackRat Configuration.lnk"
```

All commands should return `True`.

### Verify Scheduled Task (if enabled)

If you enabled scheduling:

1. **Open Task Scheduler**
   - Press Win+R
   - Type `taskschd.msc`
   - Press Enter

2. **Locate TidyPackRat Task**
   - Look for "TidyPackRat-AutoOrganize"
   - Right-click → Properties
   - Verify the trigger and actions

3. **Test Run from Task Scheduler**
   - Right-click the task
   - Select "Run"
   - Check the log file for results

### Verify Logging

After running TidyPackRat at least once:

```powershell
# Check log directory exists
Test-Path "C:\ProgramData\TidyPackRat\logs"

# List log files
Get-ChildItem "C:\ProgramData\TidyPackRat\logs"

# View latest log
Get-Content "C:\ProgramData\TidyPackRat\logs\TidyPackRat-$(Get-Date -Format 'yyyy-MM').log"
```

## Troubleshooting

### Installation Failed

**Error: "This installation is forbidden by system policy"**
- Run the installer as Administrator
- Right-click MSI → "Run as administrator"

**Error: "The installer was interrupted before TidyPackRat could be installed"**
- Disable antivirus temporarily
- Ensure no other installation is in progress
- Check Windows Installer service is running

### Missing Prerequisites

**PowerShell Version Too Old**
- Windows 10 includes PowerShell 5.1 by default
- If needed, install [Windows Management Framework 5.1](https://www.microsoft.com/en-us/download/details.aspx?id=54616)

**.NET Framework 4.8 Missing**
- Download from [Microsoft](https://dotnet.microsoft.com/download/dotnet-framework/net48)
- Install and restart computer

### Permission Issues

If you get permission errors when running:

1. **Run as Administrator**
   - Right-click TidyPackRat Configuration
   - Select "Run as administrator"

2. **Check Folder Permissions**
   ```powershell
   # Check if you have write access to config directory
   $acl = Get-Acl "C:\ProgramData\TidyPackRat"
   $acl.Access | Format-Table
   ```

### Start Menu Shortcuts Missing

If shortcuts weren't created:

1. **Create Manually**
   - Right-click `C:\Program Files\TidyPackRat\GUI\TidyPackRat.exe`
   - Send to → Desktop (create shortcut)
   - Move to Start Menu folder if desired

## Uninstallation

To remove TidyPackRat:

### Method 1: Settings App

1. Open Settings (Win+I)
2. Go to Apps → Apps & features
3. Find "TidyPackRat File Organizer"
4. Click → Uninstall
5. Confirm

### Method 2: Start Menu

1. Start Menu → TidyPackRat
2. Click "Uninstall TidyPackRat"
3. Confirm

### Method 3: Control Panel

1. Control Panel → Programs → Programs and Features
2. Find "TidyPackRat File Organizer"
3. Right-click → Uninstall

### What Gets Removed

- Program files in `C:\Program Files\TidyPackRat`
- Configuration files in `C:\ProgramData\TidyPackRat`
- Start Menu shortcuts
- Desktop shortcut (if created)
- Scheduled task (if enabled)
- Log files

**Note**: Files that have already been organized will remain in their destination folders.

## Next Steps

After installation:

1. Read the [Configuration Guide](configuration-guide.md) for detailed customization
2. Review the [Troubleshooting Guide](troubleshooting.md) for common issues
3. Join the [GitHub Discussions](https://github.com/ProfessorMoose74/TidyPackRat/discussions) for tips and community support

---

**Need Help?** [Open an issue](https://github.com/ProfessorMoose74/TidyPackRat/issues) on GitHub.
