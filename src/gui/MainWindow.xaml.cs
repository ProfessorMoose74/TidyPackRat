using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using TidyPackRat.Helpers;
using TidyPackRat.Models;
using Microsoft.Win32;
using System.Windows.Forms;
using MessageBox = System.Windows.MessageBox;

namespace TidyPackRat
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private AppConfiguration _currentConfig;
        private readonly string _workerScriptPath;

        public MainWindow()
        {
            InitializeComponent();

            // Determine worker script path (installed location or development location)
            _workerScriptPath = FindWorkerScript();

            // Load existing configuration or create default
            LoadConfiguration();

            // Set window icon (if exists)
            try
            {
                var iconPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "icon.ico");
                if (File.Exists(iconPath))
                {
                    Icon = new System.Windows.Media.Imaging.BitmapImage(new Uri(iconPath));
                }
            }
            catch { /* Icon loading is not critical */ }
        }

        private string FindWorkerScript()
        {
            // Check installed location first
            string installedPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles),
                "TidyPackRat",
                "TidyPackRat-Worker.ps1");

            if (File.Exists(installedPath))
                return installedPath;

            // Check ProgramData
            string programDataPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
                "TidyPackRat",
                "TidyPackRat-Worker.ps1");

            if (File.Exists(programDataPath))
                return programDataPath;

            // Development path (relative to GUI)
            string devPath = Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory,
                "..", "..", "worker",
                "TidyPackRat-Worker.ps1");

            if (File.Exists(devPath))
                return Path.GetFullPath(devPath);

            // Default fallback
            return programDataPath;
        }

        private void LoadConfiguration()
        {
            try
            {
                _currentConfig = ConfigurationManager.LoadConfiguration();

                // Populate UI from configuration
                txtSourceFolder.Text = _currentConfig.SourceFolder;
                txtFileAgeThreshold.Text = _currentConfig.FileAgeThreshold.ToString();

                // Set duplicate handling
                foreach (ComboBoxItem item in cmbDuplicateHandling.Items)
                {
                    if (item.Tag.ToString() == _currentConfig.DuplicateHandling)
                    {
                        cmbDuplicateHandling.SelectedItem = item;
                        break;
                    }
                }

                // Set exclude patterns
                if (_currentConfig.ExcludePatterns != null && _currentConfig.ExcludePatterns.Count > 0)
                {
                    txtExcludePatterns.Text = string.Join(Environment.NewLine, _currentConfig.ExcludePatterns);
                }

                // Bind categories to DataGrid
                dgCategories.ItemsSource = _currentConfig.Categories;

                // Set schedule settings
                if (_currentConfig.Schedule != null)
                {
                    chkScheduleEnabled.IsChecked = _currentConfig.Schedule.Enabled;
                    txtScheduleTime.Text = _currentConfig.Schedule.Time ?? "02:00";
                    chkRunOnStartup.IsChecked = _currentConfig.Schedule.RunOnStartup;

                    // Set frequency
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
                              "Configuration Error",
                              MessageBoxButton.OK,
                              MessageBoxImage.Warning);

                _currentConfig = ConfigurationManager.CreateDefaultConfiguration();
            }
        }

        private void BrowseSourceFolder_Click(object sender, RoutedEventArgs e)
        {
            using (var dialog = new FolderBrowserDialog())
            {
                dialog.Description = "Select the source folder to organize";
                dialog.SelectedPath = txtSourceFolder.Text;

                if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    txtSourceFolder.Text = dialog.SelectedPath;
                }
            }
        }

        private void BrowseCategoryDestination_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as System.Windows.Controls.Button;
            var category = button?.Tag as FileCategory;

            if (category == null) return;

            using (var dialog = new FolderBrowserDialog())
            {
                dialog.Description = $"Select destination folder for {category.Name}";
                dialog.SelectedPath = category.Destination;

                if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    category.Destination = dialog.SelectedPath;
                    dgCategories.Items.Refresh();
                }
            }
        }

        private void ScheduleEnabled_Changed(object sender, RoutedEventArgs e)
        {
            // UI binding handles enable/disable of child controls
        }

        private void SaveConfiguration_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Update configuration from UI
                _currentConfig.SourceFolder = txtSourceFolder.Text;

                if (int.TryParse(txtFileAgeThreshold.Text, out int ageThreshold))
                {
                    _currentConfig.FileAgeThreshold = ageThreshold;
                }

                var selectedDuplicateItem = cmbDuplicateHandling.SelectedItem as ComboBoxItem;
                if (selectedDuplicateItem != null)
                {
                    _currentConfig.DuplicateHandling = selectedDuplicateItem.Tag.ToString();
                }

                // Update exclude patterns
                var patterns = txtExcludePatterns.Text
                    .Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(p => p.Trim())
                    .Where(p => !string.IsNullOrWhiteSpace(p))
                    .ToList();
                _currentConfig.ExcludePatterns = patterns;

                // Update schedule settings
                _currentConfig.Schedule.Enabled = chkScheduleEnabled.IsChecked ?? false;
                _currentConfig.Schedule.Time = txtScheduleTime.Text;
                _currentConfig.Schedule.RunOnStartup = chkRunOnStartup.IsChecked ?? false;

                var selectedFreqItem = cmbFrequency.SelectedItem as ComboBoxItem;
                if (selectedFreqItem != null)
                {
                    _currentConfig.Schedule.Frequency = selectedFreqItem.Tag.ToString();
                }

                // Save configuration
                ConfigurationManager.SaveConfiguration(_currentConfig);

                // Update scheduled task if scheduling is enabled
                if (_currentConfig.Schedule.Enabled)
                {
                    if (!File.Exists(_workerScriptPath))
                    {
                        MessageBox.Show($"Worker script not found at: {_workerScriptPath}\n\nScheduling cannot be configured.",
                                      "Worker Script Missing",
                                      MessageBoxButton.OK,
                                      MessageBoxImage.Warning);
                    }
                    else if (TaskSchedulerHelper.CreateOrUpdateScheduledTask(_currentConfig, _workerScriptPath))
                    {
                        MessageBox.Show("Configuration saved and scheduled task updated successfully!",
                                      "Success",
                                      MessageBoxButton.OK,
                                      MessageBoxImage.Information);
                    }
                    else
                    {
                        MessageBox.Show("Configuration saved, but failed to update scheduled task.\nYou may need to run this application as Administrator.",
                                      "Partial Success",
                                      MessageBoxButton.OK,
                                      MessageBoxImage.Warning);
                    }
                }
                else
                {
                    // Remove scheduled task if scheduling is disabled
                    TaskSchedulerHelper.RemoveScheduledTask();

                    MessageBox.Show("Configuration saved successfully!",
                                  "Success",
                                  MessageBoxButton.OK,
                                  MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving configuration: {ex.Message}",
                              "Save Error",
                              MessageBoxButton.OK,
                              MessageBoxImage.Error);
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
                // Save current configuration first
                ConfigurationManager.SaveConfiguration(_currentConfig);

                if (!File.Exists(_workerScriptPath))
                {
                    MessageBox.Show($"Worker script not found at: {_workerScriptPath}\n\nPlease ensure TidyPackRat is properly installed.",
                                  "Worker Script Missing",
                                  MessageBoxButton.OK,
                                  MessageBoxImage.Error);
                    return;
                }

                // Build PowerShell command
                string configPath = ConfigurationManager.DefaultConfigPath;
                string arguments = $"-ExecutionPolicy Bypass -File \"{_workerScriptPath}\" -ConfigPath \"{configPath}\"";

                if (dryRun)
                {
                    arguments += " -DryRun -VerboseLogging";
                }

                var psi = new ProcessStartInfo
                {
                    FileName = "powershell.exe",
                    Arguments = arguments,
                    UseShellExecute = true,
                    WindowStyle = ProcessWindowStyle.Normal
                };

                Process.Start(psi);

                string message = dryRun
                    ? "Test run started! A PowerShell window will show what would be moved without actually moving files."
                    : "TidyPackRat is now organizing your files! Check the log for details.";

                MessageBox.Show(message,
                              "Running",
                              MessageBoxButton.OK,
                              MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error running worker script: {ex.Message}",
                              "Execution Error",
                              MessageBoxButton.OK,
                              MessageBoxImage.Error);
            }
        }

        private void ViewLog_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string logPath = _currentConfig.Logging?.LogPath ??
                               Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
                                          "TidyPackRat", "logs");

                // Expand environment variables
                logPath = Environment.ExpandEnvironmentVariables(logPath);

                if (Directory.Exists(logPath))
                {
                    // Open log directory in Explorer
                    Process.Start("explorer.exe", logPath);
                }
                else
                {
                    MessageBox.Show($"Log directory not found: {logPath}\n\nLogs will be created after the first run.",
                                  "Log Directory Not Found",
                                  MessageBoxButton.OK,
                                  MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error opening log directory: {ex.Message}",
                              "Error",
                              MessageBoxButton.OK,
                              MessageBoxImage.Error);
            }
        }
    }
}
