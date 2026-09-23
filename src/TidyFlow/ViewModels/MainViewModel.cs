using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using TidyFlow.Core.Logging;
using TidyFlow.Core.Models;
using TidyFlow.Core.Organizing;
using TidyFlow.Core.Storage;
using TidyFlow.Services;

namespace TidyFlow.ViewModels;

public enum StatusKind
{
    Info,
    Success,
    Warning,
    Error,
}

/// <summary>
/// Everything behind the main window. Organization settings (Rules and Schedule tabs) are edited here and
/// only take effect on Save; app preferences (Settings tab, file watching) apply immediately.
/// </summary>
public sealed partial class MainViewModel : ObservableObject
{
    private static readonly HashSet<string> ConfigProperties =
    [
        nameof(SourceFolder), nameof(FileAgeThresholdHours), nameof(MinFileSizeKB), nameof(DuplicateHandling),
        nameof(SkipHiddenFiles), nameof(ExcludePatternsText), nameof(ScheduleEnabled), nameof(Frequency),
        nameof(ScheduleTime), nameof(DayOfWeek), nameof(DayOfMonth), nameof(RunAtSignIn),
    ];

    private readonly AppPaths _paths;
    private readonly ConfigurationStore _configStore;
    private readonly PreferencesStore _preferencesStore;
    private readonly ActivityStore _activity;
    private readonly FileWatcherService _watcher;
    private readonly IDialogService _dialogs;

    private AppConfiguration _config;
    private bool _loading;

    public MainViewModel(
        AppPaths paths,
        ConfigurationStore configStore,
        PreferencesStore preferencesStore,
        ActivityStore activity,
        FileWatcherService watcher,
        IDialogService dialogs,
        AppConfiguration config,
        UserPreferences preferences)
    {
        _paths = paths;
        _configStore = configStore;
        _preferencesStore = preferencesStore;
        _activity = activity;
        _watcher = watcher;
        _dialogs = dialogs;
        _config = config;
        Preferences = preferences;

        Categories.CollectionChanged += (_, e) =>
        {
            foreach (CategoryViewModel item in e.NewItems ?? Array.Empty<CategoryViewModel>())
                item.PropertyChanged += OnCategoryChanged;
            MarkDirty();
        };

        LoadFromConfig(config);
        LoadPreferences();
        RefreshActivity();
        RefreshScheduleStatus();
    }

    /// <summary>Asks the view to show the preview window; returns true if the user chose to go ahead.</summary>
    public Func<PreviewViewModel, bool>? ShowPreview { get; set; }

    /// <summary>True while the window is visible, so results are shown in the window rather than as a toast.</summary>
    public bool IsWindowVisible { get; set; }

    public UserPreferences Preferences { get; }

    public AppConfiguration Configuration => _config;

    public RunLogger Logger { get; private set; } = RunLogger.Disabled;

    public string VersionText { get; } = $"Version {AppInfo.Version}";

    public string DistributionText { get; } = AppInfo.IsPackaged ? "MSIX package" : "Portable";

    #region Dashboard

    [ObservableProperty]
    public partial string TotalFiles { get; set; } = "0";

    [ObservableProperty]
    public partial string TotalSize { get; set; } = "0 B";

    [ObservableProperty]
    public partial string TodayFiles { get; set; } = "0";

    [ObservableProperty]
    public partial string DaysActive { get; set; } = "0";

    [ObservableProperty]
    public partial string LastRunText { get; set; } = "TidyFlow hasn't organized anything yet.";

    [ObservableProperty]
    public partial string ScheduleStatusText { get; set; } = string.Empty;

    public ObservableCollection<ActivityItemViewModel> RecentActivity { get; } = [];

    public ObservableCollection<HistoryItemViewModel> History { get; } = [];

