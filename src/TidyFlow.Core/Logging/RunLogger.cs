using System.Globalization;
using System.Text;
using TidyFlow.Core.Models;
using TidyFlow.Core.Storage;

namespace TidyFlow.Core.Logging;

/// <summary>
/// Appends to a monthly log file (TidyFlow-2026-09.log) and prunes old months.
/// Each write opens and closes the file, so the GUI and a scheduled run can log at the same time.
/// </summary>
public sealed class RunLogger
{
    private readonly object _lock = new();
    private readonly LoggingSettings _settings;
    private readonly TimeProvider _time;
    private bool _pruned;

    /// <param name="settings">Logging settings from the configuration.</param>
    /// <param name="defaultDirectory">Where to log when the settings don't name a folder (the data folder's "logs").</param>
    /// <param name="time">Clock, for tests.</param>
    public RunLogger(LoggingSettings settings, string defaultDirectory, TimeProvider? time = null)
    {
        _settings = settings;
        _time = time ?? TimeProvider.System;
        LogDirectory = string.IsNullOrWhiteSpace(settings.LogPath) ? defaultDirectory : PathHelper.Expand(settings.LogPath);
    }

    /// <summary>A logger that discards everything.</summary>
    public static RunLogger Disabled { get; } = new(new LoggingSettings { Enabled = false }, string.Empty);

    public string LogDirectory { get; }

    public string CurrentLogFile =>
        Path.Combine(LogDirectory, $"TidyFlow-{_time.GetLocalNow():yyyy-MM}.log");

    public void Info(string message) => Write(LogLevel.Info, message);

    public void Warn(string message) => Write(LogLevel.Warn, message);

    public void Error(string message) => Write(LogLevel.Error, message);

    public void Write(LogLevel level, string message)
    {
        if (!_settings.Enabled || level < _settings.LogLevel || LogDirectory.Length == 0)
            return;

        string line = string.Create(CultureInfo.InvariantCulture,
            $"[{_time.GetLocalNow():yyyy-MM-dd HH:mm:ss}] [{level.ToString().ToUpperInvariant(),-5}] {message}{Environment.NewLine}");

        lock (_lock)
        {
            try
            {
                Directory.CreateDirectory(LogDirectory);
                PruneOnce();

                using var stream = new FileStream(CurrentLogFile, FileMode.Append, FileAccess.Write, FileShare.ReadWrite | FileShare.Delete);
                byte[] bytes = Encoding.UTF8.GetBytes(line);
                stream.Write(bytes);
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
            {
                // Logging must never break organizing.
            }
        }
    }

    private void PruneOnce()
    {
        if (_pruned)
            return;
        _pruned = true;

        var old = new DirectoryInfo(LogDirectory)
            .GetFiles("TidyFlow-*.log")
            .OrderByDescending(f => f.Name, StringComparer.Ordinal)
            .Skip(Math.Max(1, _settings.MaxLogFiles));

        foreach (var file in old)
        {
            try { file.Delete(); } catch (IOException) { }
        }
    }
}
