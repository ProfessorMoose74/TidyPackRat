using System;
using System.IO;
using Newtonsoft.Json;
using TidyFlow.Models;

namespace TidyFlow.Helpers
{
    /// <summary>
    /// Manages user preferences, statistics, and move history persistence
    /// </summary>
    public static class PreferencesManager
    {
        private static readonly string DataFolder = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
            "TidyFlow");

        public static string PreferencesPath => Path.Combine(DataFolder, "preferences.json");
        public static string StatisticsPath => Path.Combine(DataFolder, "statistics.json");
        public static string HistoryPath => Path.Combine(DataFolder, "history.json");

        /// <summary>
        /// Ensure the data folder exists
        /// </summary>
        private static void EnsureDataFolder()
        {
            if (!Directory.Exists(DataFolder))
            {
                Directory.CreateDirectory(DataFolder);
            }
        }

        #region User Preferences

        /// <summary>
        /// Load user preferences from disk
        /// </summary>
        public static UserPreferences LoadPreferences()
        {
            try
            {
                if (File.Exists(PreferencesPath))
                {
                    string json = File.ReadAllText(PreferencesPath);
                    var prefs = JsonConvert.DeserializeObject<UserPreferences>(json);
                    if (prefs != null)
                        return prefs;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading preferences: {ex.Message}");
            }

            return UserPreferences.CreateDefault();
        }

        /// <summary>
        /// Save user preferences to disk
        /// </summary>
        public static void SavePreferences(UserPreferences preferences)
        {
            try
            {
                EnsureDataFolder();
                string json = JsonConvert.SerializeObject(preferences, Formatting.Indented);
                File.WriteAllText(PreferencesPath, json);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error saving preferences: {ex.Message}");
            }
        }

        #endregion

        #region Statistics

        /// <summary>
        /// Load statistics from disk
        /// </summary>
        public static Statistics LoadStatistics()
        {
            try
            {
                if (File.Exists(StatisticsPath))
                {
                    string json = File.ReadAllText(StatisticsPath);
                    var stats = JsonConvert.DeserializeObject<Statistics>(json);
                    if (stats != null)
                    {
                        stats.CheckAndResetDailyCounters();
                        return stats;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading statistics: {ex.Message}");
            }

            return new Statistics();
        }

        /// <summary>
        /// Save statistics to disk
        /// </summary>
        public static void SaveStatistics(Statistics statistics)
        {
            try
            {
                EnsureDataFolder();
                string json = JsonConvert.SerializeObject(statistics, Formatting.Indented);
                File.WriteAllText(StatisticsPath, json);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error saving statistics: {ex.Message}");
            }
        }

        #endregion

        #region Move History

        /// <summary>
        /// Load move history from disk
        /// </summary>
        public static MoveHistory LoadHistory()
        {
            try
            {
                if (File.Exists(HistoryPath))
                {
                    string json = File.ReadAllText(HistoryPath);
                    var history = JsonConvert.DeserializeObject<MoveHistory>(json);
                    if (history != null)
                        return history;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading history: {ex.Message}");
            }

            return new MoveHistory();
        }

        /// <summary>
        /// Save move history to disk
        /// </summary>
        public static void SaveHistory(MoveHistory history)
        {
            try
            {
                EnsureDataFolder();
                string json = JsonConvert.SerializeObject(history, Formatting.Indented);
                File.WriteAllText(HistoryPath, json);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error saving history: {ex.Message}");
            }
        }

        #endregion

        #region Export/Import

        /// <summary>
        /// Export all settings to a single file
        /// </summary>
        public static void ExportSettings(string filePath, AppConfiguration config, UserPreferences prefs)
        {
            var export = new
            {
                exportDate = DateTime.Now,
                exportVersion = "1.0",
                configuration = config,
                preferences = prefs
            };

            string json = JsonConvert.SerializeObject(export, Formatting.Indented);
            File.WriteAllText(filePath, json);
        }

        /// <summary>
        /// Import settings from an export file
        /// </summary>
        public static (AppConfiguration config, UserPreferences prefs) ImportSettings(string filePath)
        {
            string json = File.ReadAllText(filePath);
            dynamic import = JsonConvert.DeserializeObject(json);

            AppConfiguration config = null;
            UserPreferences prefs = null;

            if (import.configuration != null)
            {
                config = JsonConvert.DeserializeObject<AppConfiguration>(import.configuration.ToString());
            }

            if (import.preferences != null)
            {
                prefs = JsonConvert.DeserializeObject<UserPreferences>(import.preferences.ToString());
            }

            return (config, prefs);
        }

        #endregion
    }
}
