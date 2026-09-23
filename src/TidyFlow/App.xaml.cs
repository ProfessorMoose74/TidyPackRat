using System.Windows;
using System.Windows.Threading;
using TidyFlow.Core.Logging;
using TidyFlow.Core.Models;
using TidyFlow.Core.Storage;
using TidyFlow.Services;
using TidyFlow.ViewModels;
using TidyFlow.Views;

namespace TidyFlow;

[System.Diagnostics.CodeAnalysis.SuppressMessage("Reliability", "CA1001", Justification = "WPF never disposes Application; fields are disposed in OnExit.")]
public partial class App : Application
{
    private SingleInstance? _instance;
    private TrayIcon? _tray;
    private FileWatcherService? _watcher;
    private MainViewModel? _viewModel;
    private MainWindow? _window;
    private bool _windowClosed;
    private RunLogger _startupLogger = RunLogger.Disabled;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        var commandLine = CommandLine.Parse(e.Args);
        var paths = AppPaths.Default;

        if (commandLine.Run)
        {
            Shutdown(HeadlessRunner.Run(paths));
            return;
        }

        if (commandLine.Uninstall)
        {
            RunUninstall(paths);
            return;
        }

        _instance = SingleInstance.TryAcquire(paths.DataDirectory, commandLine.ImportFile);
        if (_instance is null)
        {
            Shutdown();
            return;
        }

        // Until the configuration (and its log settings) is loaded, log to the default location.
        _startupLogger = new RunLogger(new LoggingSettings(), paths.DefaultLogDirectory);
        DispatcherUnhandledException += OnUnhandledException;
        LegacyCleanup.Run(paths);

        var preferencesStore = new PreferencesStore(paths);
        var preferences = preferencesStore.Load();
        ThemeService.Apply(preferences.Theme);

        var configStore = new ConfigurationStore(paths);
        var config = LoadConfiguration(configStore);

        var activity = new ActivityStore(paths);
        _watcher = new FileWatcherService(activity);
        _viewModel = new MainViewModel(paths, configStore, preferencesStore, activity, _watcher, new DialogService(), config, preferences);
        _window = new MainWindow(_viewModel);
        MainWindow = _window;
        _window.Closed += (_, _) =>
        {
            _windowClosed = true;
            Shutdown();
        };

        _watcher.FilesOrganized += (_, summary) => Dispatcher.BeginInvoke(() => _viewModel.ReportRun(summary));
        _watcher.Error += (_, message) => Dispatcher.BeginInvoke(() =>
        {
            _viewModel.OnWatcherFailed(message);
            _tray?.SetWatching(false);
        });
        _viewModel.PropertyChanged += (_, args) =>
        {
            if (args.PropertyName == nameof(MainViewModel.IsWatching))
                _tray?.SetWatching(_viewModel.IsWatching);
        };

        _tray = new TrayIcon(
            open: ShowMainWindow,
            organizeNow: () => _viewModel.OrganizeNowCommand.Execute(null),
            preview: () => _viewModel.PreviewCommand.Execute(null),
            toggleWatcher: () => _viewModel.WatcherEnabled = !_viewModel.WatcherEnabled,
            exit: ExitApplication);

        RepairScheduledTask(config);

        bool startHidden = commandLine.Minimized || preferences.StartMinimized || StartupService.WasLaunchedAtSignIn();
        if (!startHidden || commandLine.ImportFile is not null)
            _window.Show();

        _viewModel.StartWatcherIfEnabled();
        _tray.SetWatching(_viewModel.IsWatching);

        if (commandLine.ImportFile is not null)
            _viewModel.ImportSettingsFrom(commandLine.ImportFile);

        _instance.ListenForOtherInstances(importFile => Dispatcher.BeginInvoke(() =>
        {
            ShowMainWindow();
            if (importFile is not null)
                _viewModel.ImportSettingsFrom(importFile);
        }));

        NotificationService.Activated += (_, _) => Dispatcher.BeginInvoke(ShowMainWindow);
        NotificationService.ListenForActivation();
    }

    protected override void OnExit(ExitEventArgs e)
    {
        _watcher?.Dispose();
        _tray?.Dispose();
        _instance?.Dispose();
        base.OnExit(e);
    }

    private void RunUninstall(AppPaths paths)
    {
        if (AppInfo.IsPackaged)
        {
            MessageBox.Show("To remove this copy of TidyFlow, uninstall it from Settings > Apps > Installed apps.",
                "TidyFlow", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        else if (MessageBox.Show(
                     "Remove TidyFlow's scheduled task, start-at-sign-in entry and notification registration?\n\n"
                     + "Close any running TidyFlow window first. Your settings and history are kept.",
                     "Uninstall TidyFlow", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
        {
            MessageBox.Show(UninstallService.Run(paths), "Uninstall TidyFlow", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        Shutdown();
    }

    private static AppConfiguration LoadConfiguration(ConfigurationStore store)
    {
        try
        {
            return store.Load();
        }
        catch (ConfigurationException ex)
        {
            MessageBox.Show(
                $"{ex.Message}\n\nTidyFlow will start with its default settings. Your file hasn't been changed, and will only be replaced (with a backup) if you save.",
                "TidyFlow settings couldn't be read", MessageBoxButton.OK, MessageBoxImage.Warning);
            return ConfigurationStore.CreateDefault();
        }
    }

    /// <summary>Recreates the scheduled task if it's missing or still points at the 1.x PowerShell worker.</summary>
    private void RepairScheduledTask(AppConfiguration config)
    {
        try
        {
            if (TaskSchedulerService.Repair(config.Schedule))
            {
                _viewModel!.Logger.Info("Scheduled task repaired to match the current settings.");
                _viewModel.RefreshScheduleStatus();
            }
        }
        catch (Exception ex) when (ex is System.Runtime.InteropServices.COMException or UnauthorizedAccessException or InvalidOperationException)
        {
            _viewModel!.Logger.Warn($"Couldn't check the scheduled task: {ex.Message}");
        }
    }

    private void ShowMainWindow() => _window?.ShowAndActivate();

    private void ExitApplication()
    {
        if (_window is null || _windowClosed)
        {
            Shutdown();
            return;
        }

        // Closing the window saves its placement and offers to save edits; Closed then shuts down.
        _window.IsExiting = true;
        _window.Close();
    }

    private void OnUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
    {
        (_viewModel?.Logger ?? _startupLogger).Error($"Unexpected error: {e.Exception}");
        MessageBox.Show($"Something went wrong:\n\n{e.Exception.Message}\n\nDetails were written to the TidyFlow log.",
            "TidyFlow", MessageBoxButton.OK, MessageBoxImage.Error);
        e.Handled = true;
    }
}
