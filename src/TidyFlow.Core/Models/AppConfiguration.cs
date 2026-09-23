using System.Text.Json.Serialization;

namespace TidyFlow.Core.Models;

/// <summary>
/// The organization configuration shared by the GUI, the file watcher and headless runs.
/// Persisted as config.json in the TidyFlow data folder.
/// </summary>
public sealed class AppConfiguration
{
    /// <summary>Schema version written by this build. Older files are migrated on load.</summary>
    public const int CurrentSchemaVersion = 2;

    public int SchemaVersion { get; set; } = CurrentSchemaVersion;

    /// <summary>Folder to organize. Environment variables such as %USERPROFILE% are expanded.</summary>
    public string SourceFolder { get; set; } = @"%USERPROFILE%\Downloads";

    /// <summary>Files modified more recently than this many hours are left alone (scheduled and manual runs only).</summary>
    [JsonPropertyName("fileAgeThreshold")]
    public int FileAgeThresholdHours { get; set; } = 24;

    /// <summary>Files smaller than this many kilobytes are left alone. 0 disables the filter.</summary>
    public long MinFileSizeKB { get; set; }

    /// <summary>
    /// Pre-2.0 name for <see cref="MinFileSizeKB"/>. The 1.x GUI stored the value the user typed in KB,
    /// so it is migrated as KB. Never written back.
    /// </summary>
    [JsonPropertyName("fileSizeThreshold")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public long? LegacyFileSizeThreshold { get; set; }

    public DuplicateHandling DuplicateHandling { get; set; } = DuplicateHandling.Rename;

    /// <summary>Leave hidden and system files (desktop.ini, thumbs.db, ...) where they are.</summary>
    public bool SkipHiddenFiles { get; set; } = true;

    public List<FileCategory> Categories { get; set; } = [];

    public ScheduleSettings Schedule { get; set; } = new();

    /// <summary>Wildcard patterns (<c>*</c> and <c>?</c>) matched against file names, case-insensitively.</summary>
    public List<string> ExcludePatterns { get; set; } = [];

    public LoggingSettings Logging { get; set; } = new();
}

/// <summary>What to do when a file with the same name already exists at the destination.</summary>
[JsonConverter(typeof(TolerantEnumConverter<DuplicateHandling>))]
public enum DuplicateHandling
{
    /// <summary>Move anyway, adding a numeric suffix (report_1.pdf).</summary>
    Rename,

    /// <summary>Leave the file in the source folder.</summary>
    Skip,
}
