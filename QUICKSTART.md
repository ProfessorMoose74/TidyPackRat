# TidyFlow Quick Start Guide

Get TidyFlow up and running in 5 minutes!

## For End Users (Installation)

### Step 1: Download & Install
1. Download `TidyFlow-Setup.msi` from [Releases](https://github.com/ProfessorMoose74/TidyFlow/releases)
2. Double-click to run the installer
3. Follow the wizard (just click Next → Next → Install → Finish)

### Step 2: Configure
1. Launch "TidyFlow Configuration" from Start Menu
2. The source folder defaults to your Downloads folder (you can change it)
3. Review the file categories - they're pre-configured for common file types
4. Click "Save Configuration"

### Step 3: Test It
1. Click "Test Run (Dry Run)" button
2. Watch what files would be moved (nothing actually moves yet)
3. If it looks good, click "Run Now" to actually organize your files

### Step 4: Set It and Forget It (Optional)
1. Check "Enable automatic scheduling"
2. Choose how often (Daily is recommended)
3. Set the time (2:00 AM is default)
4. Click "Save Configuration"

**Done!** Your files will now be automatically organized.

---

## For Developers (Build from Source)

### Prerequisites
```powershell
# Check you have the required tools
dotnet --version  # Should be 4.8 or compatible
msbuild /?        # Should show MSBuild help
```

### Quick Build
```powershell
# Clone the repo
git clone https://github.com/ProfessorMoose74/TidyFlow.git
cd TidyFlow

# Build the application
.\build.ps1 -Configuration Release

# Test it
.\build\gui\TidyFlow.exe
```

### Build the Installer
```powershell
# Install WiX Toolset first: https://wixtoolset.org/releases/

# Build with installer
.\build.ps1 -Configuration Release -BuildInstaller

# Installer will be at:
# src\installer\bin\Release\TidyFlow-Setup.msi
```

---

## Common Use Cases

### 1. Organize Downloads Folder
**Default setup does this!** Just install and run.

### 2. Organize Desktop
1. Change source folder to Desktop
2. Save configuration
3. Run now or schedule

### 3. Custom File Types
Edit `C:\ProgramData\TidyFlow\config.json` and add:
```json
{
  "name": "E-Books",
  "extensions": [".epub", ".mobi", ".pdf"],
  "destination": "C:\\Users\\YourName\\Documents\\E-Books",
  "enabled": true
}
```

### 4. Very Conservative (Only Move Old Files)
1. Set "Skip files newer than" to 168 hours (1 week)
2. Set duplicate handling to "Skip"
3. Add more exclude patterns if needed

---

## Troubleshooting

### Files aren't moving
- Check the category is enabled (checkbox in GUI)
- Verify file age is > 24 hours (or your threshold)
- Click "View Log" to see what happened

### Scheduled task not running
- Make sure you clicked "Save Configuration" after enabling schedule
- Check Task Scheduler (Win+R → `taskschd.msc`) for "TidyFlow-AutoOrganize"

### Need more help?
- Read the [full documentation](docs/)
- Check [Troubleshooting Guide](docs/troubleshooting.md)
- [Open an issue](https://github.com/ProfessorMoose74/TidyFlow/issues)

---

## That's It!

TidyFlow is designed to be simple. Install, configure once, and forget about file organization forever.

**Happy organizing!**
