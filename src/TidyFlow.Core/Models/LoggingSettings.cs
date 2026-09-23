using System.Text.Json.Serialization;

namespace TidyFlow.Core.Models;

public sealed class LoggingSettings
{
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// Folder for the monthly log files. Empty means the "logs" folder in TidyFlow's data folder.
    /// Environment variables are expanded.
    /// </summary>
    public string LogPath { get; set; } = string.Empty;

    public LogLevel LogLevel { get; set; } = LogLevel.Info;

    /// <summary>Number of monthly log files to keep.</summary>
    public int MaxLogFiles { get; set; } = 12;
}

[JsonConverter(typeof(TolerantEnumConverter<LogLevel>))]
public enum LogLevel
{
    Info,
    Warn,
    Error,
}
