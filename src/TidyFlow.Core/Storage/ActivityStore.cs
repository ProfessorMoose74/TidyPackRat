using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using TidyFlow.Core.Models;

namespace TidyFlow.Core.Storage;

/// <summary>
/// Statistics and move history. The GUI, the file watcher and scheduled runs (a separate process)
/// all update these files, so every change is a locked read-modify-write against the files on disk.
/// </summary>
public sealed class ActivityStore
{
    private static readonly TimeSpan LockTimeout = TimeSpan.FromSeconds(30);

    private readonly AppPaths _paths;
    private readonly string _mutexName;

    public ActivityStore(AppPaths paths)
    {
        _paths = paths;

        // One lock per data folder, so tests using temporary folders don't contend with each other or a real install.
        byte[] hash = SHA256.HashData(Encoding.UTF8.GetBytes(paths.DataDirectory.ToUpperInvariant()));
        _mutexName = @"Local\TidyFlow.Activity." + Convert.ToHexString(hash, 0, 8);
    }

    /// <summary>A consistent snapshot of both files.</summary>
    public (Statistics Statistics, MoveHistory History) Read() =>
        WithLock(() => (LoadStatistics(), LoadHistory()));

    /// <summary>
    /// Loads both files, applies <paramref name="update"/> and saves them, all while holding the lock.
    /// </summary>
    public T Update<T>(Func<Statistics, MoveHistory, T> update)
    {
        return WithLock(() =>
        {
            var statistics = LoadStatistics();
            var history = LoadHistory();

            T value = update(statistics, history);

            JsonFile.Write(_paths.StatisticsFile, statistics);
            JsonFile.Write(_paths.HistoryFile, history);
            return value;
        });
    }

    /// <summary>Adds a finished batch to the history and statistics. Runs that moved nothing still count as a run.</summary>
    public void RecordBatch(MoveBatch batch, DateTime now) =>
        Update((statistics, history) =>
        {
            statistics.RecordRun(batch.FileCount, batch.TotalBytes, now);
            history.AddBatch(batch);
            return true;
        });

    public void ResetStatistics() =>
        Update((statistics, _) =>
        {
            statistics.TotalFilesMoved = 0;
            statistics.TotalBytesMoved = 0;
            statistics.TotalRunCount = 0;
            statistics.FirstRunDate = null;
            statistics.LastRunDate = null;
            statistics.FilesMovedToday = 0;
            statistics.BytesMovedToday = 0;
            statistics.TodayDate = null;
            return true;
        });

    private Statistics LoadStatistics() => LoadOrDefault<Statistics>(_paths.StatisticsFile);

    private MoveHistory LoadHistory()
    {
        var history = LoadOrDefault<MoveHistory>(_paths.HistoryFile);
        history.Batches ??= [];
        history.Batches.RemoveAll(b => b is null);
        foreach (var batch in history.Batches)
            batch.Moves ??= [];
        return history;
    }

    private static T LoadOrDefault<T>(string path) where T : class, new()
    {
        try
        {
            return JsonFile.Read<T>(path) ?? new T();
        }
        catch (JsonException)
        {
            // A corrupt activity file only costs history and totals; start again rather than failing every run.
            TryPreserveCorruptFile(path);
            return new T();
        }
    }

    private static void TryPreserveCorruptFile(string path)
    {
        try
        {
            File.Copy(path, path + ".corrupt", overwrite: true);
        }
        catch (IOException)
        {
        }
    }

    private T WithLock<T>(Func<T> action)
    {
        using var mutex = new Mutex(initiallyOwned: false, _mutexName);
        bool acquired;
        try
        {
            acquired = mutex.WaitOne(LockTimeout);
        }
        catch (AbandonedMutexException)
        {
            // Another TidyFlow process died while holding the lock; the files are still readable.
            acquired = true;
        }

        if (!acquired)
            throw new TimeoutException("Timed out waiting for another TidyFlow process to finish saving its activity.");

        try
        {
            return action();
        }
        finally
        {
            mutex.ReleaseMutex();
        }
    }
}
