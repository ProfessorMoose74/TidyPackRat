# TidyPackRat Configuration Guide

This guide provides detailed information about configuring TidyPackRat to meet your specific file organization needs.

## Table of Contents

- [Configuration File](#configuration-file)
- [GUI Configuration](#gui-configuration)
- [Advanced Configuration](#advanced-configuration)
- [Custom Categories](#custom-categories)
- [Rules and Filters](#rules-and-filters)
- [Scheduling](#scheduling)
- [Best Practices](#best-practices)

## Configuration File

TidyPackRat stores its configuration in JSON format at:
```
C:\ProgramData\TidyPackRat\config.json
```

### Configuration Structure

```json
{
  "appName": "TidyPackRat",
  "version": "1.0.0",
  "sourceFolder": "C:\\Users\\YourName\\Downloads",
  "fileAgeThreshold": 24,
  "fileSizeThreshold": 0,
  "duplicateHandling": "rename",
  "categories": [ ... ],
  "schedule": { ... },
  "excludePatterns": [ ... ],
  "logging": { ... }
}
```

### Editing the Configuration File

You can edit the configuration file manually:

1. **Using the GUI** (recommended)
   - Launch TidyPackRat Configuration
   - Make changes
   - Click "Save Configuration"

2. **Manually editing JSON**
   - Open `C:\ProgramData\TidyPackRat\config.json` in a text editor
   - Make changes carefully (maintain valid JSON)
   - Save the file
   - Restart TidyPackRat or reload configuration

## GUI Configuration

### Source Folder

**Purpose**: Specifies which folder TidyPackRat will monitor and organize.

**Default**: `C:\Users\YourName\Downloads`

**How to Change**:
1. Click "Browse..." next to Source Folder field
2. Navigate to desired folder
3. Click "OK"
4. Save configuration

**Tips**:
- Can be any folder on your computer
- Commonly used: Downloads, Desktop, Documents subfolders
- Can organize network drives (if always accessible)

### File Categories

Each category represents a type of file and where it should go.

**Category Properties**:
- **Name**: Display name (e.g., "Images")
- **Extensions**: List of file extensions (e.g., .jpg, .png)
- **Destination**: Where files of this type will be moved
- **Enabled**: Whether this category is active

**How to Enable/Disable Categories**:
1. Check/uncheck the "Enabled" checkbox in the data grid
2. Save configuration

**How to Change Destination Folders**:
1. Click "Browse" button for the category
2. Select destination folder
3. Click "OK"
4. Save configuration

**Pre-configured Categories**:

| Category | Default Extensions |
|----------|-------------------|
| Images | .jpg, .jpeg, .png, .gif, .bmp, .svg, .webp, .ico, .tiff, .tif |
| Documents | .pdf, .docx, .doc, .txt, .rtf, .odt, .tex, .wpd |
| Spreadsheets | .xlsx, .xls, .csv, .ods, .xlsm |
| Presentations | .pptx, .ppt, .odp, .key |
| Archives | .zip, .rar, .7z, .tar, .gz, .bz2, .xz, .iso |
| Videos | .mp4, .avi, .mkv, .mov, .wmv, .flv, .webm, .m4v |
| Audio | .mp3, .wav, .flac, .m4a, .ogg, .aac, .wma, .opus |
| Executables | .exe, .msi, .bat, .cmd, .ps1 |
| Code | .py, .js, .html, .css, .cpp, .cs, .java, .php, etc. |

## Advanced Configuration

### File Age Threshold

**Purpose**: Prevents moving files that are too new (might still be downloading or in use).

**Setting**: "Skip files newer than (hours)"

**Default**: 24 hours

**Recommended Values**:
- **1 hour**: For frequently used folders, quick organization
- **24 hours**: Default, safe for most scenarios
- **72 hours**: Very conservative, good for shared folders
- **0 hours**: Organize everything immediately (use with caution!)

**Example Scenarios**:
```json
{
  "fileAgeThreshold": 24,  // Files must be at least 1 day old
  "fileAgeThreshold": 1,   // Files must be at least 1 hour old
  "fileAgeThreshold": 168  // Files must be at least 1 week old
}
```

### File Size Threshold

**Purpose**: Ignore files smaller than a certain size.

**Setting**: "Skip files smaller than (KB)" in the GUI, or `fileSizeThreshold` in config.json

**Default**: 0 (no minimum size)

**How to Change**:
1. In the "Organization Rules" section, find "Skip files smaller than (KB)"
2. Enter a value in kilobytes
3. Click "Save Configuration"

**Common Values**:
| GUI Value (KB) | JSON Value | Description |
|----------------|------------|-------------|
| 0 | 0 | No minimum (default) |
| 1 | 1 | Skip files < 1 KB |
| 10 | 10 | Skip files < 10 KB |
| 100 | 100 | Skip files < 100 KB |
| 1024 | 1024 | Skip files < 1 MB |

**Use Cases**:
- Skip tiny temp files
- Avoid organizing thumbnails
- Focus on substantial files only

**Note**: The value must be 0 or greater. Negative values will display a validation error.

### Duplicate Handling

**Purpose**: Defines what happens when a file with the same name exists in the destination.

**Options**:
1. **Rename** (default): Adds a number to the filename
   - `photo.jpg` → `photo_1.jpg`
   - `photo_1.jpg` → `photo_2.jpg`

2. **Skip**: Leaves the file in the source folder
   - Prevents overwrites
   - File remains in Downloads

**How to Set**:
- GUI: Select from "Duplicate file handling" dropdown
- JSON: `"duplicateHandling": "rename"` or `"skip"`

**Example**:
```json
{
  "duplicateHandling": "rename"  // or "skip"
}
```

### Exclude Patterns

**Purpose**: Prevent certain files from being organized based on filename patterns.

**Default Patterns**:
- `*.tmp` - Temporary files
- `~*` - Office temp files (e.g., ~$document.docx)
- `*.crdownload` - Chrome partial downloads
- `*.part` - Firefox partial downloads

**Wildcard Syntax**:
- `*` matches any characters
- `?` matches a single character
- Patterns are case-insensitive on Windows

**How to Add Patterns**:
1. In GUI: Add each pattern on a new line in "Exclude patterns" box
2. In JSON: Add to `excludePatterns` array

**Examples**:
```json
{
  "excludePatterns": [
    "*.tmp",           // All .tmp files
    "~*",              // Files starting with ~
    "*.crdownload",    // Chrome downloads
    "desktop.ini",     // Specific filename
    "Thumbs.db",       // Windows thumbnail cache
    "*.partial",       // Partial downloads
    "backup_*"         // Files starting with "backup_"
  ]
}
```

## Custom Categories

Currently, custom categories must be added by editing the JSON configuration file.

### Adding a New Category

1. **Open the Configuration File**
   ```
   C:\ProgramData\TidyPackRat\config.json
   ```

2. **Add a New Category Object**
   ```json
   {
     "name": "E-Books",
     "extensions": [".epub", ".mobi", ".azw", ".azw3", ".pdf"],
     "destination": "C:\\Users\\YourName\\Documents\\E-Books",
     "enabled": true
   }
   ```

3. **Full Example**
   ```json
   {
     "categories": [
       {
         "name": "Images",
         "extensions": [".jpg", ".png", ".gif"],
         "destination": "C:\\Users\\YourName\\Pictures",
         "enabled": true
       },
       {
         "name": "E-Books",
         "extensions": [".epub", ".mobi", ".azw", ".azw3"],
         "destination": "C:\\Users\\YourName\\Documents\\E-Books",
         "enabled": true
       }
     ]
   }
   ```

4. **Save and Test**
   - Save the config file
   - Reload TidyPackRat Configuration tool
   - The new category should appear in the grid

### Example Custom Categories

**3D Models**
```json
{
  "name": "3D Models",
  "extensions": [".stl", ".obj", ".fbx", ".blend", ".3ds"],
  "destination": "C:\\Users\\YourName\\Documents\\3D Models",
  "enabled": true
}
```

**Fonts**
```json
{
  "name": "Fonts",
  "extensions": [".ttf", ".otf", ".woff", ".woff2"],
  "destination": "C:\\Users\\YourName\\Documents\\Fonts",
  "enabled": true
}
```

**CAD Files**
```json
{
  "name": "CAD",
  "extensions": [".dwg", ".dxf", ".step", ".stp", ".iges"],
  "destination": "C:\\Users\\YourName\\Documents\\CAD",
  "enabled": true
}
```

**Torrents**
```json
{
  "name": "Torrents",
  "extensions": [".torrent"],
  "destination": "C:\\Users\\YourName\\Downloads\\Torrents",
  "enabled": true
}
```

## Scheduling

### Schedule Configuration

**Enable Automatic Scheduling**
- Check "Enable automatic scheduling" in GUI
- Or set `"enabled": true` in schedule section

**Frequency Options**:
1. **Daily**: Runs every day at specified time
2. **Weekly**: Runs once per week (Mondays by default)
3. **Monthly**: Runs on the 1st of each month

**Time Format**: 24-hour format (HH:mm)
- `02:00` = 2:00 AM
- `14:30` = 2:30 PM
- `23:00` = 11:00 PM

**Run on Startup**
- Runs when you log in to Windows
- Good for organizing files accumulated while PC was off

### Schedule Examples

**Daily at 2 AM**
```json
{
  "schedule": {
    "enabled": true,
    "frequency": "daily",
    "time": "02:00",
    "runOnStartup": false
  }
}
```

**Weekly on Mondays at 6 PM**
```json
{
  "schedule": {
    "enabled": true,
    "frequency": "weekly",
    "time": "18:00",
    "runOnStartup": false
  }
}
```

**Monthly + Run on Startup**
```json
{
  "schedule": {
    "enabled": true,
    "frequency": "monthly",
    "time": "03:00",
    "runOnStartup": true
  }
}
```

### Disabling Scheduling

**Via GUI**:
1. Uncheck "Enable automatic scheduling"
2. Save configuration

**Via JSON**:
```json
{
  "schedule": {
    "enabled": false
  }
}
```

## Logging Configuration

### Log Settings

```json
{
  "logging": {
    "enabled": true,
    "logPath": "C:\\ProgramData\\TidyPackRat\\logs",
    "logLevel": "info",
    "maxLogFiles": 12
  }
}
```

**Properties**:
- `enabled`: Enable/disable logging
- `logPath`: Directory where log files are stored
- `logLevel`: Verbosity (info, warn, error)
- `maxLogFiles`: Maximum number of monthly log files to keep

**Log File Naming**:
- Format: `TidyPackRat-YYYY-MM.log`
- Example: `TidyPackRat-2024-11.log`
- One log file per month

**Log Rotation**:
- Automatically rotates monthly
- Keeps last N months based on `maxLogFiles`
- Old logs are automatically deleted

## Input Validation

TidyPackRat validates your configuration before saving to prevent errors.

### Validated Fields

| Field | Validation Rules |
|-------|------------------|
| Source Folder | Cannot be empty; cannot be system-critical paths (Windows, System32, Program Files, root drive) |
| File Age Threshold | Must be 0 or greater |
| File Size Threshold | Must be 0 or greater |
| Schedule Time | Must be in HH:mm format (00:00 to 23:59) |

### Validation Error Messages

If validation fails, you'll see a message explaining the issue:

- **"Source folder path cannot be empty"**: Enter a valid folder path
- **"Invalid source folder path"**: The path is a protected system location
- **"File age threshold must be 0 or greater"**: Enter a non-negative number
- **"File size threshold must be 0 or greater"**: Enter a non-negative number
- **"Please enter a valid time in HH:mm format"**: Use 24-hour time format (e.g., 02:00, 14:30)

### Time Format Examples

| Valid | Invalid |
|-------|---------|
| 02:00 | 2:00 AM |
| 14:30 | 2:30 PM |
| 00:00 | 24:00 |
| 23:59 | 25:00 |

## Configuration Backup

TidyPackRat automatically creates a backup of your configuration file each time you save.

### Backup Location

```
C:\ProgramData\TidyPackRat\config.json.backup
```

### Restoring from Backup

If your configuration becomes corrupted, you can restore from the backup:

**Method 1: Manual Copy**
```powershell
Copy-Item "C:\ProgramData\TidyPackRat\config.json.backup" `
          "C:\ProgramData\TidyPackRat\config.json" -Force
```

**Method 2: If GUI Won't Load**
1. Navigate to `C:\ProgramData\TidyPackRat\`
2. Delete or rename `config.json`
3. Rename `config.json.backup` to `config.json`
4. Launch TidyPackRat Configuration

### Backup Behavior

- A backup is created before each save operation
- Only the most recent backup is kept
- Backup creation failure does not prevent saving (non-critical)

## Best Practices

### 1. Start Small

Begin with a few enabled categories:
```json
{
  "categories": [
    {"name": "Images", "enabled": true},
    {"name": "Documents", "enabled": true},
    {"name": "Archives", "enabled": false},
    // Others disabled initially
  ]
}
```

### 2. Use Test Run First

Always use "Test Run (Dry Run)" before actual operation:
- Shows what would happen
- No files are actually moved
- Helps verify configuration

### 3. Set Appropriate File Age

Choose based on your usage:
- Active folder (daily use): 24-48 hours
- Occasional use: 72 hours or more
- Archive folder: 168 hours (1 week)

### 4. Backup Important Files

Before first run:
- Backup important files
- Or test with a copy of your Downloads folder

### 5. Review Logs Regularly

Check logs to ensure everything is working:
```powershell
# View latest log
notepad "C:\ProgramData\TidyPackRat\logs\TidyPackRat-$(Get-Date -Format 'yyyy-MM').log"
```

### 6. Exclude Patterns for Safety

Add patterns for files you never want moved:
```json
{
  "excludePatterns": [
    "*.tmp",
    "~*",
    "important_*",
    "*_donotmove*"
  ]
}
```

### 7. Organize by Project or Topic

Create destination folders by project:
```json
{
  "categories": [
    {
      "name": "Work Documents",
      "extensions": [".docx", ".pdf"],
      "destination": "C:\\Users\\YourName\\Work\\Documents",
      "enabled": true
    },
    {
      "name": "Personal Documents",
      "extensions": [".txt"],
      "destination": "C:\\Users\\YourName\\Personal\\Documents",
      "enabled": true
    }
  ]
}
```

### 8. Schedule During Off-Hours

Set schedule when computer is on but you're not using it:
- Early morning: `02:00` or `03:00`
- Late night: `23:00` or `00:00`
- Lunch time: `12:00` or `13:00`

## Troubleshooting Configuration Issues

### Configuration Not Loading

**Symptom**: Changes don't take effect

**Solutions**:
1. Verify JSON syntax is valid (use JSONLint.com)
2. Check file permissions on config.json
3. Restart TidyPackRat Configuration tool
4. Check for error messages in logs

### Files Not Being Moved

**Check**:
1. Category is enabled
2. File extension is in the category's list
3. File age is greater than threshold
4. File doesn't match exclude patterns
5. Destination folder is accessible

### Scheduled Task Not Running

**Verify**:
1. Schedule is enabled in configuration
2. Task exists in Task Scheduler (`taskschd.msc`)
3. Task is enabled
4. Trigger is configured correctly
5. You saved configuration after enabling schedule

## Configuration Templates

### Conservative Template
```json
{
  "fileAgeThreshold": 72,
  "duplicateHandling": "skip",
  "excludePatterns": ["*"]  // Start with all excluded
}
```

### Aggressive Template
```json
{
  "fileAgeThreshold": 1,
  "duplicateHandling": "rename",
  "excludePatterns": ["*.tmp", "~*"]
}
```

### Balanced Template (Recommended)
```json
{
  "fileAgeThreshold": 24,
  "duplicateHandling": "rename",
  "excludePatterns": ["*.tmp", "~*", "*.crdownload", "*.part"]
}
```

---

**Need More Help?** Check the [Troubleshooting Guide](troubleshooting.md) or [open an issue](https://github.com/ProfessorMoose74/TidyPackRat/issues).
