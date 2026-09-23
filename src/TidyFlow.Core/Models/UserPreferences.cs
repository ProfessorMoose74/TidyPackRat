using System.Text.Json.Serialization;

namespace TidyFlow.Core.Models;

/// <summary>
/// GUI preferences, stored separately from the organization configuration in preferences.json.
/// </summary>
public sealed class UserPreferences
{
    public AppTheme Theme { get; set; } = AppTheme.System;

    /// <summary>Pre-2.0 dark mode flag, migrated to <see cref="Theme"/>. Never written back.</summary>
    [JsonPropertyName("darkMode")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public bool? LegacyDarkMode { get; set; }

    public bool MinimizeToTray { get; set; } = true;

    public bool StartMinimized { get; set; }

    /// <summary>Start TidyFlow in the tray when the user signs in, so the file watcher keeps running.</summary>
    public bool LaunchAtSignIn { get; set; }

    public bool ShowNotifications { get; set; } = true;

    public bool PlaySounds { get; set; } = true;

    public bool EnableFileWatcher { get; set; }

    public double WindowWidth { get; set; } = 1000;

    public double WindowHeight { get; set; } = 760;

    /// <summary>Window position; NaN means "let Windows decide".</summary>
    public double WindowLeft { get; set; } = double.NaN;

    public double WindowTop { get; set; } = double.NaN;

    internal void Migrate()
    {
        if (LegacyDarkMode is bool dark)
        {
            Theme = dark ? AppTheme.Dark : AppTheme.System;
            LegacyDarkMode = null;
        }

        if (WindowWidth < 600 || WindowHeight < 400)
        {
            WindowWidth = 1000;
            WindowHeight = 760;
        }

        // 1.x used -1 for "unset".
        if (WindowLeft < -10000 || WindowLeft == -1)
            WindowLeft = double.NaN;
        if (WindowTop < -10000 || WindowTop == -1)
            WindowTop = double.NaN;
    }
}

[JsonConverter(typeof(TolerantEnumConverter<AppTheme>))]
public enum AppTheme
{
    System,
    Light,
    Dark,
}
