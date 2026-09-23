using TidyFlow.Core.Logging;
using TidyFlow.Core.Models;
using TidyFlow.Core.Organizing;
using TidyFlow.Core.Storage;

namespace TidyFlow.Services;

/// <summary>
/// <c>TidyFlow.exe --run</c>: one unattended pass, as launched by the scheduled task. No window is shown;
/// the result goes to the log, the history (so it can be undone later) and, if enabled, a toast.
/// </summary>
public static class HeadlessRunner
{
    public const int ExitSuccess = 0;
    public const int ExitSomeFilesFailed = 1;
    public const int ExitConfigurationError = 2;
    public const int ExitRunFailed = 3;

    public static int Run(AppPaths paths)
    {
        var preferences = new PreferencesStore(paths).Load();

        var fallbackLogger = new RunLogger(new LoggingSettings(), paths.DefaultLogDirectory);

        // Never organize with defaults nobody has reviewed; the scheduled task only exists after a save anyway.
        if (!File.Exists(paths.ConfigFile))
        {
            fallbackLogger.Error("Run skipped: TidyFlow hasn't been set up yet. Open TidyFlow and save your settings first.");
            return ExitConfigurationError;
        }

        AppConfiguration config;
        try
        {
            config = new ConfigurationStore(paths).Load();
        }
        catch (ConfigurationException ex)
        {
            fallbackLogger.Error($"Scheduled run skipped: {ex.Message}");
            Notify(preferences, "TidyFlow couldn't run", "Your settings file couldn't be read. Open TidyFlow to fix it.");
            return ExitConfigurationError;
        }

        var logger = new RunLogger(config.Logging, paths.DefaultLogDirectory);
        var errors = ConfigurationStore.Validate(config);
        if (errors.Count > 0)
        {
            logger.Error("Scheduled run skipped: " + string.Join(" ", errors));
            Notify(preferences, "TidyFlow couldn't run", errors[0]);
            return ExitConfigurationError;
        }

        try
        {
            var summary = new OrganizeRunner(config, new ActivityStore(paths), logger).Run(RunTrigger.Scheduled);

            if (summary.FilesMoved > 0 || summary.HasFailures)
                Notify(preferences, "TidyFlow organized your files", summary.Describe());

            return summary.HasFailures ? ExitSomeFilesFailed : ExitSuccess;
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or TimeoutException)
        {
            logger.Error($"Scheduled run failed: {ex.Message}");
            Notify(preferences, "TidyFlow couldn't run", ex.Message);
            return ExitRunFailed;
        }
    }

    private static void Notify(UserPreferences preferences, string title, string message)
    {
        if (preferences.ShowNotifications)
            NotificationService.Show(title, message, preferences.PlaySounds);
    }
}
