using TidyFlow.Core.Logging;
using TidyFlow.Core.Models;
using TidyFlow.Core.Organizing;
using TidyFlow.Core.Storage;

namespace TidyFlow.Services;

/// <summary>
/// Organizes new files in the source folder as they arrive, using the same rules as a normal run except the
/// age threshold. A file is handled once it has been quiet for <see cref="SettleTime"/> and can be opened
/// exclusively (so half-finished downloads are left alone). Everything handled in one pass is recorded as a
/// single batch, so Undo reverses the whole group.
/// </summary>
public sealed class FileWatcherService : IDisposable
{
    /// <summary>How long a file must go without changes before it's organized.</summary>
    public static readonly TimeSpan SettleTime = TimeSpan.FromSeconds(15);

    private static readonly TimeSpan PollInterval = TimeSpan.FromSeconds(5);

    /// <summary>Give up on a file that stays locked this long (a download paused in the browser, for example).</summary>
    private static readonly TimeSpan MaxWaitForLock = TimeSpan.FromMinutes(30);

    private readonly object _lock = new();
    private readonly Dictionary<string, PendingFile> _pending = new(StringComparer.OrdinalIgnoreCase);
    private readonly ActivityStore _activity;

    private FileSystemWatcher? _watcher;
    private Timer? _timer;
    private OrganizeRunner? _runner;
    private RunLogger _logger = RunLogger.Disabled;
    private int _processing;

    public FileWatcherService(ActivityStore activity)
    {
        _activity = activity;
    }

    /// <summary>Raised on a background thread after a pass moved or failed to move something.</summary>
    public event EventHandler<RunSummary>? FilesOrganized;

    /// <summary>Raised on a background thread when watching stops unexpectedly or can't start.</summary>
    public event EventHandler<string>? Error;

    public bool IsRunning => _watcher is not null;

    /// <summary>Starts (or restarts, with new settings) watching the configured source folder.</summary>
    public void Start(AppConfiguration config, RunLogger logger)
    {
        Stop();

        var runner = new OrganizeRunner(config, _activity, logger);
        string folder = runner.Organizer.SourceFolder;
        if (!Directory.Exists(folder))
        {
            Error?.Invoke(this, $"Can't watch {folder} because it doesn't exist.");
            return;
        }

        _runner = runner;
        _logger = logger;

        var watcher = new FileSystemWatcher(folder)
        {
            NotifyFilter = NotifyFilters.FileName | NotifyFilters.LastWrite | NotifyFilters.Size,
            IncludeSubdirectories = false,
            InternalBufferSize = 64 * 1024,
        };
        watcher.Created += (_, e) => Queue(e.FullPath);
        watcher.Changed += (_, e) => Touch(e.FullPath);
        // Browsers download to a temporary name (.crdownload, .part) and rename at the end. Only react when the rename
        // makes the file organizable, so renaming an old file by hand doesn't whisk it away seconds later.
        watcher.Renamed += (_, e) =>
        {
            if (!runner.Organizer.IsCandidateName(e.OldName ?? string.Empty))
                Queue(e.FullPath);
        };
        watcher.Error += OnWatcherError;
        watcher.EnableRaisingEvents = true;

        _watcher = watcher;
        _timer = new Timer(_ => ProcessPending(), null, PollInterval, PollInterval);
        _logger.Info($"File watcher started on {folder}");
    }

    public void Stop()
    {
        var watcher = Interlocked.Exchange(ref _watcher, null);
        if (watcher is null)
            return;

        watcher.EnableRaisingEvents = false;
        watcher.Dispose();
        _timer?.Dispose();
        _timer = null;

        lock (_lock)
            _pending.Clear();

        _logger.Info("File watcher stopped");
    }

    public void Dispose() => Stop();

    private void Queue(string path)
    {
        lock (_lock)
        {
            var now = DateTime.UtcNow;
            _pending[path] = _pending.TryGetValue(path, out var existing)
                ? existing with { LastEvent = now }
                : new PendingFile(FirstSeen: now, LastEvent: now);
        }
    }

    private void Touch(string path)
    {
        lock (_lock)
        {
            if (_pending.TryGetValue(path, out var existing))
                _pending[path] = existing with { LastEvent = DateTime.UtcNow };
        }
    }

    private void ProcessPending()
    {
        // Timer callbacks can overlap if a pass is slow (large files moving across drives).
        if (Interlocked.Exchange(ref _processing, 1) == 1)
            return;

        try
        {
            var runner = _runner;
            if (runner is null || !IsRunning)
                return;

            var ready = TakeSettledFiles();
            if (ready.Count == 0)
                return;

            var decisions = new List<FileDecision>();
            foreach (var (path, pending) in ready)
            {
                if (!File.Exists(path))
                    continue;

                if (IsLocked(path))
                {
                    RequeueIfStillWorthWaiting(path, pending);
                    continue;
                }

                var decision = runner.Organizer.Evaluate(path, ignoreAgeThreshold: true);
                if (decision.WillMove)
                    decisions.Add(decision);
            }

            if (decisions.Count == 0)
                return;

            var summary = runner.Execute(decisions, RunTrigger.Watcher);
            if (summary.FilesMoved > 0 || summary.HasFailures)
                FilesOrganized?.Invoke(this, summary);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or TimeoutException)
        {
            _logger.Error($"File watcher pass failed: {ex.Message}");
        }
        finally
        {
            Interlocked.Exchange(ref _processing, 0);
        }
    }

    private List<(string Path, PendingFile Pending)> TakeSettledFiles()
    {
        var cutoff = DateTime.UtcNow - SettleTime;
        lock (_lock)
        {
            var ready = _pending.Where(p => p.Value.LastEvent <= cutoff).Select(p => (p.Key, p.Value)).ToList();
            foreach (var (path, _) in ready)
                _pending.Remove(path);
            return ready;
        }
    }

    private void RequeueIfStillWorthWaiting(string path, PendingFile pending)
    {
        var now = DateTime.UtcNow;
        if (now - pending.FirstSeen > MaxWaitForLock)
        {
            _logger.Warn($"File watcher gave up on {Path.GetFileName(path)}: still in use after {MaxWaitForLock.TotalMinutes:0} minutes");
            return;
        }

        lock (_lock)
            _pending.TryAdd(path, pending with { LastEvent = now });
    }

    private static bool IsLocked(string path)
    {
        try
        {
            using var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.None);
            return false;
        }
        catch (IOException)
        {
            return true;
        }
        catch (UnauthorizedAccessException)
        {
            return false;
        }
    }

    private void OnWatcherError(object sender, ErrorEventArgs e)
    {
        var ex = e.GetException();
        if (ex is InternalBufferOverflowException)
        {
            // Too many changes at once; events were dropped. The next scheduled or manual run picks up the rest.
            _logger.Warn("File watcher missed some changes (too many at once).");
            return;
        }

        _logger.Error($"File watcher stopped: {ex.Message}");
        Stop();
        Error?.Invoke(this, $"File watching stopped: {ex.Message}");
    }

    private sealed record PendingFile(DateTime FirstSeen, DateTime LastEvent);
}
