using TidyFlow.Core.Logging;
using TidyFlow.Core.Models;
using TidyFlow.Core.Storage;

namespace TidyFlow.Core.Organizing;

/// <summary>Summary of one complete run.</summary>
public sealed record RunSummary(
    RunTrigger Trigger,
    int FilesScanned,
    int FilesMoved,
    long BytesMoved,
    int FilesSkipped,
    IReadOnlyList<MoveFailure> Failures,
    MoveBatch Batch)
{
    public bool HasFailures => Failures.Count > 0;

    public string Describe()
    {
        if (FilesMoved == 0 && !HasFailures)
            return "Nothing to organize. Your folder is already tidy.";

        string text = $"Moved {FilesMoved} {(FilesMoved == 1 ? "file" : "files")} ({Statistics.FormatBytes(BytesMoved)}).";
        if (HasFailures)
            text += $" {Failures.Count} couldn't be moved; see the log for details.";
        return text;
    }
}

/// <summary>
/// Runs the organizer end to end: execute, log, and record the batch in history and statistics.
/// Shared by the GUI's Run Now, the headless scheduled run and the file watcher.
/// </summary>
public sealed class OrganizeRunner(AppConfiguration config, ActivityStore activity, RunLogger logger, TimeProvider? time = null)
{
    private readonly TimeProvider _time = time ?? TimeProvider.System;

    public Organizer Organizer { get; } = new(config, time);

    /// <summary>Plans and executes a full pass over the source folder.</summary>
    public RunSummary Run(RunTrigger trigger, CancellationToken cancellationToken = default)
    {
        logger.Info($"{trigger} run started. Source: {Organizer.SourceFolder}");

        IReadOnlyList<FileDecision> plan;
        try
        {
            plan = Organizer.Plan();
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            logger.Error($"Run aborted: {ex.Message}");
            throw;
        }

        return Execute(plan, trigger, cancellationToken);
    }

    /// <summary>
    /// Executes decisions that were already made (a previewed plan, or files collected by the watcher).
    /// </summary>
    public RunSummary Execute(IReadOnlyList<FileDecision> decisions, RunTrigger trigger, CancellationToken cancellationToken = default)
    {
        var batch = new MoveBatch { Trigger = trigger, StartTime = _time.GetLocalNow().DateTime };

        var result = Organizer.Execute(decisions, cancellationToken);

        foreach (var move in result.Moved)
            logger.Info($"MOVED [{move.Category}] {move.FileName} -> {move.DestinationPath}");
        foreach (var skip in result.Skipped.Where(s => s.SkipReason is not SkipReason.NoCategory))
            logger.Info($"SKIPPED {skip.FileName} ({skip.SkipReasonText})");
        foreach (var failure in result.Failed)
            logger.Error($"FAILED {failure.Decision.FileName}: {failure.Error}");

        batch.Moves.AddRange(result.Moved);
        batch.EndTime = _time.GetLocalNow().DateTime;

        // The watcher calls this for every settled file; only record watcher passes that did something.
        if (trigger != RunTrigger.Watcher || result.Moved.Count > 0)
            activity.RecordBatch(batch, batch.EndTime);

        var summary = new RunSummary(
            trigger,
            FilesScanned: decisions.Count,
            FilesMoved: result.Moved.Count,
            BytesMoved: result.BytesMoved,
            FilesSkipped: result.Skipped.Count,
            Failures: result.Failed,
            Batch: batch);

        if (trigger != RunTrigger.Watcher || result.Moved.Count > 0 || result.Failed.Count > 0)
        {
            logger.Info($"{trigger} run finished: {summary.FilesMoved} moved, {summary.FilesSkipped} skipped, "
                + $"{summary.Failures.Count} failed{(result.Cancelled ? " (cancelled)" : string.Empty)}.");
        }

        return summary;
    }

    /// <summary>Undoes a batch from history and updates statistics to match.</summary>
    public static UndoResult Undo(string batchId, ActivityStore activity, RunLogger logger, TimeProvider? time = null)
    {
        var now = (time ?? TimeProvider.System).GetLocalNow().DateTime;

        var result = activity.Update((statistics, history) =>
        {
            var batch = history.Find(batchId);
            if (batch is null)
                return new UndoResult();

            var undo = Organizer.Undo(batch);
            statistics.RecordUndo(undo.Restored.Count, undo.Restored.Sum(m => m.FileSize), batch.StartTime, now);
            return undo;
        });

        foreach (var move in result.Restored)
            logger.Info($"UNDO restored {move.FileName} -> {Path.GetDirectoryName(move.SourcePath)}");
        foreach (var move in result.Missing)
            logger.Warn($"UNDO skipped {move.FileName}: no longer at {move.DestinationPath}");
        foreach (var (move, error) in result.Failed)
            logger.Error($"UNDO failed for {move.FileName}: {error}");

        return result;
    }
}
