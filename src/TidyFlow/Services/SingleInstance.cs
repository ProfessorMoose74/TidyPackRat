namespace TidyFlow.Services;

/// <summary>
/// Keeps one TidyFlow window per user. A second launch hands over any file to import,
/// asks the first instance to come to the front, and exits.
/// </summary>
public sealed class SingleInstance : IDisposable
{
    private const string MutexName = @"Local\TidyFlow.SingleInstance";
    private const string ShowEventName = @"Local\TidyFlow.ShowWindow";

    private readonly Mutex _mutex;
    private readonly EventWaitHandle _showEvent;
    private readonly string _handoffFile;
    private RegisteredWaitHandle? _registration;

    private SingleInstance(Mutex mutex, EventWaitHandle showEvent, string handoffFile)
    {
        _mutex = mutex;
        _showEvent = showEvent;
        _handoffFile = handoffFile;
    }

    /// <summary>
    /// Returns the instance guard when this is the first instance; otherwise signals the running
    /// instance and returns null.
    /// </summary>
    public static SingleInstance? TryAcquire(string dataDirectory, string? importFile)
    {
        var mutex = new Mutex(initiallyOwned: true, MutexName, out bool createdNew);
        var showEvent = new EventWaitHandle(false, EventResetMode.AutoReset, ShowEventName);
        string handoffFile = Path.Combine(dataDirectory, "pending-import.txt");

        if (createdNew)
            return new SingleInstance(mutex, showEvent, handoffFile);

        if (importFile is not null)
        {
            try
            {
                Directory.CreateDirectory(dataDirectory);
                File.WriteAllText(handoffFile, importFile);
            }
            catch (IOException)
            {
            }
        }

        showEvent.Set();
        showEvent.Dispose();
        mutex.Dispose();
        return null;
    }

    /// <summary>Calls <paramref name="onShowRequested"/> (on a pool thread) with any handed-over import file.</summary>
    public void ListenForOtherInstances(Action<string?> onShowRequested)
    {
        _registration = ThreadPool.RegisterWaitForSingleObject(_showEvent, (_, _) => onShowRequested(TakeHandoffFile()), null, Timeout.Infinite, executeOnlyOnce: false);
    }

    private string? TakeHandoffFile()
    {
        try
        {
            if (!File.Exists(_handoffFile))
                return null;
            string path = File.ReadAllText(_handoffFile).Trim();
            File.Delete(_handoffFile);
            return path.Length > 0 ? path : null;
        }
        catch (IOException)
        {
            return null;
        }
    }

    public void Dispose()
    {
        _registration?.Unregister(null);
        _showEvent.Dispose();
        try { _mutex.ReleaseMutex(); } catch (ApplicationException) { }
        _mutex.Dispose();
    }
}
