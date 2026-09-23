using System.Text.Json;
using TidyFlow.Core.Models;

namespace TidyFlow.Core.Storage;

/// <summary>Raised when config.json exists but cannot be read, and no usable backup exists.</summary>
public sealed class ConfigurationException(string message, Exception? inner = null) : Exception(message, inner);

/// <summary>
/// Loads, migrates, validates and saves config.json.
/// </summary>
public sealed class ConfigurationStore(AppPaths paths)
{
    private const string DefaultConfigResource = "TidyFlow.Core.default-config.json";

    public AppPaths Paths { get; } = paths;

    /// <summary>
    /// Loads the configuration. Returns the defaults when no file exists yet, falls back to the
    /// backup when the main file is corrupt, and throws <see cref="ConfigurationException"/> only
    /// when neither can be read.
    /// </summary>
    public AppConfiguration Load()
    {
        if (!File.Exists(Paths.ConfigFile))
            return CreateDefault();

        try
        {
            return Normalize(JsonFile.Read<AppConfiguration>(Paths.ConfigFile) ?? CreateDefault());
        }
        catch (Exception ex) when (ex is JsonException or IOException or UnauthorizedAccessException)
        {
            if (TryLoadBackup() is AppConfiguration backup)
                return backup;

            throw new ConfigurationException($"Could not read {Paths.ConfigFile}: {ex.Message}", ex);
        }
    }

    /// <summary>Validates and saves the configuration, keeping the previous file as config.json.backup.</summary>
    public void Save(AppConfiguration config)
    {
        Normalize(config);

        var errors = Validate(config);
        if (errors.Count > 0)
            throw new ConfigurationException(string.Join(Environment.NewLine, errors));

        if (File.Exists(Paths.ConfigFile))
        {
            try
            {
                File.Copy(Paths.ConfigFile, Paths.ConfigBackupFile, overwrite: true);
            }
            catch (IOException)
            {
                // A missing backup is not worth failing the save over.
            }
        }

        JsonFile.Write(Paths.ConfigFile, config);
    }

    /// <summary>A fresh copy of the built-in default configuration.</summary>
    public static AppConfiguration CreateDefault()
    {
        using var stream = typeof(ConfigurationStore).Assembly.GetManifestResourceStream(DefaultConfigResource)
            ?? throw new InvalidOperationException($"Missing embedded resource {DefaultConfigResource}.");

        var config = JsonSerializer.Deserialize<AppConfiguration>(stream, JsonFile.Options)
            ?? throw new InvalidOperationException("The embedded default configuration is empty.");

        return Normalize(config);
    }

    /// <summary>Returns a list of human-readable problems; empty when the configuration is usable.</summary>
    public static List<string> Validate(AppConfiguration config)
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(config.SourceFolder))
            errors.Add("Choose a source folder to organize.");
        else if (PathHelper.IsProtectedFolder(config.SourceFolder))
            errors.Add($"TidyFlow can't organize {PathHelper.Expand(config.SourceFolder)}. Choose a regular folder such as Downloads.");

        if (config.FileAgeThresholdHours < 0)
            errors.Add("The file age threshold can't be negative.");

        if (config.MinFileSizeKB < 0)
            errors.Add("The minimum file size can't be negative.");

        if (!ScheduleSettings.TryParseTime(config.Schedule.Time, out _))
            errors.Add($"'{config.Schedule.Time}' isn't a valid time. Use 24-hour HH:mm, for example 02:00.");

        foreach (var category in config.Categories.Where(c => c.Enabled))
        {
            if (string.IsNullOrWhiteSpace(category.Name))
                errors.Add("Every category needs a name.");
            if (string.IsNullOrWhiteSpace(category.Destination))
                errors.Add($"Category '{category.Name}' needs a destination folder.");
            else if (PathHelper.AreSameFolder(category.Destination, config.SourceFolder))
                errors.Add($"Category '{category.Name}' moves files into the source folder itself.");
            if (category.Extensions.Count == 0)
                errors.Add($"Category '{category.Name}' has no file extensions.");
        }

        return errors;
    }

    /// <summary>
    /// Fills in anything missing, migrates 1.x fields and cleans up user-entered values.
    /// Safe to call repeatedly.
    /// </summary>
    internal static AppConfiguration Normalize(AppConfiguration config)
    {
        config.Categories ??= [];
        config.ExcludePatterns ??= [];
        config.Schedule ??= new ScheduleSettings();
        config.Logging ??= new LoggingSettings();

        // 1.x: the GUI labelled this field KB and stored what the user typed, so treat it as KB.
        if (config.LegacyFileSizeThreshold is long legacySize)
        {
            if (config.MinFileSizeKB == 0)
                config.MinFileSizeKB = Math.Max(0, legacySize);
            config.LegacyFileSizeThreshold = null;
        }

        config.SourceFolder = config.SourceFolder?.Trim() ?? string.Empty;
        config.FileAgeThresholdHours = Math.Max(0, config.FileAgeThresholdHours);
        config.MinFileSizeKB = Math.Max(0, config.MinFileSizeKB);

        config.Categories.RemoveAll(c => c is null);
        foreach (var category in config.Categories)
        {
            category.Name = category.Name?.Trim() ?? string.Empty;
            category.Destination = category.Destination?.Trim() ?? string.Empty;
            category.Extensions = (category.Extensions ?? [])
                .Where(e => !string.IsNullOrWhiteSpace(e))
                .Select(FileCategory.NormalizeExtension)
                .Where(e => e.Length > 1)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();
        }

        config.ExcludePatterns = config.ExcludePatterns
            .Select(p => p?.Trim() ?? string.Empty)
            .Where(p => p.Length > 0)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        var schedule = config.Schedule;
        if (ScheduleSettings.TryParseTime(schedule.Time, out var time))
            schedule.Time = time.ToString("HH:mm", System.Globalization.CultureInfo.InvariantCulture);
        schedule.DayOfMonth = Math.Clamp(schedule.DayOfMonth, 0, 28);

        var logging = config.Logging;
        logging.LogPath = logging.LogPath?.Trim() ?? string.Empty;
        // 1.x defaulted to ProgramData, which MSIX virtualizes and the GUI never read back.
        if (logging.LogPath.Contains("%PROGRAMDATA%", StringComparison.OrdinalIgnoreCase))
            logging.LogPath = string.Empty;
        logging.MaxLogFiles = Math.Clamp(logging.MaxLogFiles, 1, 120);

        config.SchemaVersion = AppConfiguration.CurrentSchemaVersion;
        return config;
    }

    private AppConfiguration? TryLoadBackup()
    {
        try
        {
            var backup = JsonFile.Read<AppConfiguration>(Paths.ConfigBackupFile);
            return backup is null ? null : Normalize(backup);
        }
        catch (Exception ex) when (ex is JsonException or IOException or UnauthorizedAccessException)
        {
            return null;
        }
    }
}
