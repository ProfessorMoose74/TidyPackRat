using Microsoft.Toolkit.Uwp.Notifications;

namespace TidyFlow.Services;

/// <summary>
/// Windows toast notifications, with an optional sound. Works from the GUI and from headless scheduled runs.
/// </summary>
public static class NotificationService
{
    /// <summary>Shows a toast. Returns false if Windows refused (for example, notifications are turned off).</summary>
    public static bool Show(string title, string message, bool playSound)
    {
        try
        {
            var toast = new ToastContentBuilder()
                .AddText(title)
                .AddText(message);

            // Windows plays the notification sound itself, so it still works after a scheduled run's process exits.
            if (!playSound)
                toast.AddAudio(new ToastAudio { Silent = true });

            toast.Show();
            return true;
        }
        catch (Exception ex) when (ex is InvalidOperationException or System.Runtime.InteropServices.COMException or UnauthorizedAccessException)
        {
            return false;
        }
    }

    /// <summary>Raised (on a background thread) when the user clicks one of our toasts while TidyFlow is running.</summary>
    public static event EventHandler? Activated;

    public static void ListenForActivation() =>
        ToastNotificationManagerCompat.OnActivated += _ => Activated?.Invoke(null, EventArgs.Empty);
}
