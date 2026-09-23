namespace TidyFlow.Core.Models;

/// <summary>
/// Running totals shown on the dashboard. Persisted as statistics.json.
/// </summary>
public sealed class Statistics
{
    public long TotalFilesMoved { get; set; }

    public long TotalBytesMoved { get; set; }

    public int TotalRunCount { get; set; }

    public DateTime? FirstRunDate { get; set; }

    public DateTime? LastRunDate { get; set; }

    public int FilesMovedToday { get; set; }

    public long BytesMovedToday { get; set; }

    public DateTime? TodayDate { get; set; }

    /// <summary>Whole days since TidyFlow first moved a file, counting the first day as day 1.</summary>
    public int DaysActive(DateTime now) =>
        FirstRunDate is DateTime first ? Math.Max(1, (int)(now.Date - first.Date).TotalDays + 1) : 0;

    /// <summary>Resets the "today" counters when the date has changed.</summary>
    public void RollOverDay(DateTime now)
    {
        if (TodayDate?.Date != now.Date)
        {
            FilesMovedToday = 0;
            BytesMovedToday = 0;
            TodayDate = now.Date;
        }
    }

    /// <summary>Records a completed run (manual, scheduled, or one watcher batch).</summary>
    public void RecordRun(int filesMoved, long bytesMoved, DateTime now)
    {
        RollOverDay(now);

        TotalRunCount++;
        LastRunDate = now;

        if (filesMoved == 0)
            return;

        TotalFilesMoved += filesMoved;
        TotalBytesMoved += bytesMoved;
        FilesMovedToday += filesMoved;
        BytesMovedToday += bytesMoved;
        FirstRunDate ??= now;
    }

    /// <summary>Removes undone moves from the totals.</summary>
    public void RecordUndo(int filesRestored, long bytesRestored, DateTime movedAt, DateTime now)
    {
        TotalFilesMoved = Math.Max(0, TotalFilesMoved - filesRestored);
        TotalBytesMoved = Math.Max(0, TotalBytesMoved - bytesRestored);

        RollOverDay(now);
        if (movedAt.Date == now.Date)
        {
            FilesMovedToday = Math.Max(0, FilesMovedToday - filesRestored);
            BytesMovedToday = Math.Max(0, BytesMovedToday - bytesRestored);
        }
    }

    /// <summary>Formats a byte count as a human-readable size ("1.5 MB").</summary>
    public static string FormatBytes(long bytes)
    {
        string[] sizes = ["B", "KB", "MB", "GB", "TB"];
        int order = 0;
        double size = bytes;

        while (size >= 1024 && order < sizes.Length - 1)
        {
            order++;
            size /= 1024;
        }

        return $"{size:0.##} {sizes[order]}";
    }
}
