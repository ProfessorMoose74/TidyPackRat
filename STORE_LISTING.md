# Microsoft Store Listing - TidyFlow

This document contains the content needed for Microsoft Store submission.

---

## App Identity

- **App Name:** TidyFlow
- **Publisher:** ProfessorMoose74
- **Category:** Utilities & tools
- **Sub-category:** File managers

---

## Short Description (100 characters max)

```
Automatically organize your Downloads folder. Set it once, stay tidy forever.
```

---

## Description (10,000 characters max)

```
TidyFlow - Sorting your files, to clean up your mess.

Tired of a cluttered Downloads folder? TidyFlow is an automated file organizer that keeps your files tidy without any effort. Set it up once, and let TidyFlow do the rest.

WHAT IT DOES
TidyFlow monitors your Downloads folder (or any folder you choose) and automatically moves files to organized destinations based on their type. Images go to Pictures, documents to Documents, music to Music - you get the idea.

KEY FEATURES

• Automatic File Organization
  Move files automatically based on file extensions. Supports 9 pre-configured categories including Images, Documents, Videos, Audio, Archives, and more.

• Smart Filtering
  - Skip files still being downloaded (age threshold)
  - Skip tiny temp files (size threshold)
  - Exclude patterns like *.tmp, *.part, *.crdownload

• Flexible Scheduling
  Run daily, weekly, or monthly. Choose your preferred time. Optionally run when you log in to Windows.

• Safe Operation
  - Test mode (dry run) to preview changes before committing
  - Automatic configuration backup
  - Never touches system folders
  - Handles duplicates gracefully (rename or skip)

• Comprehensive Logging
  Track every file movement with detailed logs. Know exactly what was moved and when.

• Runs Locally
  No cloud, no account needed, no data collection. TidyFlow runs entirely on your PC and never connects to the internet.

PRE-CONFIGURED CATEGORIES
- Images: .jpg, .png, .gif, .bmp, .svg, .webp, and more
- Documents: .pdf, .docx, .doc, .txt, .rtf, .odt
- Spreadsheets: .xlsx, .xls, .csv, .ods
- Presentations: .pptx, .ppt, .odp
- Archives: .zip, .rar, .7z, .tar, .gz, .iso
- Videos: .mp4, .avi, .mkv, .mov, .wmv, .webm
- Audio: .mp3, .wav, .flac, .m4a, .ogg, .aac
- Executables: .exe, .msi, .bat (disabled by default)
- Code: .py, .js, .html, .css, .java (disabled by default)

GETTING STARTED
1. Launch TidyFlow from the Start Menu
2. Review the default settings (most users won't need to change anything)
3. Click "Test Run" to preview what would be moved
4. Click "Save Configuration" to enable automatic organization
5. That's it! Your files will stay organized automatically.

REQUIREMENTS
- Windows 10 version 1903 or later
- Windows 11 (fully supported)

PRIVACY
TidyFlow does not collect any data. No telemetry, no analytics, no cloud sync. Your files and settings stay on your computer.

OPEN SOURCE
TidyFlow is open source software licensed under the MIT License. View the source code, report issues, or contribute at:
https://github.com/ProfessorMoose74/TidyFlow

Stop wasting time organizing files manually. Let TidyFlow handle it for you!
```

---

## What's New (Release Notes)

```
Version 1.2.0
• NEW: System tray integration - minimize to tray, run from tray menu
• NEW: Dark mode - toggle between light and dark themes
• NEW: Statistics dashboard - track files organized, space saved
• NEW: Real-time file watching - auto-organize files as they appear
• NEW: Undo/Rollback - reverse recent file moves with one click
• NEW: Custom categories - create your own file categories in the GUI
• NEW: Toast notifications - get notified when files are organized
• NEW: Sound effects - optional audio feedback
• NEW: Export/Import settings - share configurations between computers
• IMPROVED: High DPI support for crisp display on all monitors
• IMPROVED: Accessibility features for screen readers
• IMPROVED: Keyboard shortcuts (Alt+T, Alt+R, Alt+V, Alt+S)
```

---

## Keywords/Search Terms

```
file organizer, download organizer, automatic file sorting, file manager, cleanup downloads, organize files, tidy files, file mover, download manager, folder organizer, windows utility, productivity tool, declutter
```

---

## Screenshots Required

Microsoft Store requires screenshots at these sizes:
- **Desktop:** 1366 x 768 pixels (minimum), 3840 x 2160 (maximum)
- **Recommended:** 1920 x 1080 pixels

### Screenshot Suggestions:

1. **Main Configuration Window**
   - Show the full app with default settings
   - Caption: "Simple, intuitive configuration"

2. **File Categories Grid**
   - Focus on the categories section with various types enabled
   - Caption: "9 pre-configured file categories"

3. **Test Run Results**
   - Show a dry run with files that would be moved
   - Caption: "Preview changes before committing"

4. **Scheduling Options**
   - Show scheduling section enabled with daily schedule
   - Caption: "Set it and forget it - automatic scheduling"

5. **Log File View**
   - Show a log file with successful moves
   - Caption: "Comprehensive logging tracks every move"

---

## App Icon Requirements

Microsoft Store requires these icon sizes:
- Store listing: 300 x 300 pixels (PNG)
- App tile: 150 x 150 pixels (PNG)
- Small tile: 71 x 71 pixels (PNG)
- Splash screen: 620 x 300 pixels (PNG, optional)

**Current assets:**
- `assets/logo.png` - Main mascot logo
- `assets/logo.ico` - Application icon
- `assets/title-logo.png` - Title text logo

**TODO:** Generate additional sizes from logo.png for Store submission.

---

## Age Rating

**IARC Rating:** Everyone (E)

Content questionnaire answers:
- Violence: None
- Fear: None
- Sexuality: None
- Crude Humor: None
- Language: None
- Controlled Substance: None
- Gambling: None
- User Interaction: None (app runs locally, no online features)
- Data Collection: None

---

## Privacy Policy URL

```
https://github.com/ProfessorMoose74/TidyFlow/blob/main/PRIVACY.md
```

---

## Support Contact

```
https://github.com/ProfessorMoose74/TidyFlow/issues
```

---

## Website

```
https://github.com/ProfessorMoose74/TidyFlow
```

---

## System Requirements

**Minimum:**
- OS: Windows 10 version 1903 (build 18362)
- Architecture: x86, x64
- Memory: 50 MB
- Storage: 10 MB

**Recommended:**
- OS: Windows 11
- PowerShell 5.1 or later (included in Windows)
- .NET Framework 4.8 (included in Windows 10 1903+)

---

## Pricing

**Free** - Open source software (MIT License)

---

## Notes for Submission

1. **MSIX Packaging Required**
   - Convert current MSI installer to MSIX format
   - Consider using MSIX Packaging Tool or Desktop Bridge

2. **Code Signing**
   - Obtain a code signing certificate (can use Windows Store certificate for Store-only distribution)

3. **Testing**
   - Test on Windows 10 (1903, 21H2, 22H2)
   - Test on Windows 11 (21H2, 22H2, 23H2)
   - Test high DPI scenarios (100%, 125%, 150%, 200% scaling)
   - Test with screen readers (Narrator, NVDA)

4. **Pre-submission Checklist**
   - [ ] All screenshots captured at correct resolution
   - [ ] App icons generated at all required sizes
   - [ ] Privacy policy published and accessible
   - [ ] MSIX package built and signed
   - [ ] Tested on clean Windows installation
   - [ ] Accessibility tested with Narrator
