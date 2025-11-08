using System;
using System.IO;
using Newtonsoft.Json;
using TidyPackRat.Models;

namespace TidyPackRat.Helpers
{
    /// <summary>
    /// Manages loading and saving of TidyPackRat configuration files
    /// </summary>
    public static class ConfigurationManager
    {
        /// <summary>
        /// Default configuration file path
        /// </summary>
        public static readonly string DefaultConfigPath =
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
                        "TidyPackRat", "config.json");

        /// <summary>
        /// Loads configuration from the specified file path
        /// </summary>
        /// <param name="filePath">Path to the configuration file</param>
        /// <returns>Loaded configuration object</returns>
        public static AppConfiguration LoadConfiguration(string filePath = null)
        {
            filePath = filePath ?? DefaultConfigPath;

            try
            {
                if (!File.Exists(filePath))
                {
                    // Return default configuration if file doesn't exist
                    return CreateDefaultConfiguration();
                }

                string jsonContent = File.ReadAllText(filePath);
                var config = JsonConvert.DeserializeObject<AppConfiguration>(jsonContent);

                // Validate configuration
                if (config == null)
                {
                    throw new InvalidOperationException("Configuration file is empty or invalid");
                }

                return config;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to load configuration from {filePath}: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Saves configuration to the specified file path
        /// </summary>
        /// <param name="config">Configuration object to save</param>
        /// <param name="filePath">Path where configuration should be saved</param>
        public static void SaveConfiguration(AppConfiguration config, string filePath = null)
        {
            filePath = filePath ?? DefaultConfigPath;

            try
            {
                // Ensure directory exists
                string directory = Path.GetDirectoryName(filePath);
                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                // Serialize with formatting for readability
                string jsonContent = JsonConvert.SerializeObject(config, Formatting.Indented);
                File.WriteAllText(filePath, jsonContent);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to save configuration to {filePath}: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Creates a default configuration with common file categories
        /// </summary>
        /// <returns>Default configuration object</returns>
        public static AppConfiguration CreateDefaultConfiguration()
        {
            string userProfile = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            string downloads = Path.Combine(userProfile, "Downloads");
            string documents = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            string pictures = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures);
            string videos = Environment.GetFolderPath(Environment.SpecialFolder.MyVideos);
            string music = Environment.GetFolderPath(Environment.SpecialFolder.MyMusic);

            return new AppConfiguration
            {
                AppName = "TidyPackRat",
                Version = "1.0.0",
                SourceFolder = downloads,
                FileAgeThreshold = 24,
                FileSizeThreshold = 0,
                DuplicateHandling = "rename",
                Categories = new System.Collections.Generic.List<FileCategory>
                {
                    new FileCategory
                    {
                        Name = "Images",
                        Extensions = new System.Collections.Generic.List<string>
                            { ".jpg", ".jpeg", ".png", ".gif", ".bmp", ".svg", ".webp", ".ico", ".tiff", ".tif" },
                        Destination = pictures,
                        Enabled = true
                    },
                    new FileCategory
                    {
                        Name = "Documents",
                        Extensions = new System.Collections.Generic.List<string>
                            { ".pdf", ".docx", ".doc", ".txt", ".rtf", ".odt", ".tex", ".wpd" },
                        Destination = documents,
                        Enabled = true
                    },
                    new FileCategory
                    {
                        Name = "Spreadsheets",
                        Extensions = new System.Collections.Generic.List<string>
                            { ".xlsx", ".xls", ".csv", ".ods", ".xlsm" },
                        Destination = Path.Combine(documents, "Spreadsheets"),
                        Enabled = true
                    },
                    new FileCategory
                    {
                        Name = "Presentations",
                        Extensions = new System.Collections.Generic.List<string>
                            { ".pptx", ".ppt", ".odp", ".key" },
                        Destination = Path.Combine(documents, "Presentations"),
                        Enabled = true
                    },
                    new FileCategory
                    {
                        Name = "Archives",
                        Extensions = new System.Collections.Generic.List<string>
                            { ".zip", ".rar", ".7z", ".tar", ".gz", ".bz2", ".xz", ".iso" },
                        Destination = Path.Combine(documents, "Archives"),
                        Enabled = true
                    },
                    new FileCategory
                    {
                        Name = "Videos",
                        Extensions = new System.Collections.Generic.List<string>
                            { ".mp4", ".avi", ".mkv", ".mov", ".wmv", ".flv", ".webm", ".m4v" },
                        Destination = videos,
                        Enabled = true
                    },
                    new FileCategory
                    {
                        Name = "Audio",
                        Extensions = new System.Collections.Generic.List<string>
                            { ".mp3", ".wav", ".flac", ".m4a", ".ogg", ".aac", ".wma", ".opus" },
                        Destination = music,
                        Enabled = true
                    },
                    new FileCategory
                    {
                        Name = "Executables",
                        Extensions = new System.Collections.Generic.List<string>
                            { ".exe", ".msi", ".bat", ".cmd", ".ps1" },
                        Destination = Path.Combine(downloads, "Executables"),
                        Enabled = false
                    },
                    new FileCategory
                    {
                        Name = "Code",
                        Extensions = new System.Collections.Generic.List<string>
                            { ".py", ".js", ".html", ".css", ".cpp", ".cs", ".java", ".php", ".rb",
                              ".go", ".rs", ".ts", ".jsx", ".tsx", ".vue", ".json", ".xml", ".yaml", ".yml" },
                        Destination = Path.Combine(documents, "Code"),
                        Enabled = false
                    }
                },
                Schedule = new ScheduleSettings
                {
                    Enabled = false,
                    Frequency = "daily",
                    Time = "02:00",
                    RunOnStartup = false
                },
                ExcludePatterns = new System.Collections.Generic.List<string>
                {
                    "*.tmp",
                    "~*",
                    "*.crdownload",
                    "*.part"
                },
                Logging = new LoggingSettings
                {
                    Enabled = true,
                    LogPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
                                          "TidyPackRat", "logs"),
                    LogLevel = "info",
                    MaxLogFiles = 12
                }
            };
        }

        /// <summary>
        /// Validates a configuration object
        /// </summary>
        /// <param name="config">Configuration to validate</param>
        /// <returns>True if valid, false otherwise</returns>
        public static bool ValidateConfiguration(AppConfiguration config)
        {
            if (config == null) return false;
            if (string.IsNullOrWhiteSpace(config.SourceFolder)) return false;
            if (config.Categories == null || config.Categories.Count == 0) return false;
            if (config.Schedule == null) return false;
            if (config.Logging == null) return false;

            return true;
        }
    }
}
