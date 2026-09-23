using TidyFlow.Core.Models;
using TidyFlow.Core.Storage;

namespace TidyFlow.Core.Tests;

/// <summary>A temporary folder tree (source, destinations, data) deleted after each test.</summary>
public sealed class TempWorkspace : IDisposable
{
    public TempWorkspace()
    {
        Root = Path.Combine(Path.GetTempPath(), "TidyFlowTests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(Source);
        Directory.CreateDirectory(Data);
        Paths = new AppPaths(Data);
    }

    public string Root { get; }

    public string Source => Path.Combine(Root, "Source");

    public string Data => Path.Combine(Root, "Data");

    public AppPaths Paths { get; }

    public string Dest(string name) => Path.Combine(Root, "Dest", name);

    /// <summary>Creates a file in the source folder, by default old enough to pass the age threshold.</summary>
    public string CreateFile(string name, int sizeBytes = 16, DateTime? lastWrite = null, FileAttributes? attributes = null)
    {
        string path = Path.Combine(Source, name);
        File.WriteAllBytes(path, new byte[sizeBytes]);
        File.SetLastWriteTime(path, lastWrite ?? TestClock.Now.AddDays(-3));
        if (attributes is FileAttributes attrs)
            File.SetAttributes(path, attrs);
        return path;
    }

    public AppConfiguration Config(Action<AppConfiguration>? customize = null)
    {
        var config = new AppConfiguration
        {
            SourceFolder = Source,
            FileAgeThresholdHours = 24,
            Categories =
            [
                new FileCategory { Name = "Documents", Extensions = [".pdf", ".txt"], Destination = Dest("Documents") },
                new FileCategory { Name = "Images", Extensions = [".jpg", ".png"], Destination = Dest("Images") },
                new FileCategory { Name = "Archives", Extensions = [".zip"], Destination = Dest("Archives"), Enabled = false },
            ],
            ExcludePatterns = ["*.tmp", "~*"],
            Logging = new LoggingSettings { Enabled = false },
        };
        customize?.Invoke(config);
        return config;
    }

    public void Dispose()
    {
        try
        {
            foreach (var file in Directory.EnumerateFiles(Root, "*", SearchOption.AllDirectories))
                File.SetAttributes(file, FileAttributes.Normal);
            Directory.Delete(Root, recursive: true);
        }
        catch (IOException)
        {
        }
    }
}

/// <summary>A clock frozen at a fixed local time.</summary>
public sealed class TestClock(DateTime? now = null) : TimeProvider
{
    public static readonly DateTime Now = new(2026, 9, 23, 14, 0, 0, DateTimeKind.Local);

    private readonly DateTime _now = now ?? Now;

    public override DateTimeOffset GetUtcNow() => new DateTimeOffset(_now).ToUniversalTime();
}
