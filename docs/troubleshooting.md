# TidyPackRat Troubleshooting Guide

This guide helps you diagnose and fix common issues with TidyPackRat.

## Table of Contents

- [General Troubleshooting](#general-troubleshooting)
- [Installation Issues](#installation-issues)
- [Configuration Issues](#configuration-issues)
- [File Organization Issues](#file-organization-issues)
- [Scheduling Issues](#scheduling-issues)
- [Performance Issues](#performance-issues)
- [Error Messages](#error-messages)
- [Getting Help](#getting-help)

## General Troubleshooting

### Check the Logs

Logs are your first resource for troubleshooting. They contain detailed information about what TidyPackRat is doing.

**Log Location**:
```
C:\ProgramData\TidyPackRat\logs\TidyPackRat-YYYY-MM.log
```

**View Logs**:
```powershell
# View latest log file
notepad "C:\ProgramData\TidyPackRat\logs\TidyPackRat-$(Get-Date -Format 'yyyy-MM').log"

# Or from GUI
Click "View Log" button in TidyPackRat Configuration
```

**What to Look For**:
- Error messages in red
- Warnings in yellow
- Files that were skipped and why
- Actual file movements

### Run in Dry Run Mode

Before troubleshooting further, run in dry run mode to see what TidyPackRat would do:

**Via GUI**:
1. Click "Test Run (Dry Run)" button
2. Watch the PowerShell window
3. Check what files would be moved

**Via PowerShell**:
```powershell
cd "C:\Program Files\TidyPackRat\Worker"
.\TidyPackRat-Worker.ps1 -ConfigPath "C:\ProgramData\TidyPackRat\config.json" -DryRun -VerboseLogging
```

### Verify Installation

Check that all components are installed:

```powershell
# Check executable
Test-Path "C:\Program Files\TidyPackRat\GUI\TidyPackRat.exe"

# Check worker script
Test-Path "C:\Program Files\TidyPackRat\Worker\TidyPackRat-Worker.ps1"

# Check configuration
Test-Path "C:\ProgramData\TidyPackRat\config.json"

# All should return True
```

## Installation Issues

### Installer Won't Run

**Error**: "Windows cannot access the specified device, path, or file"

**Solutions**:
1. Right-click installer → Properties → Unblock
2. Run as Administrator (Right-click → Run as administrator)
3. Temporarily disable antivirus
4. Download installer again (may be corrupted)

**Error**: "This installation is forbidden by system policy"

**Solutions**:
1. Run as Administrator
2. Check Group Policy settings (if on a company computer)
3. Contact your IT department

### Installation Hangs or Fails

**Symptoms**:
- Installer stops responding
- Installation never completes
- Error during installation

**Solutions**:
1. Close all running applications
2. Disable antivirus temporarily
3. Restart computer and try again
4. Check Windows Installer service:
   ```powershell
   Get-Service -Name msiserver
   # Should show "Running"

   # If not running:
   Start-Service -Name msiserver
   ```
5. Check disk space (need at least 100 MB free)

### Missing Start Menu Shortcuts

**Solutions**:
1. Manually create shortcut:
   - Navigate to `C:\Program Files\TidyPackRat\GUI`
   - Right-click `TidyPackRat.exe`
   - Send to → Desktop (create shortcut)
2. Repair installation:
   - Run installer again
   - Choose "Repair"

## Configuration Issues

### Configuration File Not Found

**Error**: "Configuration file not found"

**Solutions**:
1. Check if file exists:
   ```powershell
   Test-Path "C:\ProgramData\TidyPackRat\config.json"
   ```

2. Restore default configuration:
   ```powershell
   # Copy default config
   Copy-Item "C:\Program Files\TidyPackRat\config\default-config.json" `
             "C:\ProgramData\TidyPackRat\config.json"
   ```

3. Launch GUI and save configuration to recreate file

### Configuration Changes Not Saving

**Symptoms**:
- Save button doesn't work
- Changes revert after closing GUI
- Error when saving

**Solutions**:
1. Run GUI as Administrator:
   - Right-click TidyPackRat Configuration
   - Select "Run as administrator"

2. Check file permissions:
   ```powershell
   # View permissions
   Get-Acl "C:\ProgramData\TidyPackRat" | Format-List

   # Grant yourself write access (run as admin)
   $acl = Get-Acl "C:\ProgramData\TidyPackRat"
   $permission = "$env:USERNAME", "FullControl", "Allow"
   $rule = New-Object System.Security.AccessControl.FileSystemAccessRule $permission
   $acl.SetAccessRule($rule)
   Set-Acl "C:\ProgramData\TidyPackRat" $acl
   ```

3. Check if file is read-only:
   ```powershell
   # Remove read-only attribute
   Set-ItemProperty "C:\ProgramData\TidyPackRat\config.json" -Name IsReadOnly -Value $false
   ```

### Invalid JSON Configuration

**Error**: "Failed to load configuration" or "Invalid configuration file"

**Symptoms**:
- GUI shows error on startup
- Worker script fails to run

**Solutions**:
1. Validate JSON syntax:
   - Copy config file contents
   - Paste into JSONLint.com
   - Fix any syntax errors

2. Common JSON mistakes:
   ```json
   // WRONG - Missing comma
   {
     "name": "Images"
     "enabled": true
   }

   // CORRECT
   {
     "name": "Images",
     "enabled": true
   }

   // WRONG - Trailing comma
   {
     "name": "Images",
     "enabled": true,
   }

   // CORRECT
   {
     "name": "Images",
     "enabled": true
   }
   ```

3. Restore from automatic backup:
   ```powershell
   # TidyPackRat creates a backup before each save
   Copy-Item "C:\ProgramData\TidyPackRat\config.json.backup" `
             "C:\ProgramData\TidyPackRat\config.json" -Force
   ```

4. Restore default configuration:
   ```powershell
   # If backup is also corrupted, restore from default
   Copy-Item "C:\Program Files\TidyPackRat\config\default-config.json" `
             "C:\ProgramData\TidyPackRat\config.json" -Force
   ```

### Validation Errors When Saving

**Symptoms**:
- Error message appears when clicking "Save Configuration"
- Configuration not saved

**Common Validation Errors and Solutions**:

| Error Message | Solution |
|---------------|----------|
| "Source folder path cannot be empty" | Enter a valid folder path |
| "Invalid source folder path" | Don't use system folders (Windows, Program Files, root drive) |
| "File age threshold must be 0 or greater" | Enter 0 or a positive number |
| "File size threshold must be 0 or greater" | Enter 0 or a positive number |
| "Please enter a valid time in HH:mm format" | Use 24-hour format like 02:00 or 14:30 |

**Time Format Requirements**:
- Hours: 00-23 (24-hour format)
- Minutes: 00-59
- Format: HH:mm (e.g., 02:00, 14:30, 23:59)
- Invalid examples: 2:00 AM, 25:00, 12:60

## File Organization Issues

### Files Aren't Being Moved

**Symptoms**:
- Files remain in source folder
- No errors in log
- Test run shows files should be moved

**Checklist**:

1. **Is the category enabled?**
   ```json
   {
     "name": "Images",
     "enabled": true  // Must be true
   }
   ```

2. **Is the file extension in the category?**
   - Check category's `extensions` array
   - Extensions are case-insensitive
   - Must include the dot: `.jpg` not `jpg`

3. **Is the file too new?**
   ```powershell
   # Check file age
   $file = Get-Item "C:\Users\YourName\Downloads\file.jpg"
   $age = (Get-Date) - $file.LastWriteTime
   Write-Host "File is $($age.TotalHours) hours old"

   # Compare with threshold in config
   # If file age < threshold, it won't be moved
   ```

4. **Does it match an exclude pattern?**
   - Check `excludePatterns` in config
   - Common patterns: `*.tmp`, `~*`, `*.part`

5. **Is the destination accessible?**
   ```powershell
   # Test destination folder
   Test-Path "C:\Users\YourName\Pictures"

   # Try creating a test file there
   "test" | Out-File "C:\Users\YourName\Pictures\test.txt"
   ```

6. **Run with verbose logging**:
   ```powershell
   .\TidyPackRat-Worker.ps1 -ConfigPath "C:\ProgramData\TidyPackRat\config.json" `
                             -VerboseLogging
   ```

### Files Being Moved to Wrong Location

**Symptoms**:
- Files end up in unexpected folders
- Wrong category being used

**Solutions**:

1. **Check for overlapping extensions**:
   ```json
   // If .pdf is in multiple categories, first enabled one wins
   {
     "categories": [
       {
         "name": "Documents",
         "extensions": [".pdf"],  // This will be used
         "enabled": true
       },
       {
         "name": "Work Docs",
         "extensions": [".pdf"],  // This won't be used for .pdf
         "enabled": true
       }
     ]
   }
   ```

2. **Verify destination paths**:
   - Check each category's `destination` field
   - Ensure paths are correct
   - Check for typos

3. **Use dry run to verify**:
   ```powershell
   .\TidyPackRat-Worker.ps1 -DryRun -VerboseLogging
   ```

### Duplicate Files Not Being Handled Correctly

**Symptoms**:
- Files with same name causing errors
- Files being skipped unexpectedly
- File names not being renamed

**Solutions**:

1. **Check duplicate handling setting**:
   ```json
   {
     "duplicateHandling": "rename"  // or "skip"
   }
   ```

2. **Verify rename logic**:
   - `rename`: Adds `_1`, `_2`, etc.
   - `skip`: Leaves file in source folder

3. **Check permissions** on destination:
   ```powershell
   # Ensure you can write to destination
   $dest = "C:\Users\YourName\Pictures"
   "test" | Out-File "$dest\test.txt"
   ```

### Files with Special Characters

**Symptoms**:
- Files with brackets, ampersands, etc. not moving
- Errors with certain filenames

**Solutions**:

The worker script should handle special characters, but if you encounter issues:

1. **Check logs** for specific errors
2. **Rename problematic files** manually:
   ```powershell
   # Remove special characters
   Get-ChildItem "C:\Users\YourName\Downloads" | ForEach-Object {
     $newName = $_.Name -replace '[^\w\s\.\-]', '_'
     if ($newName -ne $_.Name) {
       Rename-Item $_.FullName -NewName $newName
     }
   }
   ```

3. **Add to exclude patterns** if specific files cause issues

## Scheduling Issues

### Scheduled Task Not Running

**Symptoms**:
- Files not being organized automatically
- Task doesn't execute at scheduled time

**Troubleshooting Steps**:

1. **Verify task exists**:
   - Open Task Scheduler (`taskschd.msc`)
   - Look for "TidyPackRat-AutoOrganize"
   - Check if it exists

2. **Check task is enabled**:
   - Right-click task → Properties
   - Ensure "Enabled" is checked at bottom

3. **Verify trigger**:
   - Open task properties → Triggers tab
   - Check frequency and time
   - Ensure trigger is enabled

4. **Check last run result**:
   - Select task in Task Scheduler
   - Look at "Last Run Result" column
   - `0x0` = Success
   - Other codes = Error

5. **View task history**:
   - Right-click task → View History
   - Check for errors or warnings

6. **Manually run task**:
   - Right-click task → Run
   - Check if it runs successfully
   - Review logs

7. **Check PowerShell execution policy**:
   ```powershell
   Get-ExecutionPolicy
   # Should allow scripts

   # If restricted, set to RemoteSigned (run as admin)
   Set-ExecutionPolicy -ExecutionPolicy RemoteSigned -Scope LocalMachine
   ```

### Task Runs But Files Not Organized

**Symptoms**:
- Task shows as successful
- But files aren't moved

**Solutions**:

1. **Check task action**:
   - Task properties → Actions tab
   - Verify command is correct:
     ```
     powershell.exe
     -ExecutionPolicy Bypass -File "C:\Program Files\TidyPackRat\Worker\TidyPackRat-Worker.ps1" -ConfigPath "C:\ProgramData\TidyPackRat\config.json"
     ```

2. **Check logs after scheduled run**:
   ```powershell
   # View log
   notepad "C:\ProgramData\TidyPackRat\logs\TidyPackRat-$(Get-Date -Format 'yyyy-MM').log"
   ```

3. **Test manually**:
   ```powershell
   # Run the exact command the task uses
   powershell.exe -ExecutionPolicy Bypass -File "C:\Program Files\TidyPackRat\Worker\TidyPackRat-Worker.ps1" -ConfigPath "C:\ProgramData\TidyPackRat\config.json"
   ```

### Task Runs Multiple Times

**Symptoms**:
- Files being moved repeatedly
- Multiple log entries

**Solutions**:

1. **Check for duplicate tasks**:
   - Open Task Scheduler
   - Search for all TidyPackRat tasks
   - Delete duplicates

2. **Check trigger settings**:
   - Ensure "Repeat task every" is not set
   - Remove any extra triggers

3. **Recreate task**:
   - Delete existing task
   - Save configuration again in GUI

## Performance Issues

### TidyPackRat Runs Slowly

**Symptoms**:
- Takes a long time to process files
- High CPU or disk usage

**Solutions**:

1. **Large number of files**:
   - This is normal for thousands of files
   - Consider organizing more frequently

2. **Slow network drives**:
   - Avoid organizing files on network shares
   - Or increase file age threshold

3. **Check destination disk space**:
   ```powershell
   # Check free space
   Get-PSDrive C | Select-Object Used,Free
   ```

4. **Exclude unnecessary patterns**:
   - Review and optimize exclude patterns
   - Avoid overly complex wildcards

### High Memory Usage

**Symptoms**:
- PowerShell using lots of RAM
- System slows down during organization

**Solutions**:

1. **Normal for large operations**:
   - PowerShell will use memory for file processing
   - Memory released after completion

2. **Schedule during off-hours**:
   - Set schedule for times when you're not using PC

3. **Process folders in batches**:
   - Organize smaller sets of files more frequently

## Error Messages

### "Access Denied"

**Causes**:
- Insufficient permissions
- File in use by another program
- Protected system files

**Solutions**:
1. Run as Administrator
2. Close programs using the files
3. Add files to exclude patterns if they're system files
4. Check file/folder permissions

### "Path Too Long"

**Error**: Path exceeds Windows 260-character limit

**Solutions**:
1. Enable long paths in Windows 10:
   ```powershell
   # Run as Administrator
   New-ItemProperty -Path "HKLM:\SYSTEM\CurrentControlSet\Control\FileSystem" `
                     -Name "LongPathsEnabled" -Value 1 -PropertyType DWORD -Force
   ```

2. Shorten destination folder paths
3. Rename files with very long names

### "Execution Policy" Errors

**Error**: "...cannot be loaded because running scripts is disabled..."

**Solutions**:
```powershell
# Check current policy
Get-ExecutionPolicy

# Set to RemoteSigned (run as admin)
Set-ExecutionPolicy -ExecutionPolicy RemoteSigned -Scope LocalMachine

# Or bypass for single run
powershell.exe -ExecutionPolicy Bypass -File "path\to\script.ps1"
```

### "File Not Found"

**Causes**:
- Configuration file missing
- Worker script not found
- Source or destination folder doesn't exist

**Solutions**:
1. Verify all paths exist
2. Reinstall if program files are missing
3. Restore default configuration

## Security Considerations

### Protected Folder Restrictions

TidyPackRat prevents you from using certain system-critical folders as the source folder to protect your system:

**Blocked Source Folders**:
- `C:\Windows` (and subfolders)
- `C:\Windows\System32`
- `C:\Program Files`
- `C:\Program Files (x86)`
- Root drives (e.g., `C:\`)

**Why?** Organizing files in these locations could break Windows or installed applications.

**Solution**: Use user folders like Downloads, Desktop, or custom folders in your user profile.

### Administrator Requirements

Some operations require administrator privileges:

| Operation | Requires Admin? |
|-----------|-----------------|
| Installing TidyPackRat | Yes |
| Creating scheduled tasks | Sometimes (depends on system policy) |
| Running the worker script | No |
| Editing configuration | No (unless file permissions restrict it) |

**If you get "Access Denied" when creating a scheduled task**:
1. Close TidyPackRat Configuration
2. Right-click → Run as administrator
3. Enable scheduling and save again

### PowerShell Execution Policy

TidyPackRat uses `-ExecutionPolicy Bypass` when running the worker script. This is necessary because:
- The script is locally installed and trusted
- It allows the scheduled task to run without policy restrictions

**Note**: This only affects the TidyPackRat script execution, not your system-wide policy.

### Configuration File Security

Your configuration is stored at:
```
C:\ProgramData\TidyPackRat\config.json
```

**Best Practices**:
- Don't store sensitive information in folder paths
- The file is readable by all users on the system
- A backup is automatically created before each save

## Getting Help

If you've tried these troubleshooting steps and still have issues:

### Collect Information

1. **TidyPackRat version**: Check Help → About (or README)
2. **Windows version**:
   ```powershell
   [System.Environment]::OSVersion.Version
   ```
3. **PowerShell version**:
   ```powershell
   $PSVersionTable.PSVersion
   ```
4. **Recent log file**:
   ```
   C:\ProgramData\TidyPackRat\logs\TidyPackRat-YYYY-MM.log
   ```
5. **Configuration file** (remove sensitive paths):
   ```
   C:\ProgramData\TidyPackRat\config.json
   ```
6. **Error messages**: Copy exact error text

### Get Support

1. **GitHub Issues**: [Report a bug](https://github.com/ProfessorMoose74/TidyPackRat/issues)
   - Include collected information above
   - Describe what you expected vs. what happened
   - Steps to reproduce the issue

2. **GitHub Discussions**: [Ask for help](https://github.com/ProfessorMoose74/TidyPackRat/discussions)
   - For general questions
   - Share tips and tricks
   - Request features

3. **Check Existing Issues**:
   - Someone may have already reported your issue
   - Check closed issues too (may have solution)

### Community Resources

- Review [Configuration Guide](configuration-guide.md) for advanced options
- Check [Installation Guide](installation-guide.md) for setup help
- Browse [GitHub Discussions](https://github.com/ProfessorMoose74/TidyPackRat/discussions) for community tips

---

**Still stuck?** [Open an issue](https://github.com/ProfessorMoose74/TidyPackRat/issues) with detailed information about your problem.
