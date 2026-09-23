using Microsoft.Win32;
using Windows.ApplicationModel;

namespace TidyFlow.Services;

/// <summary>
/// Starts TidyFlow in the notification area when the user signs in, so the file watcher keeps working.
/// The MSIX build uses the package's StartupTask (which users can also toggle in Settings &gt; Apps &gt; Startup);
/// the unpackaged build uses the per-user Run key.
/// </summary>
public static class StartupService
{
    /// <summary>Matches the TaskId in Package.appxmanifest.</summary>
    private const string StartupTaskId = "TidyFlowStartup";
    private const string RunKeyPath = @"Software\Microsoft\Windows\CurrentVersion\Run";
    private const string RunValueName = "TidyFlow";

    /// <summary>Enables or disables launch at sign-in. Returns an explanation when Windows won't allow the change.</summary>
    public static async Task<string?> SetEnabledAsync(bool enabled)
    {
        if (!AppInfo.IsPackaged)
        {
            using var key = Registry.CurrentUser.CreateSubKey(RunKeyPath);
            if (enabled)
                key.SetValue(RunValueName, $"\"{AppInfo.ExecutablePath}\" --minimized");
            else
                key.DeleteValue(RunValueName, throwOnMissingValue: false);
            return null;
        }

        var task = await StartupTask.GetAsync(StartupTaskId);
        if (!enabled)
        {
            task.Disable();
            return null;
        }

        var state = await task.RequestEnableAsync();
        return state switch
        {
            StartupTaskState.Enabled or StartupTaskState.EnabledByPolicy => null,
            StartupTaskState.DisabledByUser =>
                "Windows has TidyFlow's startup entry turned off. Turn it on in Settings > Apps > Startup.",
            StartupTaskState.DisabledByPolicy => "Your organization's policy doesn't allow TidyFlow to start at sign-in.",
            _ => "Windows didn't allow TidyFlow to start at sign-in.",
        };
    }

    /// <summary>True when this process was started by the package's startup task.</summary>
    public static bool WasLaunchedAtSignIn()
    {
        if (!AppInfo.IsPackaged)
            return false;

        try
        {
            return AppInstance.GetActivatedEventArgs()?.Kind == Windows.ApplicationModel.Activation.ActivationKind.StartupTask;
        }
        catch (Exception ex) when (ex is InvalidOperationException or System.Runtime.InteropServices.COMException)
        {
            return false;
        }
    }
}
