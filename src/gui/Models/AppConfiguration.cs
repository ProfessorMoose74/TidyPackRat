using System.Collections.Generic;
using System.ComponentModel;
using Newtonsoft.Json;

namespace TidyFlow.Models
{
    /// <summary>
    /// Main configuration class that represents the entire TidyFlow configuration
    /// </summary>
    public class AppConfiguration : INotifyPropertyChanged
    {
        private string _appName;
        private string _version;
        private string _sourceFolder;
        private int _fileAgeThreshold;
        private long _fileSizeThreshold;
        private string _duplicateHandling;
        private List<FileCategory> _categories;
        private ScheduleSettings _schedule;
        private List<string> _excludePatterns;
        private LoggingSettings _logging;

        /// <summary>
        /// Gets or sets the application name
        /// </summary>
        [JsonProperty("appName")]
        public string AppName
        {
            get => _appName;
            set
            {
                if (_appName != value)
                {
                    _appName = value;
                    OnPropertyChanged(nameof(AppName));
                }
            }
        }

        /// <summary>
        /// Gets or sets the configuration version
        /// </summary>
        [JsonProperty("version")]
        public string Version
        {
            get => _version;
            set
            {
                if (_version != value)
                {
                    _version = value;
                    OnPropertyChanged(nameof(Version));
                }
            }
        }

        /// <summary>
        /// Gets or sets the source folder to scan for files
        /// </summary>
        [JsonProperty("sourceFolder")]
        public string SourceFolder
        {
            get => _sourceFolder;
            set
            {
                if (_sourceFolder != value)
                {
                    _sourceFolder = value;
                    OnPropertyChanged(nameof(SourceFolder));
                }
            }
        }

        /// <summary>
        /// Gets or sets the file age threshold in hours (files newer than this won't be moved)
        /// </summary>
        [JsonProperty("fileAgeThreshold")]
        public int FileAgeThreshold
        {
            get => _fileAgeThreshold;
            set
            {
                if (_fileAgeThreshold != value)
                {
                    _fileAgeThreshold = value;
                    OnPropertyChanged(nameof(FileAgeThreshold));
                }
            }
        }

        /// <summary>
        /// Gets or sets the minimum file size threshold in bytes (files smaller won't be moved, 0 = no limit)
        /// </summary>
        [JsonProperty("fileSizeThreshold")]
        public long FileSizeThreshold
        {
            get => _fileSizeThreshold;
            set
            {
                if (_fileSizeThreshold != value)
                {
                    _fileSizeThreshold = value;
                    OnPropertyChanged(nameof(FileSizeThreshold));
                }
            }
        }

        /// <summary>
        /// Gets or sets the duplicate handling strategy (rename, skip, ask)
        /// </summary>
        [JsonProperty("duplicateHandling")]
        public string DuplicateHandling
        {
            get => _duplicateHandling;
            set
            {
                if (_duplicateHandling != value)
                {
                    _duplicateHandling = value;
                    OnPropertyChanged(nameof(DuplicateHandling));
                }
            }
        }

        /// <summary>
        /// Gets or sets the list of file categories
        /// </summary>
        [JsonProperty("categories")]
        public List<FileCategory> Categories
        {
            get => _categories;
            set
            {
                if (_categories != value)
                {
                    _categories = value;
                    OnPropertyChanged(nameof(Categories));
                }
            }
        }

        /// <summary>
        /// Gets or sets the scheduling configuration
        /// </summary>
        [JsonProperty("schedule")]
        public ScheduleSettings Schedule
        {
            get => _schedule;
            set
            {
                if (_schedule != value)
                {
                    _schedule = value;
                    OnPropertyChanged(nameof(Schedule));
                }
            }
        }

        /// <summary>
        /// Gets or sets the list of exclude patterns (wildcards)
        /// </summary>
        [JsonProperty("excludePatterns")]
        public List<string> ExcludePatterns
        {
            get => _excludePatterns;
            set
            {
                if (_excludePatterns != value)
                {
                    _excludePatterns = value;
                    OnPropertyChanged(nameof(ExcludePatterns));
                }
            }
        }

        /// <summary>
        /// Gets or sets the logging configuration
        /// </summary>
        [JsonProperty("logging")]
        public LoggingSettings Logging
        {
            get => _logging;
            set
            {
                if (_logging != value)
                {
                    _logging = value;
                    OnPropertyChanged(nameof(Logging));
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
