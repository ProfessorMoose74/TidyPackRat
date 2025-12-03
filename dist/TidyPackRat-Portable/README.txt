================================================================================
  TidyPackRat v1.1.0 - Portable Edition
  "Sorting your files, to clean up your mess."
================================================================================

WHAT IS THIS?
-------------
TidyPackRat is an automated file management system that organizes your
Downloads folder (or any folder) into categorized destinations based on
file type.

This is the PORTABLE version - no installation required!


QUICK START
-----------
1. Double-click "Launch-TidyPackRat.bat" to run the configuration tool

   OR

   Run GUI\TidyPackRat.exe directly

2. Configure your source folder and destinations
3. Click "Test Run (Dry Run)" to preview
4. Click "Run Now" to organize your files
5. Optionally set up scheduling for automatic organization


WHAT'S INCLUDED?
----------------
GUI\
  - TidyPackRat.exe          : Configuration application
  - Newtonsoft.Json.dll      : Required dependency
  - TidyPackRat.exe.config   : Application settings

Worker\
  - TidyPackRat-Worker.ps1   : PowerShell automation script

Config\
  - default-config.json      : Default configuration template


SYSTEM REQUIREMENTS
-------------------
- Windows 10 or later
- PowerShell 5.1+ (included in Windows 10)
- .NET Framework 4.8 (included in Windows 10 version 1903+)


FIRST TIME SETUP
----------------
When you first run TidyPackRat, it will:
1. Create a configuration file in: %PROGRAMDATA%\TidyPackRat\config.json
2. Set up a logs folder in: %PROGRAMDATA%\TidyPackRat\logs

Your configuration will be saved and persist between runs.


FEATURES
--------
✓ 9 pre-configured file categories (Images, Documents, Videos, etc.)
✓ Smart file age filtering (skip recently created files)
✓ Duplicate file handling (rename or skip)
✓ Dry-run test mode
✓ Comprehensive logging
✓ Flexible scheduling (via Windows Task Scheduler)
✓ Exclude patterns to skip certain files


PORTABLE VS INSTALLER
----------------------
PORTABLE (this version):
  ✓ No installation required
  ✓ Run from any folder
  ✓ Easy to move or backup
  ✓ No admin rights needed to run
  ✗ Manual Task Scheduler setup for automation
  ✗ No Start Menu shortcuts

INSTALLER (MSI):
  ✓ Professional installation
  ✓ Start Menu shortcuts
  ✓ Automatic Task Scheduler integration
  ✓ Desktop shortcut (optional)
  ✗ Requires installation
  ✗ May trigger Windows Defender warnings


SETTING UP SCHEDULED AUTOMATION (OPTIONAL)
-------------------------------------------
If you want automatic file organization:

1. Run the GUI and configure your settings
2. Check "Enable automatic scheduling"
3. Set your preferred frequency and time
4. Click "Save Configuration"
5. TidyPackRat will attempt to create a scheduled task

Note: You may need to run as Administrator for Task Scheduler access.


TROUBLESHOOTING
---------------
If the GUI won't start:
  - Ensure .NET Framework 4.8 is installed
  - Check that Newtonsoft.Json.dll is in the GUI folder

If files aren't being moved:
  - Verify the file category is enabled
  - Check the file age threshold (default: 24 hours)
  - Review logs in %PROGRAMDATA%\TidyPackRat\logs

If you get PowerShell errors:
  - Run: powershell -ExecutionPolicy Bypass
  - Then navigate to Worker folder and run the script


DOCUMENTATION
-------------
Full documentation available at:
https://github.com/ProfessorMoose74/TidyPackRat

- Quick Start Guide
- Installation Guide
- Configuration Guide
- Troubleshooting Guide


GETTING HELP
------------
GitHub Issues: https://github.com/ProfessorMoose74/TidyPackRat/issues
GitHub Discussions: https://github.com/ProfessorMoose74/TidyPackRat/discussions


LICENSE
-------
TidyPackRat is open-source software licensed under the MIT License.
See: https://github.com/ProfessorMoose74/TidyPackRat/blob/main/LICENSE


CREDITS
-------
Created with ❤️ for organized file systems
Logo: Pack rat mascot with glasses

================================================================================
Thank you for using TidyPackRat!
"Sorting your files, to clean up your mess." 🐭
================================================================================
