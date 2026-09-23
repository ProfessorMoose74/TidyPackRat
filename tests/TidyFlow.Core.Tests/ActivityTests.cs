using TidyFlow.Core.Logging;
using TidyFlow.Core.Models;
using TidyFlow.Core.Organizing;
using TidyFlow.Core.Storage;

namespace TidyFlow.Core.Tests;

public sealed class ActivityTests : IDisposable
{
    private readonly TempWorkspace _ws = new();
    private readonly TestClock _clock = new();

    public void Dispose() => _ws.Dispose();

    [Fact]
    public void Scheduled_runs_record_history_and_statistics()
    {
        // 1.x bug: only file-watcher moves reached history, so scheduled runs couldn't be undone.
        _ws.CreateFile("report.pdf", sizeBytes: 2048);
        _ws.CreateFile("photo.jpg", sizeBytes: 1024);
        var activity = new ActivityStore(_ws.Paths);

        var summary = new OrganizeRunner(_ws.Config(), activity, RunLogger.Disabled, _clock).Run(RunTrigger.Scheduled);
        var (statistics, history) = activity.Read();

        Assert.Equal(2, summary.FilesMoved);
        Assert.Equal(2, statistics.TotalFilesMoved);
        Assert.Equal(3072, statistics.TotalBytesMoved);
        Assert.Equal(2, statistics.FilesMovedToday);
        Assert.Equal(1, statistics.TotalRunCount);
        var batch = Assert.Single(history.Batches);
        Assert.Equal(RunTrigger.Scheduled, batch.Trigger);
        Assert.True(batch.CanUndo);
    }

    [Fact]
    public void Undo_restores_files_and_corrects_statistics()
    {
        _ws.CreateFile("report.pdf", sizeBytes: 2048);
        var activity = new ActivityStore(_ws.Paths);
        var summary = new OrganizeRunner(_ws.Config(), activity, RunLogger.Disabled, _clock).Run(RunTrigger.Manual);

        var undo = OrganizeRunner.Undo(summary.Batch.BatchId, activity, RunLogger.Disabled, _clock);
        var (statistics, history) = activity.Read();

        Assert.Single(undo.Restored);
        Assert.True(File.Exists(Path.Combine(_ws.Source, "report.pdf")));
        Assert.Equal(0, statistics.TotalFilesMoved);
        Assert.Equal(0, statistics.FilesMovedToday);
        Assert.True(history.Batches[0].WasUndone);
        Assert.Null(history.GetLastUndoableBatch());
    }

    [Fact]
    public void Watcher_passes_that_move_nothing_are_not_recorded()
    {
        var activity = new ActivityStore(_ws.Paths);
        var runner = new OrganizeRunner(_ws.Config(), activity, RunLogger.Disabled, _clock);

        runner.Execute([], RunTrigger.Watcher);

        Assert.Equal(0, activity.Read().Statistics.TotalRunCount);
    }

    [Fact]
    public void Concurrent_updates_are_never_lost()
    {
        var activity = new ActivityStore(_ws.Paths);
        var otherProcess = new ActivityStore(new AppPaths(_ws.Data));

        Parallel.For(0, 40, i =>
        {
            var store = i % 2 == 0 ? activity : otherProcess;
            var batch = new MoveBatch();
            batch.Moves.Add(new MoveRecord { FileName = $"f{i}", FileSize = 10 });
            store.RecordBatch(batch, TestClock.Now);
        });

        var (statistics, history) = activity.Read();
        Assert.Equal(40, statistics.TotalFilesMoved);
        Assert.Equal(40, history.Batches.Count);
    }

    [Fact]
    public void Reads_1x_history_and_statistics_files()
    {
        File.WriteAllText(_ws.Paths.HistoryFile, """
            {
              "batches": [
                { "batchId": "ab12cd34", "startTime": "2026-01-05T09:00:00", "endTime": "2026-01-05T09:00:01",
                  "moves": [ { "sourcePath": "C:\\a.pdf", "destinationPath": "C:\\D\\a.pdf", "fileName": "a.pdf",
                               "fileSize": 10, "category": "Documents", "movedAt": "2026-01-05T09:00:00", "canUndo": true } ],
                  "wasUndone": false }
              ],
              "maxBatches": 50
            }
            """);
        File.WriteAllText(_ws.Paths.StatisticsFile, """{ "totalFilesMoved": 120, "totalBytesMoved": 99999, "totalRunCount": 9 }""");

        var (statistics, history) = new ActivityStore(_ws.Paths).Read();

        Assert.Equal(120, statistics.TotalFilesMoved);
        Assert.Equal("ab12cd34", history.GetLastUndoableBatch()!.BatchId);
        Assert.Equal(RunTrigger.Manual, history.Batches[0].Trigger);
    }

    [Fact]
    public void A_corrupt_history_file_is_set_aside_rather_than_breaking_runs()
    {
        File.WriteAllText(_ws.Paths.HistoryFile, "{ broken");

        var (_, history) = new ActivityStore(_ws.Paths).Read();

        Assert.Empty(history.Batches);
        Assert.True(File.Exists(_ws.Paths.HistoryFile + ".corrupt"));
    }

    [Fact]
    public void History_is_capped()
    {
        var history = new MoveHistory { MaxBatches = 3 };
        for (int i = 0; i < 5; i++)
        {
            var batch = new MoveBatch { BatchId = i.ToString(System.Globalization.CultureInfo.InvariantCulture) };
            batch.Moves.Add(new MoveRecord());
            history.AddBatch(batch);
        }

        Assert.Equal(["4", "3", "2"], history.Batches.Select(b => b.BatchId));
    }

    [Fact]
    public void Statistics_roll_over_at_midnight()
    {
        var statistics = new Statistics();
        statistics.RecordRun(5, 500, TestClock.Now);
        statistics.RollOverDay(TestClock.Now.AddDays(1));

        Assert.Equal(0, statistics.FilesMovedToday);
        Assert.Equal(5, statistics.TotalFilesMoved);
        Assert.Equal(2, statistics.DaysActive(TestClock.Now.AddDays(1)));
    }

    [Fact]
    public void Logger_defaults_to_the_data_folder()
    {
        string defaultDir = Path.Combine(_ws.Data, "logs");
        var logger = new RunLogger(new LoggingSettings(), defaultDir, _clock);

        logger.Info("hello");

        Assert.True(File.Exists(Path.Combine(defaultDir, "TidyFlow-2026-09.log")));
    }

    [Fact]
    public void Logger_writes_monthly_files_and_honours_the_level()
    {
        string logDir = Path.Combine(_ws.Root, "logs");
        var logger = new RunLogger(new LoggingSettings { LogPath = logDir, LogLevel = LogLevel.Warn }, Path.Combine(_ws.Root, "unused"), _clock);

        logger.Info("hidden");
        logger.Warn("shown");

        string text = File.ReadAllText(Path.Combine(logDir, "TidyFlow-2026-09.log"));
        Assert.DoesNotContain("hidden", text);
        Assert.Contains("[WARN ] shown", text);
    }
}
