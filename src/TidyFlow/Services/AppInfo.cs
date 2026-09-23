using System.Reflection;

namespace TidyFlow.Services;

/// <summary>Facts about how this copy of TidyFlow is running.</summary>
public static class AppInfo
{
    /// <summary>Name of the execution alias declared in Package.appxmanifest.</summary>
    public const string ExecutionAlias = "tidyflow.exe";

    public const string ProjectUrl = "https://github.com/ProfessorMoose74/TidyPackRat";

    private static readonly Lazy<bool> s_isPackaged = new(DetectPackage);

    /// <summary>"2.0.0" (without the source-control suffix the SDK appends).</summary>
    public static string Version { get; } =
        (typeof(AppInfo).Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion ?? "0.0.0")
        .Split('+')[0];

    /// <summary>True when running from the MSIX package (Microsoft Store or sideloaded).</summary>
    public static bool IsPackaged => s_isPackaged.Value;

    public static string ExecutablePath => Environment.ProcessPath ?? Path.Combine(AppContext.BaseDirectory, "TidyFlow.exe");

    /// <summary>
    /// The command a scheduled task should launch. For the MSIX build this is the execution alias, whose path
    /// never changes between app updates (1.x pointed the task at a version-specific folder and broke on every update).
    /// </summary>
    public static string ScheduledTaskCommand => IsPackaged
        ? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Microsoft", "WindowsApps", ExecutionAlias)
        : ExecutablePath;

    /// <summary>
    /// Maps a path as TidyFlow sees it to the path other programs (Explorer) see. In the MSIX build, folders the app
    /// creates under %LOCALAPPDATA% are redirected into the package's LocalCache\Local folder, which only the app sees
    /// under the original name.
    /// </summary>
    public static string ToExternalPath(string path)
    {
        if (!IsPackaged)
            return path;

        try
        {
            string localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            string relative = Path.GetRelativePath(localAppData, Path.GetFullPath(path));
            if (relative.StartsWith("..", StringComparison.Ordinal) || Path.IsPathRooted(relative))
                return path;

            string redirected = Path.Combine(Windows.Storage.ApplicationData.Current.LocalCacheFolder.Path, "Local", relative);
            return Directory.Exists(redirected) ? redirected : path;
        }
        catch (Exception ex) when (ex is InvalidOperationException or System.Runtime.InteropServices.COMException or ArgumentException)
        {
            return path;
        }
    }

    private static bool DetectPackage()
    {
        try
        {
            _ = Windows.ApplicationModel.Package.Current.Id;
            return true;
        }
        catch (Exception ex) when (ex is InvalidOperationException or System.Runtime.InteropServices.COMException)
        {
            // "The process has no package identity."
            return false;
        }
    }
}
