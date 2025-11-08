# Contributing to TidyPackRat

First off, thank you for considering contributing to TidyPackRat! It's people like you that make TidyPackRat such a great tool.

## Code of Conduct

This project and everyone participating in it is governed by a code of conduct. By participating, you are expected to uphold this code. Please be respectful, inclusive, and constructive in all interactions.

## How Can I Contribute?

### Reporting Bugs

Before creating bug reports, please check the existing issues to avoid duplicates. When you create a bug report, include as many details as possible:

- **Use a clear and descriptive title**
- **Describe the exact steps to reproduce the problem**
- **Provide specific examples** to demonstrate the steps
- **Describe the behavior you observed** and what you expected
- **Include screenshots** if relevant
- **Include your environment details**:
  - Windows version
  - PowerShell version
  - TidyPackRat version
  - Relevant log files from `C:\ProgramData\TidyPackRat\logs`

### Suggesting Enhancements

Enhancement suggestions are tracked as GitHub issues. When creating an enhancement suggestion:

- **Use a clear and descriptive title**
- **Provide a detailed description** of the suggested enhancement
- **Explain why this enhancement would be useful**
- **List any similar features** in other applications if applicable

### Pull Requests

1. **Fork the repository** and create your branch from `main`
2. **Follow the coding standards** (see below)
3. **Test your changes** thoroughly
4. **Update documentation** as needed
5. **Ensure your code compiles** without warnings
6. **Commit with clear messages** (see commit message guidelines)
7. **Submit a pull request** with a comprehensive description

## Development Setup

### Prerequisites

- Visual Studio 2017 or newer
- .NET Framework 4.8 SDK
- WiX Toolset v3.11+ (for installer development)
- Git for Windows
- PowerShell 5.1+

### Getting Started

1. Clone your fork:
   ```bash
   git clone https://github.com/YOUR-USERNAME/TidyPackRat.git
   cd TidyPackRat
   ```

2. Open the solution in Visual Studio:
   ```bash
   start src\gui\TidyPackRat.sln
   ```

3. Build the solution (Ctrl+Shift+B)

4. Test your changes

### Testing

#### Testing the PowerShell Worker

```powershell
# Dry run test
.\src\worker\TidyPackRat-Worker.ps1 -ConfigPath "config\default-config.json" -DryRun -VerboseLogging

# Create test files
mkdir TestSource
echo "test" > TestSource\test.txt
echo "test" > TestSource\test.jpg

# Run the worker
.\src\worker\TidyPackRat-Worker.ps1 -ConfigPath "config\default-config.json"
```

#### Testing the GUI

1. Set `src\gui` as the startup project
2. Press F5 to run in debug mode
3. Test all configuration options
4. Verify "Test Run" and "Run Now" buttons work correctly

## Coding Standards

### C# (GUI Application)

- Follow Microsoft C# coding conventions
- Use meaningful variable and method names
- Add XML documentation comments for public members
- Keep methods focused and single-purpose
- Use `async/await` for I/O operations where appropriate
- Handle exceptions gracefully with user-friendly messages

Example:
```csharp
/// <summary>
/// Loads configuration from the specified file path
/// </summary>
/// <param name="filePath">Path to the configuration file</param>
/// <returns>Loaded configuration object</returns>
public static AppConfiguration LoadConfiguration(string filePath = null)
{
    // Implementation
}
```

### PowerShell (Worker Script)

- Use approved PowerShell verbs (Get, Set, New, Remove, etc.)
- Add comment-based help for functions
- Use proper error handling with try/catch
- Write verbose logging statements
- Follow PowerShell naming conventions (Pascal case for functions)
- Use strict mode: `Set-StrictMode -Version Latest`

Example:
```powershell
<#
.SYNOPSIS
    Moves a file to its destination folder

.PARAMETER File
    The file to move

.PARAMETER DestinationPath
    The destination folder path
#>
function Move-FileToDestination {
    param(
        [System.IO.FileInfo]$File,
        [string]$DestinationPath
    )
    # Implementation
}
```

### WiX (Installer)

- Use descriptive component IDs
- Include comments for complex sections
- Follow WiX best practices for upgrades
- Test installer on clean Windows installations

## Commit Message Guidelines

We follow the [Conventional Commits](https://www.conventionalcommits.org/) specification:

```
<type>(<scope>): <subject>

<body>

<footer>
```

### Types

- `feat`: New feature
- `fix`: Bug fix
- `docs`: Documentation changes
- `style`: Code style changes (formatting, etc.)
- `refactor`: Code refactoring
- `test`: Adding tests
- `chore`: Maintenance tasks

### Examples

```
feat(gui): add custom category creation dialog

Users can now create custom file categories with their own
extensions and destination folders.

Closes #123
```

```
fix(worker): handle files with special characters in names

Files with characters like brackets, parentheses, and ampersands
are now properly handled during file operations.

Fixes #456
```

## Documentation

When adding new features or making changes:

- Update the README.md if user-facing changes
- Update relevant documentation in `docs/`
- Add inline code comments for complex logic
- Update XML documentation for public APIs

## Project Structure

```
TidyPackRat/
├── src/
│   ├── gui/              # WPF configuration application
│   │   ├── Models/       # Data models
│   │   ├── Helpers/      # Utility classes
│   │   └── *.xaml        # UI files
│   ├── worker/           # PowerShell worker script
│   └── installer/        # WiX installer project
├── config/               # Default configuration
├── docs/                 # Documentation
├── tests/                # Test files and scripts
└── assets/               # Images, icons, etc.
```

## Areas for Contribution

We especially welcome contributions in these areas:

### High Priority
- Unit tests for C# code
- Integration tests for PowerShell scripts
- Performance optimizations
- Accessibility improvements in GUI
- Additional file category presets
- Internationalization/localization

### Medium Priority
- Custom category editor in GUI
- Advanced filtering options
- Statistics and reporting
- File preview functionality
- Undo/rollback system

### Future Phases
- Linux version (bash/Python)
- macOS version
- Cloud storage integration
- Network share support

## Getting Help

If you need help with development:

1. Check existing [GitHub Discussions](https://github.com/ProfessorMoose74/TidyPackRat/discussions)
2. Ask in a new discussion thread
3. Reference relevant documentation
4. Provide context about what you're trying to achieve

## Recognition

Contributors will be recognized in:
- The project README
- Release notes
- GitHub contributors page

## License

By contributing to TidyPackRat, you agree that your contributions will be licensed under the MIT License.

---

Thank you for contributing to TidyPackRat! Your efforts help make file organization easier for everyone.
