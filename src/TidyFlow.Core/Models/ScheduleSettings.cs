using System.Text.Json.Serialization;

namespace TidyFlow.Core.Models;

/// <summary>
/// When the scheduled task runs TidyFlow unattended.
/// </summary>
public sealed class ScheduleSettings
{
    /// <summary>Run on the calendar schedule below.</summary>
    public bool Enabled { get; set; }

    public ScheduleFrequency Frequency { get; set; } = ScheduleFrequency.Daily;

    /// <summary>Time of day in 24-hour HH:mm format.</summary>
    public string Time { get; set; } = "02:00";

    /// <summary>Day used by the weekly schedule.</summary>
    [JsonConverter(typeof(TolerantEnumConverter<DayOfWeek>))]
    public DayOfWeek DayOfWeek { get; set; } = DayOfWeek.Monday;

    /// <summary>Day used by the monthly schedule: 1-28, or 0 for the last day of the month.</summary>
    public int DayOfMonth { get; set; } = 1;

    /// <summary>Also organize a minute after the user signs in to Windows. Independent of <see cref="Enabled"/>.</summary>
    [JsonPropertyName("runOnStartup")]
    public bool RunAtSignIn { get; set; }

    /// <summary>Parses <see cref="Time"/>; returns false for anything that is not a valid HH:mm value.</summary>
    public static bool TryParseTime(string? value, out TimeOnly time)
    {
        time = new TimeOnly(2, 0);
        if (string.IsNullOrWhiteSpace(value))
            return false;

        return TimeOnly.TryParseExact(value.Trim(), ["H:mm", "HH:mm"], null, System.Globalization.DateTimeStyles.None, out time);
    }
}

[JsonConverter(typeof(TolerantEnumConverter<ScheduleFrequency>))]
public enum ScheduleFrequency
{
    Daily,
    Weekly,
    Monthly,
}
