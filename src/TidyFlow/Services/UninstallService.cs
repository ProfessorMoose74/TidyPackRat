using Microsoft.Toolkit.Uwp.Notifications;
using TidyFlow.Core.Storage;

namespace TidyFlow.Services;

/// <summary>
/// <c>TidyFlow.exe --uninstall</c>: removes what the portable build registered with Windows (the scheduled task,
/// the sign-in startup entry and the notification registration) so its folder can simply be deleted.
/// Settings and history are left in place; the result message says where they are.
/// </summary>
public static class UninstallService
{
    public static string Run(AppPaths paths)
    {
        var removed = new List<string>();
        var problems = new List<string>();

        void Try(string what, Action action)
        {
            try
            {
                action();
                removed.Add(what);
            }
            catch (Exception ex) when (ex is System.Runtime.InteropServices.COMException or UnauthorizedAccessException
                or InvalidOperationException or IOException or System.Security.SecurityException)
            {
                problems.Add($"{what}: {ex.Message}");
            }
        }

        Try("scheduled task", TaskSchedulerService.Remove);
        Try("start-at-sign-in entry", () => StartupService.SetEnabledAsync(false).GetAwaiter().GetResult());
        Try("notification registration", ToastNotificationManagerCompat.Uninstall);

        string message = removed.Count switch
        {
            0 => "Nothing could be removed.",
            1 => $"Removed TidyFlow's {removed[0]}.",
            _ => $"Removed TidyFlow's {string.Join(", ", removed[..^1])} and {removed[^1]}.",
        };
        if (problems.Count > 0)
            message += "\n\nSome items couldn't be removed:\n" + string.Join("\n", problems);

        message += $"\n\nYour settings, history and logs are still in:\n{AppInfo.ToExternalPath(paths.DataDirectory)}\n"
            + "Delete that folder too if you don't want to keep them. You can now delete TidyFlow's program folder.";
        return message;
    }
}