    [ObservableProperty]
    public partial bool HasActivity { get; set; }

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(UndoLastCommand))]
    public partial bool CanUndoLast { get; set; }

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(OrganizeNowCommand), nameof(PreviewCommand), nameof(UndoLastCommand), nameof(UndoBatchCommand))]
    public partial bool IsBusy { get; set; }

    [ObservableProperty]
    public partial string BusyText { get; set; } = string.Empty;

    #endregion

    #region Status banner

    [ObservableProperty]
    public partial string? StatusMessage { get; set; }

    [ObservableProperty]
    public partial StatusKind StatusKind { get; set; }

    public void SetStatus(StatusKind kind, string message)
    {
        StatusKind = kind;
        StatusMessage = message;
    }

    [RelayCommand]
    private void DismissStatus() => StatusMessage = null;

    #endregion

    #region Rules (saved configuration)

    [ObservableProperty]
    public partial string SourceFolder { get; set; } = string.Empty;

    public ObservableCollection<CategoryViewModel> Categories { get; } = [];

    [ObservableProperty]
    public partial int FileAgeThresholdHours { get; set; }

    [ObservableProperty]
    public partial long MinFileSizeKB { get; set; }

    [ObservableProperty]
    public partial DuplicateHandling DuplicateHandling { get; set; }

    [ObservableProperty]
    public partial bool SkipHiddenFiles { get; set; }

    [ObservableProperty]
    public partial string ExcludePatternsText { get; set; } = string.Empty;

    #endregion

    #region Schedule (saved configuration)

    [ObservableProperty]
    public partial bool ScheduleEnabled { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsWeekly), nameof(IsMonthly))]
    public partial ScheduleFrequency Frequency { get; set; }

    [ObservableProperty]
    public partial string ScheduleTime { get; set; } = "02:00";

    [ObservableProperty]
    public partial DayOfWeek DayOfWeek { get; set; }

    [ObservableProperty]
    public partial int DayOfMonth { get; set; }

    [ObservableProperty]
    public partial bool RunAtSignIn { get; set; }

    public bool IsWeekly => Frequency == ScheduleFrequency.Weekly;

    public bool IsMonthly => Frequency == ScheduleFrequency.Monthly;

    #endregion

    #region Preferences (apply immediately)

    [ObservableProperty]
    public partial AppTheme Theme { get; set; }

    [ObservableProperty]
    public partial bool WatcherEnabled { get; set; }

    [ObservableProperty]
    public partial bool MinimizeToTray { get; set; }

    [ObservableProperty]
    public partial bool StartMinimized { get; set; }

    [ObservableProperty]
    public partial bool LaunchAtSignIn { get; set; }

    [ObservableProperty]
    public partial bool ShowNotifications { get; set; }

    [ObservableProperty]
    public partial bool PlaySounds { get; set; }

    #endregion

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SaveCommand), nameof(DiscardChangesCommand))]
    public partial bool IsDirty { get; set; }

    protected override void OnPropertyChanged(PropertyChangedEventArgs e)
    {
        base.OnPropertyChanged(e);
        if (e.PropertyName is not null && ConfigProperties.Contains(e.PropertyName))
            MarkDirty();
    }

    private void OnCategoryChanged(object? sender, PropertyChangedEventArgs e) => MarkDirty();

    private void MarkDirty()
    {
        if (!_loading)
            IsDirty = true;
    }

    #region Loading and saving

    private void LoadFromConfig(AppConfiguration config)
    {
        _loading = true;
        try
        {
            _config = config;
            Logger = new RunLogger(config.Logging, _paths.DefaultLogDirectory);

            SourceFolder = config.SourceFolder;
            FileAgeThresholdHours = config.FileAgeThresholdHours;
            MinFileSizeKB = config.MinFileSizeKB;
            DuplicateHandling = config.DuplicateHandling;
            SkipHiddenFiles = config.SkipHiddenFiles;
            ExcludePatternsText = string.Join(Environment.NewLine, config.ExcludePatterns);

            Categories.Clear();
            foreach (var category in config.Categories)
                Categories.Add(new CategoryViewModel(category));

            ScheduleEnabled = config.Schedule.Enabled;
            Frequency = config.Schedule.Frequency;
            ScheduleTime = config.Schedule.Time;
            DayOfWeek = config.Schedule.DayOfWeek;
            DayOfMonth = config.Schedule.DayOfMonth;
            RunAtSignIn = config.Schedule.RunAtSignIn;
        }
        finally
        {
            _loading = false;
        }

        IsDirty = false;
    }

    private AppConfiguration BuildConfig() => new()
    {
        SourceFolder = SourceFolder.Trim(),
        FileAgeThresholdHours = FileAgeThresholdHours,
        MinFileSizeKB = MinFileSizeKB,
        DuplicateHandling = DuplicateHandling,
        SkipHiddenFiles = SkipHiddenFiles,
        Categories = Categories.Select(c => c.ToModel()).ToList(),
        ExcludePatterns = ExcludePatternsText.Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToList(),
        Schedule = new ScheduleSettings
        {
            Enabled = ScheduleEnabled,
            Frequency = Frequency,
            Time = ScheduleTime.Trim(),
            DayOfWeek = DayOfWeek,
            DayOfMonth = DayOfMonth,
            RunAtSignIn = RunAtSignIn,
        },
        Logging = _config.Logging,
    };

    [RelayCommand(CanExecute = nameof(IsDirty))]
    private void Save() => TrySave(announce: true);

    [RelayCommand(CanExecute = nameof(IsDirty))]
    private void DiscardChanges()
    {
        LoadFromConfig(_config);
        SetStatus(StatusKind.Info, "Changes discarded.");
    }

    /// <summary>Validates and saves pending changes, then updates the scheduled task and file watcher.</summary>
    private bool TrySave(bool announce)
    {
        var config = BuildConfig();
        var errors = ConfigurationStore.Validate(config);
        if (errors.Count > 0)
        {
            SetStatus(StatusKind.Error, string.Join(Environment.NewLine, errors));
            return false;
        }

        try
        {
            _configStore.Save(config);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or ConfigurationException)
        {
            SetStatus(StatusKind.Error, $"Couldn't save your settings: {ex.Message}");
            return false;
        }

        LoadFromConfig(config);

        string? scheduleProblem = ApplySchedule();
        RestartWatcherIfRunning();

        if (scheduleProblem is not null)
            SetStatus(StatusKind.Warning, $"Settings saved, but the schedule couldn't be set up: {scheduleProblem}");
        else if (announce)
            SetStatus(StatusKind.Success, "Settings saved.");

        return true;
    }

    /// <summary>Saves first if there are pending edits, so runs always use what's on screen.</summary>
    private bool EnsureSaved() => !IsDirty || TrySave(announce: false);

    private string? ApplySchedule()
    {
        try
        {
            TaskSchedulerService.Apply(_config.Schedule);
            return null;
        }
        catch (Exception ex) when (ex is System.Runtime.InteropServices.COMException or UnauthorizedAccessException or InvalidOperationException)
        {
            Logger.Error($"Scheduled task update failed: {ex.Message}");
            return ex.Message;
        }
        finally
        {
            RefreshScheduleStatus();
        }
    }

    public void RefreshScheduleStatus()
    {
        var status = TaskSchedulerService.GetStatus();
        ScheduleStatusText = status switch
        {
            { Exists: true, NextRun: DateTime next } => $"Next scheduled run: {Formatting.RelativeTime(next)}",
            { Exists: true } when _config.Schedule.RunAtSignIn => "Runs a minute after you sign in to Windows",
            { Exists: true } => "Scheduled task is set up",
            _ when TaskSchedulerService.IsNeeded(_config.Schedule) => "The scheduled task is missing. Save your settings to recreate it.",
            _ => "Not scheduled. TidyFlow only organizes when you ask it to" + (WatcherEnabled ? " or a new file arrives." : "."),
        };
    }

    #endregion

    #region Rules commands

    [RelayCommand]
    private void BrowseSource()
    {
        if (_dialogs.PickFolder("Choose the folder to organize", SourceFolder) is string folder)
            SourceFolder = folder;
    }

    [RelayCommand]
    private void BrowseDestination(CategoryViewModel? category)
    {
        if (category is not null && _dialogs.PickFolder($"Where should {category.Name} files go?", category.Destination) is string folder)
            category.Destination = folder;
    }

    [RelayCommand]
    private void AddCategory() =>
        Categories.Add(new CategoryViewModel(new FileCategory
        {
            Name = "New category",
            Destination = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "New category"),
        }));

    [RelayCommand]
    private void RemoveCategory(CategoryViewModel? category)
    {
        if (category is not null)
            Categories.Remove(category);
    }

    [RelayCommand]
    private void RestoreDefaultCategories()
    {
        if (!_dialogs.Confirm("Restore default categories", "Replace your categories with TidyFlow's defaults? Your other settings stay as they are."))
            return;

        Categories.Clear();
        foreach (var category in ConfigurationStore.CreateDefault().Categories)
            Categories.Add(new CategoryViewModel(category));
    }

    #endregion

    #region Running

    private bool CanRun() => !IsBusy;

    [RelayCommand(CanExecute = nameof(CanRun))]
    private async Task OrganizeNowAsync()
    {
        if (!EnsureSaved())
            return;

        var runner = new OrganizeRunner(_config, _activity, Logger);
        var summary = await RunBusyAsync("Organizing…", () => runner.Run(RunTrigger.Manual));
        if (summary is not null)
            ReportRun(summary);
    }

    [RelayCommand(CanExecute = nameof(CanRun))]
    private async Task PreviewAsync()
    {
        if (!EnsureSaved())
            return;

        var runner = new OrganizeRunner(_config, _activity, Logger);
        var plan = await RunBusyAsync("Checking your files…", () => runner.Organizer.Plan());
        if (plan is null)
            return;

        var preview = new PreviewViewModel(runner.Organizer.SourceFolder, plan);
        if (ShowPreview?.Invoke(preview) != true || !preview.HasMoves)
            return;

        var summary = await RunBusyAsync("Organizing…", () => runner.Execute(plan, RunTrigger.Manual));
        if (summary is not null)
            ReportRun(summary);
    }

    [RelayCommand(CanExecute = nameof(CanUndoLastNow))]
    private async Task UndoLastAsync()
    {
        var batch = _activity.Read().History.GetLastUndoableBatch();
        if (batch is not null)
            await UndoAsync(batch.BatchId, batch.FileCount, batch.StartTime);
    }

    private bool CanUndoLastNow() => CanUndoLast && !IsBusy;

    [RelayCommand(CanExecute = nameof(CanRun))]
    private async Task UndoBatchAsync(HistoryItemViewModel? item)
    {
        if (item is { CanUndo: true })
            await UndoAsync(item.BatchId, item.FileCount, item.When);
    }

    private async Task UndoAsync(string batchId, int fileCount, DateTime movedAt)
    {
        string files = fileCount == 1 ? "1 file" : $"{fileCount} files";
        if (!_dialogs.Confirm("Undo", $"Put {files} back where {(fileCount == 1 ? "it" : "they")} came from?\n\nMoved {Formatting.RelativeTime(movedAt)}."))
            return;

        var result = await RunBusyAsync("Undoing…", () => OrganizeRunner.Undo(batchId, _activity, Logger));
        if (result is null)
            return;

        RefreshActivity();

        string message = $"Restored {result.Restored.Count} {(result.Restored.Count == 1 ? "file" : "files")}.";
        if (result.Missing.Count > 0)
            message += $" {result.Missing.Count} had already been moved or deleted.";
        if (result.Failed.Count > 0)
            message += $" {result.Failed.Count} couldn't be moved back; see the log.";

        SetStatus(result.Failed.Count > 0 ? StatusKind.Warning : StatusKind.Success, message);
    }

    private async Task<T?> RunBusyAsync<T>(string text, Func<T> work) where T : class
    {
        IsBusy = true;
        BusyText = text;
        try
        {
            return await Task.Run(work);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or TimeoutException)
        {
            SetStatus(StatusKind.Error, ex.Message);
            return null;
        }
        finally
        {
            IsBusy = false;
        }
    }

    /// <summary>Shows the result of a run in the window, or as a toast when the window is hidden.</summary>
    public void ReportRun(RunSummary summary)
    {
        RefreshActivity();

        var kind = summary.HasFailures ? StatusKind.Warning : summary.FilesMoved > 0 ? StatusKind.Success : StatusKind.Info;
        SetStatus(kind, summary.Describe());

        if (summary.FilesMoved > 0 && ShowNotifications)
        {
            if (IsWindowVisible)
            {
                if (PlaySounds)
                    System.Media.SystemSounds.Asterisk.Play();
            }
            else
            {
                NotificationService.Show("TidyFlow organized your files", summary.Describe(), PlaySounds);
            }
        }
    }

    public void RefreshActivity()
    {
        Statistics statistics;
        MoveHistory history;
        try
        {
            (statistics, history) = _activity.Read();
        }
        catch (TimeoutException)
        {
            return;
        }

        var now = DateTime.Now;
        statistics.RollOverDay(now);
        TotalFiles = statistics.TotalFilesMoved.ToString("N0", System.Globalization.CultureInfo.CurrentCulture);
        TotalSize = Statistics.FormatBytes(statistics.TotalBytesMoved);
        TodayFiles = statistics.FilesMovedToday.ToString("N0", System.Globalization.CultureInfo.CurrentCulture);
        DaysActive = statistics.DaysActive(now).ToString("N0", System.Globalization.CultureInfo.CurrentCulture);
        LastRunText = statistics.LastRunDate is DateTime last
            ? $"Last run: {Formatting.RelativeTime(last)}"
            : "TidyFlow hasn't organized anything yet.";

        RecentActivity.Clear();
        // Undone moves are back where they started, so they no longer count as "recently organized".
        foreach (var move in history.Batches.Where(b => !b.WasUndone).SelectMany(b => b.Moves).Take(30))
            RecentActivity.Add(new ActivityItemViewModel(move.FileName, move.Category, move.MovedAt));

        History.Clear();
        foreach (var batch in history.Batches)
            History.Add(new HistoryItemViewModel(batch));

        HasActivity = RecentActivity.Count > 0;
        CanUndoLast = history.GetLastUndoableBatch() is not null;
    }

    #endregion

    #region File watcher and preferences

    public void StartWatcherIfEnabled()
    {
        if (WatcherEnabled)
            _watcher.Start(_config, Logger);
        OnPropertyChanged(nameof(IsWatching));
    }

    private void RestartWatcherIfRunning()
    {
        if (_watcher.IsRunning)
            _watcher.Start(_config, Logger);
        OnPropertyChanged(nameof(IsWatching));
    }

    partial void OnWatcherEnabledChanged(bool value)
    {
        if (_loading)
            return;

        if (value)
        {
            if (!EnsureSaved())
            {
                WatcherEnabled = false;
                return;
            }
            _watcher.Start(_config, Logger);
        }
        else
        {
            _watcher.Stop();
        }

        SavePreferences();
        RefreshScheduleStatus();
        OnPropertyChanged(nameof(IsWatching));
    }

    public bool IsWatching => _watcher.IsRunning;

    /// <summary>Called when the watcher stops on its own (for example, the folder was deleted).</summary>
    public void OnWatcherFailed(string message)
    {
        _loading = true;
        WatcherEnabled = _watcher.IsRunning;
        _loading = false;
        OnPropertyChanged(nameof(IsWatching));
        SetStatus(StatusKind.Warning, message);
    }

    private void LoadPreferences()
    {
        _loading = true;
        Theme = Preferences.Theme;
        WatcherEnabled = Preferences.EnableFileWatcher;
        MinimizeToTray = Preferences.MinimizeToTray;
        StartMinimized = Preferences.StartMinimized;
        LaunchAtSignIn = Preferences.LaunchAtSignIn;
        ShowNotifications = Preferences.ShowNotifications;
        PlaySounds = Preferences.PlaySounds;
        _loading = false;
    }

    partial void OnThemeChanged(AppTheme value)
    {
        ThemeService.Apply(value);
        SavePreferences();
    }

    partial void OnMinimizeToTrayChanged(bool value) => SavePreferences();

    partial void OnStartMinimizedChanged(bool value) => SavePreferences();

    partial void OnShowNotificationsChanged(bool value) => SavePreferences();

    partial void OnPlaySoundsChanged(bool value) => SavePreferences();

    partial void OnLaunchAtSignInChanged(bool value)
    {
        if (_loading)
            return;
        _ = ApplyLaunchAtSignInAsync(value);
    }

    private async Task ApplyLaunchAtSignInAsync(bool enabled)
    {
        string? problem;
        try
        {
            problem = await StartupService.SetEnabledAsync(enabled);
        }
        catch (Exception ex) when (ex is UnauthorizedAccessException or System.Runtime.InteropServices.COMException or System.Security.SecurityException)
        {
            problem = ex.Message;
        }

        if (problem is not null)
        {
            _loading = true;
            LaunchAtSignIn = false;
            _loading = false;
            SetStatus(StatusKind.Warning, problem);
        }

        SavePreferences();
    }

    public void SavePreferences()
    {
        if (_loading)
            return;

        Preferences.Theme = Theme;
        Preferences.EnableFileWatcher = WatcherEnabled;
        Preferences.MinimizeToTray = MinimizeToTray;
        Preferences.StartMinimized = StartMinimized;
        Preferences.LaunchAtSignIn = LaunchAtSignIn;
        Preferences.ShowNotifications = ShowNotifications;
        Preferences.PlaySounds = PlaySounds;

        try
        {
            _preferencesStore.Save(Preferences);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            Logger.Warn($"Couldn't save preferences: {ex.Message}");
        }
    }

    #endregion

    #region Data management

    [RelayCommand]
    private void ExportSettings()
    {
        if (_dialogs.PickSettingsFileToSave() is not string path)
            return;

        try
        {
            SettingsBundle.Export(path, IsDirty ? BuildConfig() : _config, Preferences);
            SetStatus(StatusKind.Success, $"Settings exported to {Path.GetFileName(path)}.");
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            SetStatus(StatusKind.Error, $"Couldn't export settings: {ex.Message}");
        }
    }

    [RelayCommand]
    private void ImportSettings() => ImportSettingsFrom(_dialogs.PickSettingsFileToOpen());

    /// <summary>Imports a .tfconfig file into the editor. The imported settings still need to be saved.</summary>
    public void ImportSettingsFrom(string? path)
    {
        if (path is null)
            return;

        SettingsBundle bundle;
        try
        {
            bundle = SettingsBundle.Import(path);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or ConfigurationException)
        {
            SetStatus(StatusKind.Error, $"Couldn't import {Path.GetFileName(path)}: {ex.Message}");
            return;
        }

        var imported = bundle.Configuration!;
        imported.Logging = _config.Logging;
        LoadFromConfig(imported);
        IsDirty = true;

        if (bundle.Preferences is UserPreferences prefs)
        {
            Theme = prefs.Theme;
            MinimizeToTray = prefs.MinimizeToTray;
            StartMinimized = prefs.StartMinimized;
            ShowNotifications = prefs.ShowNotifications;
            PlaySounds = prefs.PlaySounds;
        }

        SetStatus(StatusKind.Info, $"Imported {Path.GetFileName(path)}. Review the settings, then select Save to use them.");
    }

    [RelayCommand]
    private void ResetStatistics()
    {
        if (!_dialogs.Confirm("Reset statistics", "Reset the totals on the dashboard to zero? Your history and undo are kept."))
            return;

        _activity.ResetStatistics();
        RefreshActivity();
        SetStatus(StatusKind.Info, "Statistics reset.");
    }

    [RelayCommand]
    private void OpenLogFolder()
    {
        Directory.CreateDirectory(Logger.LogDirectory);
        OpenInExplorer(Logger.LogDirectory);
    }

    [RelayCommand]
    private void OpenDataFolder() => OpenInExplorer(_paths.DataDirectory);

    [RelayCommand]
    private void OpenSourceFolder() => OpenInExplorer(PathHelper.Expand(_config.SourceFolder));

    [RelayCommand]
    private static void OpenProjectPage() =>
        Process.Start(new ProcessStartInfo(AppInfo.ProjectUrl) { UseShellExecute = true });

    private void OpenInExplorer(string folder)
    {
        if (!Directory.Exists(folder))
        {
            SetStatus(StatusKind.Warning, $"{folder} doesn't exist yet.");
            return;
        }

        Process.Start(new ProcessStartInfo("explorer.exe", $"\"{AppInfo.ToExternalPath(folder)}\"") { UseShellExecute = true });
    }

    #endregion
}
