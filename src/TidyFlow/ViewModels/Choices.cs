using TidyFlow.Core.Models;

namespace TidyFlow.ViewModels;

/// <summary>A value with a friendly label, for combo boxes.</summary>
public sealed record Choice<T>(T Value, string Label)
{
    public override string ToString() => Label;
}

public static class Choices
{
    public static IReadOnlyList<Choice<ScheduleFrequency>> Frequencies { get; } =
    [
        new(ScheduleFrequency.Daily, "Every day"),
        new(ScheduleFrequency.Weekly, "Every week"),
        new(ScheduleFrequency.Monthly, "Every month"),
    ];

    public static IReadOnlyList<Choice<DayOfWeek>> DaysOfWeek { get; } =
        Enum.GetValues<DayOfWeek>()
            .OrderBy(d => ((int)d + 6) % 7) // Monday first
            .Select(d => new Choice<DayOfWeek>(d, System.Globalization.CultureInfo.CurrentCulture.DateTimeFormat.GetDayName(d)))
            .ToList();

    /// <summary>1-28 plus "last day"; 29-31 would silently skip short months.</summary>
    public static IReadOnlyList<Choice<int>> DaysOfMonth { get; } =
        Enumerable.Range(1, 28).Select(d => new Choice<int>(d, Ordinal(d)))
            .Append(new Choice<int>(0, "Last day"))
            .ToList();

    public static IReadOnlyList<Choice<DuplicateHandling>> DuplicateStrategies { get; } =
    [
        new(DuplicateHandling.Rename, "Move it and add a number (report_1.pdf)"),
        new(DuplicateHandling.Skip, "Leave it where it is"),
    ];

    public static IReadOnlyList<Choice<AppTheme>> Themes { get; } =
    [
        new(AppTheme.System, "Use my Windows setting"),
        new(AppTheme.Light, "Light"),
        new(AppTheme.Dark, "Dark"),
    ];

    private static string Ordinal(int n) => n switch
    {
        1 or 21 => $"{n}st",
        2 or 22 => $"{n}nd",
        3 or 23 => $"{n}rd",
        _ => $"{n}th",
    };
}
