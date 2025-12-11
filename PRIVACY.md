# Privacy Policy for TidyFlow

**Last Updated:** December 11, 2025

## Overview

TidyFlow is a local file organization utility for Windows. Your privacy is important to us, and we want to be transparent about how TidyFlow handles your data.

## Data Collection

**TidyFlow does not collect, transmit, or store any personal data.**

Specifically:

- **No telemetry**: We do not collect usage statistics, crash reports, or analytics
- **No network access**: TidyFlow operates entirely offline and makes no internet connections
- **No cloud sync**: Your configuration and logs stay on your local machine
- **No personal information**: We do not collect names, email addresses, or any identifying information

## Local Data Storage

TidyFlow stores the following data locally on your computer:

### Configuration File
- **Location**: `C:\ProgramData\TidyFlow\config.json`
- **Contents**: Your file organization preferences (source folder, destination folders, scheduling settings)
- **Purpose**: To remember your settings between sessions

### Log Files
- **Location**: `C:\ProgramData\TidyFlow\logs\`
- **Contents**: Records of file organization operations (file names, source/destination paths, timestamps)
- **Purpose**: To help you track what files were moved and troubleshoot issues
- **Retention**: Logs are automatically rotated monthly; maximum 12 log files kept

### Configuration Backups
- **Location**: `C:\ProgramData\TidyFlow\config.json.backup`
- **Contents**: Backup of your previous configuration
- **Purpose**: To allow recovery if settings are accidentally changed

## File Access

TidyFlow accesses files only in the folders you configure:

- **Source folder**: The folder you specify for monitoring (default: Downloads)
- **Destination folders**: The folders you specify for each category
- **Log folder**: For writing operation logs

TidyFlow does not access, read, or modify files outside of these configured locations.

## Third-Party Services

TidyFlow does not integrate with or send data to any third-party services.

## Data Security

- All data remains on your local machine
- No encryption is applied to configuration or log files (they contain no sensitive data)
- Standard Windows file permissions apply

## Children's Privacy

TidyFlow does not collect any data from any users, including children.

## Changes to This Policy

We may update this Privacy Policy from time to time. Changes will be noted by updating the "Last Updated" date at the top of this document.

## Your Rights

Since TidyFlow does not collect any personal data, there is no personal data to access, correct, or delete. You can remove all TidyFlow data by:

1. Uninstalling the application
2. Deleting the folder: `C:\ProgramData\TidyFlow\`

## Contact

If you have questions about this Privacy Policy, please:

- Open an issue on GitHub: [https://github.com/ProfessorMoose74/TidyFlow/issues](https://github.com/ProfessorMoose74/TidyFlow/issues)
- Start a discussion: [https://github.com/ProfessorMoose74/TidyFlow/discussions](https://github.com/ProfessorMoose74/TidyFlow/discussions)

## Summary

**TidyFlow respects your privacy by design.** It operates entirely locally, collects no data, and makes no network connections. Your files and settings never leave your computer.
