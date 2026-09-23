namespace TidyFlow.Services;

/// <summary>
/// Parsed command-line arguments.
/// <list type="bullet">
/// <item><c>--run</c>: organize once without showing any UI, then exit (used by the scheduled task).</item>
/// <item><c>--minimized</c>: start in the notification area (used when launched at sign-in).</item>
/// <item><c>--uninstall</c>: remove the scheduled task, startup entry and notification registration (portable build).</item>
/// <item>A path to a <c>.tfconfig</c> file: offer to import it (file association).</item>
/// </list>
/// </summary>
public sealed record CommandLine(bool Run, bool Minimized, string? ImportFile, bool Uninstall = false)
{
    public static CommandLine Parse(IEnumerable<string> args)
    {
        bool run = false, minimized = false, uninstall = false;
        string? importFile = null;

        foreach (string arg in args)
        {
            string flag = arg.TrimStart('-', '/').ToLowerInvariant();
            switch (flag)
            {
                case "run":
                    run = true;
                    break;
                case "minimized":
                case "tray":
                    minimized = true;
                    break;
                case "uninstall":
                    uninstall = true;
                    break;
                default:
                    if (arg.EndsWith(Core.Storage.SettingsBundle.FileExtension, StringComparison.OrdinalIgnoreCase) && File.Exists(arg))
                        importFile = Path.GetFullPath(arg);
                    // Anything else (for example -ToastActivated from a notification click) just opens the window.
                    break;
            }
        }

        return new CommandLine(run, minimized, importFile, uninstall);
    }
}
