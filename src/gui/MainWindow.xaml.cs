using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using TidyFlow.Helpers;
using TidyFlow.Models;
using TidyFlow.Services;
using Microsoft.Win32;
using Forms = System.Windows.Forms;

namespace TidyFlow
{
    public partial class MainWindow : Window
    {
        private AppConfiguration _currentConfig;
        private UserPreferences _preferences;
        private Statistics _statistics;
        private MoveHistory _history;
        private readonly string _workerScriptPath;

        // System tray
        private Forms.NotifyIcon _notifyIcon;
        private bool _isExiting;

        // File watcher
        private FileWatcherService _fileWatcher;

        // Recent activity
        private ObservableCollection<string> _recentActivity;

        public MainWindow()
        {
            InitializeComponent();

            // Load preferences first to apply theme
            _preferences = PreferencesManager.LoadPreferences();
            _statistics = PreferencesManager.LoadStatistics();
            _history = PreferencesManager.LoadHistory();

            // Apply theme
            ThemeManager.ApplyTheme(_preferences.DarkMode);
            UpdateThemeButton();

            // Deploy worker script to stable location (required for MSIX/Store apps)
            // This ensures the script survives app updates
            var deployResult = WorkerScriptDeployer.DeployWorkerScript();
            if (!deployResult.Success)
            {
                Debug.WriteLine($"[MainWindow] Worker script deployment warning: {deployResult.ErrorMessage}");
            }
            else if (deployResult.WasUpdated)
            {
                Debug.WriteLine("[MainWindow] Worker script was updated to latest version");
            }

            // Determine worker script path (uses deployed path if available)
            _workerScriptPath = FindWorkerScript();

            // Initialize system tray
            InitializeSystemTray();

            // Initialize recent activity
            _recentActivity = new ObservableCollection<string>();
            lstRecentActivity.ItemsSource = _recentActivity;
            LoadRecentActivity();

            // Initialize file watcher
            _fileWatcher = new FileWatcherService();
            _fileWatcher.FileOrganized += FileWatcher_FileOrganized;
            _fileWatcher.Error += FileWatcher_Error;

            // Load configuration
            LoadConfiguration();

            // Load preferences into UI
            LoadPreferencesUI();

            // Update statistics display
            UpdateStatisticsDisplay();

            // Validate and repair scheduled task if needed (self-healing after app updates)
            if (_currentConfig?.Schedule?.Enabled == true)
            {
                var taskResult = TaskSchedulerHelper.ValidateAndRepairTask(_currentConfig);
                if (taskResult.WasRepaired)
                {
                    Debug.WriteLine($"[MainWindow] Scheduled task was repaired: {taskResult.Message}");
                    AddRecentActivity("Scheduled task repaired after update");
                }
            }

            // Check if should start minimized
            if (_preferences.StartMinimized)
            {
                WindowState = WindowState.Minimized;
                if (_preferences.MinimizeToTray)
                {
                    Hide();
                }
            }

            // Start file watcher if enabled
            if (_preferences.EnableFileWatcher)
            {
                StartFileWatcher();
            }

            // Set window icon
            try
            {
                var iconPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "icon.ico");
                if (File.Exists(iconPath))
                {
                    Icon = new System.Windows.Media.Imaging.BitmapImage(new Uri(iconPath));
                }
            }
            catch { }
        }

        #region System Tray

        private void InitializeSystemTray()
        {
            _notifyIcon = new Forms.NotifyIcon
            {
                Text = "TidyFlow",
                Visible = true
            };

            // Try to load icon
            try
            {
                var iconPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "icon.ico");
                if (File.Exists(iconPath))
                {
                    _notifyIcon.Icon = new System.Drawing.Icon(iconPath);
                }
                else
                {
                    _notifyIcon.Icon = System.Drawing.SystemIcons.Application;
                }
            }
            catch
            {
                _notifyIcon.Icon = System.Drawing.SystemIcons.Application;
            }

