using TidyFlow.Core.Models;

namespace TidyFlow.ViewModels;

/// <summary>A past run, shown in the History list.</summary>
public sealed class HistoryItemViewModel(MoveBatch batch)
{
    public string BatchId { get; } = batch.BatchId;

    public DateTime When { get; } = batch.StartTime;

    public string WhenText => Formatting.RelativeTime(When);

    public string TriggerText { get; } = batch.Trigger switch
    {
        RunTrigger.Scheduled => "Scheduled run",
        RunTrigger.Watcher => "Watched folder",
        _ => "Manual run",
    };

    public int FileCount { get; } = batch.FileCount;

    public string Summary { get; } =
        $"{batch.FileCount} {(batch.FileCount == 1 ? "file" : "files")}, {Statistics.FormatBytes(batch.TotalBytes)}";

    public string Details { get; } = string.Join(Environment.NewLine,
        batch.Moves.Take(15).Select(m => $"{m.FileName}  →  {m.Category}")
            .Concat(batch.FileCount > 15 ? [$"…and {batch.FileCount - 15} more"] : []));

    public bool CanUndo { get; } = batch.CanUndo;

    public string StateText => batch.WasUndone ? "Undone" : batch.CanUndo ? string.Empty : "Can't undo";
}

/// <summary>One moved file, shown in the dashboard's recent activity list.</summary>
public sealed record ActivityItemViewModel(string FileName, string Category, DateTime MovedAt)
{
    public string WhenText => Formatting.RelativeTime(MovedAt);
}

internal static class Formatting
{
    public static string RelativeTime(DateTime when)
    {
        var now = DateTime.Now;
        if (when.Date == now.Date)
            return $"Today {when:t}";
        if (when.Date == now.Date.AddDays(-1))
            return $"Yesterday {when:t}";
        if (when.Date == now.Date.AddDays(1))
            return $"Tomorrow {when:t}";
        if ((now - when).Duration() < TimeSpan.FromDays(7))
            return $"{when:dddd} {when:t}";
        return when.ToString("g", System.Globalization.CultureInfo.CurrentCulture);
    }
}
