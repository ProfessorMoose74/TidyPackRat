using System.ComponentModel;
using Newtonsoft.Json;

namespace TidyFlow.Models
{
    /// <summary>
    /// Represents logging configuration
    /// </summary>
    public class LoggingSettings : INotifyPropertyChanged
    {
        private bool _enabled;
        private string _logPath;
        private string _logLevel;
        private int _maxLogFiles;

        /// <summary>
        /// Gets or sets whether logging is enabled
        /// </summary>
        [JsonProperty("enabled")]
        public bool Enabled
        {
            get => _enabled;
            set
            {
                if (_enabled != value)
                {
                    _enabled = value;
                    OnPropertyChanged(nameof(Enabled));
                }
            }
        }

        /// <summary>
        /// Gets or sets the log file directory path
        /// </summary>
        [JsonProperty("logPath")]
        public string LogPath
        {
            get => _logPath;
            set
            {
                if (_logPath != value)
                {
                    _logPath = value;
                    OnPropertyChanged(nameof(LogPath));
                }
            }
        }

        /// <summary>
        /// Gets or sets the log level (info, warn, error)
        /// </summary>
        [JsonProperty("logLevel")]
        public string LogLevel
        {
            get => _logLevel;
            set
            {
                if (_logLevel != value)
                {
                    _logLevel = value;
                    OnPropertyChanged(nameof(LogLevel));
                }
            }
        }

        /// <summary>
        /// Gets or sets the maximum number of log files to keep
        /// </summary>
        [JsonProperty("maxLogFiles")]
        public int MaxLogFiles
        {
            get => _maxLogFiles;
            set
            {
                if (_maxLogFiles != value)
                {
                    _maxLogFiles = value;
                    OnPropertyChanged(nameof(MaxLogFiles));
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