            // Context menu
            var contextMenu = new Forms.ContextMenuStrip();
            contextMenu.Items.Add("Open TidyFlow", null, (s, e) => ShowWindow());
            contextMenu.Items.Add("-");
            contextMenu.Items.Add("Run Now", null, (s, e) => RunWorkerScript(false));
            contextMenu.Items.Add("Test Run", null, (s, e) => RunWorkerScript(true));
            contextMenu.Items.Add("-");
            contextMenu.Items.Add("Exit", null, (s, e) => ExitApplication());

            _notifyIcon.ContextMenuStrip = contextMenu;
            _notifyIcon.DoubleClick += (s, e) => ShowWindow();

            // Initialize notification helper
            NotificationHelper.Initialize(_notifyIcon);
        }

        private void ShowWindow()
        {
            Show();
            WindowState = WindowState.Normal;
            Activate();
        }

        private void ExitApplication()
        {
            _isExiting = true;
            _notifyIcon?.Dispose();
            _fileWatcher?.Dispose();
            Application.Current.Shutdown();
        }

        #endregion

        #region File Watcher

        private void StartFileWatcher()
        {
            if (_currentConfig != null)
            {
                _fileWatcher.Start(_currentConfig, _statistics, _history);
                borderWatcherStatus.Visibility = Visibility.Visible;
            }
        }

        private void StopFileWatcher()
        {
            _fileWatcher.Stop();
            borderWatcherStatus.Visibility = Visibility.Collapsed;
        }

        private void FileWatcher_FileOrganized(object sender, FileOrganizedEventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                AddRecentActivity($"Moved '{e.FileName}' to {e.Category}");
                UpdateStatisticsDisplay();

                if (_preferences.ShowNotifications)
                {
                    NotificationHelper.ShowFileWatcherNotification(e.FileName, e.Category);
                }
            });
        }

        private void FileWatcher_Error(object sender, string e)
        {
            Dispatcher.Invoke(() =>
            {
                AddRecentActivity($"Error: {e}");
            });
        }

        private void FileWatcher_Changed(object sender, RoutedEventArgs e)
        {
            if (chkEnableWatcher.IsChecked == true)
            {
                StartFileWatcher();
            }
            else
            {
                StopFileWatcher();
            }
            _preferences.EnableFileWatcher = chkEnableWatcher.IsChecked ?? false;
        }

        #endregion

        #region Configuration

        private string FindWorkerScript()
        {
            // Use the WorkerScriptDeployer to get the best available execution path.
            // This handles MSIX, MSI, and development scenarios automatically.
            // The deployer prefers the stable deployed path (ProgramData) which survives updates,
            // falling back to the packaged path if deployment hasn't occurred yet.
            string path = WorkerScriptDeployer.GetExecutionPath();

            if (!string.IsNullOrEmpty(path) && File.Exists(path))
            {
                Debug.WriteLine($"[MainWindow] Using worker script at: {path}");
                return path;
            }

            // If GetExecutionPath returned nothing usable, try to deploy and get the path
            var deployResult = WorkerScriptDeployer.DeployWorkerScript();
            if (deployResult.Success)
            {
                Debug.WriteLine($"[MainWindow] Deployed worker script to: {deployResult.DeployedPath}");
                return deployResult.DeployedPath;
            }

            // Last resort fallback
            Debug.WriteLine($"[MainWindow] Worker script not found, using fallback path");
            return WorkerScriptDeployer.StableWorkerPath;
        }

        private void LoadConfiguration()
        {
            try
            {
                _currentConfig = ConfigurationManager.LoadConfiguration();

                txtSourceFolder.Text = _currentConfig.SourceFolder;
                txtFileAgeThreshold.Text = _currentConfig.FileAgeThreshold.ToString();
                txtFileSizeThreshold.Text = _currentConfig.FileSizeThreshold.ToString();

                foreach (ComboBoxItem item in cmbDuplicateHandling.Items)
                {
                    if (item.Tag.ToString() == _currentConfig.DuplicateHandling)
                    {
                        cmbDuplicateHandling.SelectedItem = item;
                        break;
                    }
                }

                if (_currentConfig.ExcludePatterns != null && _currentConfig.ExcludePatterns.Count > 0)
                {
                    txtExcludePatterns.Text = string.Join(Environment.NewLine, _currentConfig.ExcludePatterns);
                }

                dgCategories.ItemsSource = _currentConfig.Categories;

                if (_currentConfig.Schedule != null)
                {
                    chkScheduleEnabled.IsChecked = _currentConfig.Schedule.Enabled;
                    txtScheduleTime.Text = _currentConfig.Schedule.Time ?? "02:00";
                    chkRunOnStartup.IsChecked = _currentConfig.Schedule.RunOnStartup;

                    foreach (ComboBoxItem item in cmbFrequency.Items)
                    {
                        if (item.Tag.ToString() == _currentConfig.Schedule.Frequency)
                        {
                            cmbFrequency.SelectedItem = item;
                            break;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading configuration: {ex.Message}\n\nA default configuration will be used.",
                              "Configuration Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                _currentConfig = ConfigurationManager.CreateDefaultConfiguration();
            }
        }

        private void LoadPreferencesUI()
        {
            chkDarkMode.IsChecked = _preferences.DarkMode;
            chkMinimizeToTray.IsChecked = _preferences.MinimizeToTray;
            chkStartMinimized.IsChecked = _preferences.StartMinimized;
            chkShowNotifications.IsChecked = _preferences.ShowNotifications;
            chkPlaySounds.IsChecked = _preferences.PlaySounds;
            chkEnableWatcher.IsChecked = _preferences.EnableFileWatcher;

            NotificationHelper.SoundEnabled = _preferences.PlaySounds;
        }

        private void SavePreferencesFromUI()
        {
            _preferences.DarkMode = chkDarkMode.IsChecked ?? false;
            _preferences.MinimizeToTray = chkMinimizeToTray.IsChecked ?? false;
            _preferences.StartMinimized = chkStartMinimized.IsChecked ?? false;
            _preferences.ShowNotifications = chkShowNotifications.IsChecked ?? true;
            _preferences.PlaySounds = chkPlaySounds.IsChecked ?? true;
            _preferences.EnableFileWatcher = chkEnableWatcher.IsChecked ?? false;

            NotificationHelper.SoundEnabled = _preferences.PlaySounds;
            PreferencesManager.SavePreferences(_preferences);
        }

        #endregion

        #region Statistics

        private void UpdateStatisticsDisplay()
        {
            _statistics.CheckAndResetDailyCounters();

            txtStatTotalFiles.Text = _statistics.TotalFilesMoved.ToString("N0");
            txtStatTotalSize.Text = _statistics.TotalBytesMovedFormatted;
            txtStatTodayFiles.Text = _statistics.FilesMovedToday.ToString("N0");
            txtStatDaysActive.Text = _statistics.DaysSinceFirstUse.ToString("N0");
        }

        private void LoadRecentActivity()
        {
            _recentActivity.Clear();

            if (_history.Batches != null && _history.Batches.Count > 0)
            {
                int count = 0;
                foreach (var batch in _history.Batches)
                {
                    if (batch.Moves != null)
                    {
                        foreach (var move in batch.Moves)
                        {
                            _recentActivity.Add($"{move.MovedAt:g} - Moved '{move.FileName}' to {move.Category}");
                            count++;
                            if (count >= 20) return;
                        }
                    }
                }
            }

            if (_recentActivity.Count == 0)
            {
                _recentActivity.Add("No recent activity. Run TidyFlow to start organizing!");
            }
        }

        private void AddRecentActivity(string message)
        {
            _recentActivity.Insert(0, $"{DateTime.Now:g} - {message}");
            while (_recentActivity.Count > 20)
            {
                _recentActivity.RemoveAt(_recentActivity.Count - 1);
            }
        }

        #endregion

        #region Event Handlers

        private void Window_StateChanged(object sender, EventArgs e)
        {
            if (WindowState == WindowState.Minimized && _preferences.MinimizeToTray)
            {
                Hide();
            }
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            if (!_isExiting && _preferences.MinimizeToTray)
            {
                e.Cancel = true;
                Hide();
            }
            else
            {
                SavePreferencesFromUI();
                _notifyIcon?.Dispose();
                _fileWatcher?.Dispose();
            }
        }

        private void ToggleTheme_Click(object sender, RoutedEventArgs e)
        {
            _preferences.DarkMode = !_preferences.DarkMode;
            ThemeManager.ApplyTheme(_preferences.DarkMode);
            chkDarkMode.IsChecked = _preferences.DarkMode;
            UpdateThemeButton();
            PreferencesManager.SavePreferences(_preferences);
        }

        private void UpdateThemeButton()
        {
            btnToggleTheme.Content = _preferences.DarkMode ? "☀" : "🌙";
        }

        private void DarkMode_Changed(object sender, RoutedEventArgs e)
        {
            _preferences.DarkMode = chkDarkMode.IsChecked ?? false;
            ThemeManager.ApplyTheme(_preferences.DarkMode);
            UpdateThemeButton();
        }

        private void ScheduleEnabled_Changed(object sender, RoutedEventArgs e)
        {
            // UI binding handles enable/disable
        }

        private void BrowseSourceFolder_Click(object sender, RoutedEventArgs e)
        {
            using (var dialog = new Forms.FolderBrowserDialog())
            {
                dialog.Description = "Select the source folder to organize";
                dialog.SelectedPath = txtSourceFolder.Text;

                if (dialog.ShowDialog() == Forms.DialogResult.OK)
                {
                    txtSourceFolder.Text = dialog.SelectedPath;
                }
            }
        }

        private void BrowseCategoryDestination_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var category = button?.Tag as FileCategory;
            if (category == null) return;

            using (var dialog = new Forms.FolderBrowserDialog())
            {
                dialog.Description = $"Select destination folder for {category.Name}";
                dialog.SelectedPath = category.Destination;

                if (dialog.ShowDialog() == Forms.DialogResult.OK)
                {
                    category.Destination = dialog.SelectedPath;
                    dgCategories.Items.Refresh();
                }
            }
        }

        private void AddCategory_Click(object sender, RoutedEventArgs e)
        {
            // Simple dialog for adding a category
            var inputDialog = new AddCategoryDialog();
            if (inputDialog.ShowDialog() == true)
            {
                var newCategory = new FileCategory
                {
                    Name = inputDialog.CategoryName,
                    Extensions = inputDialog.Extensions.Split(',').Select(x => x.Trim().ToLower()).ToList(),
                    Destination = inputDialog.Destination,
                    Enabled = true
                };
                _currentConfig.Categories.Add(newCategory);
                dgCategories.Items.Refresh();
            }
        }

        private void UndoLast_Click(object sender, RoutedEventArgs e)
        {
            var batch = _history.GetLastUndoableBatch();
            if (batch == null)
            {
                MessageBox.Show("No recent operations to undo.", "Undo", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var result = MessageBox.Show(
                $"Undo {batch.FileCount} file move(s) from {batch.StartTime:g}?\n\nThis will move files back to their original locations.",
                "Confirm Undo",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result != MessageBoxResult.Yes) return;

            int successCount = 0;
            int failCount = 0;

            foreach (var move in batch.Moves)
            {
                try
                {
                    if (File.Exists(move.DestinationPath) && !File.Exists(move.SourcePath))
                    {
                        File.Move(move.DestinationPath, move.SourcePath);
                        successCount++;
                    }
                    else
                    {
                        failCount++;
                    }
                }
                catch
                {
                    failCount++;
                }
            }

            batch.WasUndone = true;
            PreferencesManager.SaveHistory(_history);

            string message = $"Undo complete: {successCount} file(s) restored.";
            if (failCount > 0)
                message += $"\n{failCount} file(s) could not be restored.";

            MessageBox.Show(message, "Undo Complete", MessageBoxButton.OK, MessageBoxImage.Information);
            AddRecentActivity($"Undid {successCount} file move(s)");
        }

        private void ExportSettings_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new Microsoft.Win32.SaveFileDialog
            {
                Filter = "TidyFlow Settings|*.tfconfig",
                DefaultExt = ".tfconfig",
                FileName = "TidyFlow-Settings"
            };

            if (dialog.ShowDialog() == true)
            {
                try
                {
                    PreferencesManager.ExportSettings(dialog.FileName, _currentConfig, _preferences);
                    MessageBox.Show("Settings exported successfully!", "Export", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error exporting settings: {ex.Message}", "Export Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void ImportSettings_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new Microsoft.Win32.OpenFileDialog
            {
                Filter = "TidyFlow Settings|*.tfconfig",
                DefaultExt = ".tfconfig"
            };

            if (dialog.ShowDialog() == true)
            {
                try
                {
                    var (config, prefs) = PreferencesManager.ImportSettings(dialog.FileName);

                    if (config != null)
                    {
                        _currentConfig = config;
                        LoadConfiguration();
                    }

                    if (prefs != null)
                    {
                        _preferences = prefs;
                        LoadPreferencesUI();
                        ThemeManager.ApplyTheme(_preferences.DarkMode);
                    }

                    MessageBox.Show("Settings imported successfully!", "Import", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error importing settings: {ex.Message}", "Import Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void ClearStats_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show(
                "Are you sure you want to clear all statistics?\nThis cannot be undone.",
                "Clear Statistics",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                _statistics = new Statistics();
                PreferencesManager.SaveStatistics(_statistics);
                UpdateStatisticsDisplay();
                MessageBox.Show("Statistics cleared.", "Clear Statistics", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri) { UseShellExecute = true });
            e.Handled = true;
        }

        #endregion

        #region Save and Run

        private bool IsValidFolderPath(string path)
        {
            if (string.IsNullOrWhiteSpace(path)) return false;

            try
            {
                if (path.Contains(".."))
                    path = Path.GetFullPath(path);

                Path.GetFullPath(path);

                string[] dangerousPaths = new[]
                {
                    Environment.GetFolderPath(Environment.SpecialFolder.Windows),
                    Environment.GetFolderPath(Environment.SpecialFolder.System),
                    Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles),
                    Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86),
                    Path.GetPathRoot(Environment.GetFolderPath(Environment.SpecialFolder.Windows))
                };

                string normalizedPath = Path.GetFullPath(path).TrimEnd(Path.DirectorySeparatorChar);
                foreach (var dangerous in dangerousPaths)
                {
                    if (!string.IsNullOrEmpty(dangerous))
                    {
                        string normalizedDangerous = Path.GetFullPath(dangerous).TrimEnd(Path.DirectorySeparatorChar);
                        if (string.Equals(normalizedPath, normalizedDangerous, StringComparison.OrdinalIgnoreCase))
                            return false;
                    }
                }

                return true;
            }
            catch
            {
                return false;
            }
        }

        private void SaveConfiguration_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string sourceFolder = txtSourceFolder.Text.Trim();
                if (!IsValidFolderPath(sourceFolder))
                {
                    MessageBox.Show("Invalid source folder path.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                _currentConfig.SourceFolder = sourceFolder;

                if (int.TryParse(txtFileAgeThreshold.Text, out int ageThreshold) && ageThreshold >= 0)
                    _currentConfig.FileAgeThreshold = ageThreshold;
                else
                {
                    MessageBox.Show("Please enter a valid file age threshold.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (int.TryParse(txtFileSizeThreshold.Text, out int sizeThreshold) && sizeThreshold >= 0)
                    _currentConfig.FileSizeThreshold = sizeThreshold;
                else
                {
                    MessageBox.Show("Please enter a valid file size threshold.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var selectedDuplicateItem = cmbDuplicateHandling.SelectedItem as ComboBoxItem;
                if (selectedDuplicateItem != null)
                    _currentConfig.DuplicateHandling = selectedDuplicateItem.Tag.ToString();

                _currentConfig.ExcludePatterns = txtExcludePatterns.Text
                    .Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(p => p.Trim())
                    .Where(p => !string.IsNullOrWhiteSpace(p))
                    .ToList();

                _currentConfig.Schedule.Enabled = chkScheduleEnabled.IsChecked ?? false;
                _currentConfig.Schedule.RunOnStartup = chkRunOnStartup.IsChecked ?? false;
                _currentConfig.Schedule.Time = txtScheduleTime.Text.Trim();

                var selectedFreqItem = cmbFrequency.SelectedItem as ComboBoxItem;
                if (selectedFreqItem != null)
                    _currentConfig.Schedule.Frequency = selectedFreqItem.Tag.ToString();

                ConfigurationManager.SaveConfiguration(_currentConfig);
                SavePreferencesFromUI();

                // Update scheduled task (uses stable deployed path automatically)
                var taskResult = TaskSchedulerHelper.CreateOrUpdateScheduledTask(_currentConfig);
                if (!taskResult.Success)
                {
                    MessageBox.Show($"Warning: {taskResult.Message}", "Scheduler Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                }

                // Restart file watcher with new config
                if (_preferences.EnableFileWatcher)
                {
                    StopFileWatcher();
                    StartFileWatcher();
                }

                MessageBox.Show("Configuration saved successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving configuration: {ex.Message}", "Save Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void RunNow_Click(object sender, RoutedEventArgs e)
        {
            RunWorkerScript(dryRun: false);
        }

        private void TestRun_Click(object sender, RoutedEventArgs e)
        {
            RunWorkerScript(dryRun: true);
        }

        private void RunWorkerScript(bool dryRun)
        {
            try
            {
                ConfigurationManager.SaveConfiguration(_currentConfig);

                if (!File.Exists(_workerScriptPath))
                {
                    MessageBox.Show($"Worker script not found at: {_workerScriptPath}", "Worker Script Missing", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                string configPath = ConfigurationManager.DefaultConfigPath;
                string arguments = $"-ExecutionPolicy Bypass -File \"{_workerScriptPath}\" -ConfigPath \"{configPath}\"";

                if (dryRun)
                    arguments += " -DryRun -VerboseLogging";

                var psi = new ProcessStartInfo
                {
                    FileName = "powershell.exe",
                    Arguments = arguments,
                    UseShellExecute = true,
                    WindowStyle = ProcessWindowStyle.Normal
                };

                Process.Start(psi);

                string message = dryRun
                    ? "Test run started! Check the PowerShell window for results."
                    : "TidyFlow is organizing your files!";

                AddRecentActivity(dryRun ? "Started test run" : "Started organization run");

                if (_preferences.ShowNotifications && !dryRun)
                {
                    NotificationHelper.ShowNotification("TidyFlow", message);
                }
                else
                {
                    MessageBox.Show(message, "Running", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error running worker script: {ex.Message}", "Execution Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ViewLog_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string logPath = _currentConfig.Logging?.LogPath ??
                               Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
                                          "TidyFlow", "logs");

                logPath = Environment.ExpandEnvironmentVariables(logPath);

                if (Directory.Exists(logPath))
                {
                    Process.Start("explorer.exe", logPath);
                }
                else
                {
                    MessageBox.Show($"Log directory not found: {logPath}\n\nLogs will be created after the first run.",
                                  "Log Directory Not Found", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error opening log directory: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        #endregion
    }
}
