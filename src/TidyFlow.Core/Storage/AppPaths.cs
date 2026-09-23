namespace TidyFlow.Core.Storage;

/// <summary>
/// Locations of TidyFlow's data files. Everything lives under one folder so tests can point it elsewhere.
/// </summary>
/// <remarks>
/// For the MSIX build, writes to %LOCALAPPDATA% are redirected into the package's private storage.
/// That is fine now that scheduled runs launch the packaged app itself (via its execution alias)
/// and therefore see the same redirected folder.
/// </remarks>
public sealed class AppPaths
{
    public AppPaths(string dataDirectory)
    {
        DataDirectory = Path.GetFullPath(dataDirectory);
    }

    /// <summary>Set this environment variable to keep TidyFlow's data somewhere else (development, portable use).</summary>
    public const string DataDirectoryVariable = "TIDYFLOW_DATA_DIR";

    /// <summary>%LOCALAPPDATA%\TidyFlow, unless overridden by <see cref="DataDirectoryVariable"/>.</summary>
    public static AppPaths Default { get; } = new(
        Environment.GetEnvironmentVariable(DataDirectoryVariable) is { Length: > 0 } overridden
            ? overridden
            : Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "TidyFlow"));

    public string DataDirectory { get; }

    public string ConfigFile => Path.Combine(DataDirectory, "config.json");

    public string ConfigBackupFile => ConfigFile + ".backup";

    public string PreferencesFile => Path.Combine(DataDirectory, "preferences.json");

    public string StatisticsFile => Path.Combine(DataDirectory, "statistics.json");

    public string HistoryFile => Path.Combine(DataDirectory, "history.json");

    public string DefaultLogDirectory => Path.Combine(DataDirectory, "logs");

    /// <summary>Files left behind by 1.x, which copied a PowerShell worker here for the scheduled task.</summary>
    public IEnumerable<string> LegacyFiles =>
    [
        Path.Combine(DataDirectory, "TidyFlow-Worker.ps1"),
        Path.Combine(DataDirectory, "worker-deployment.json"),
    ];
}
