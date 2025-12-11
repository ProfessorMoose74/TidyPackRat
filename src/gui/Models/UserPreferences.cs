using System.Collections.Generic;
using System.ComponentModel;
using Newtonsoft.Json;

namespace TidyPackRat.Models
{
    /// <summary>
    /// User preferences and UI settings (separate from organization config)
    /// </summary>
    public class UserPreferences : INotifyPropertyChanged
    {
        private bool _darkMode;
        private bool _minimizeToTray;
        private bool _startMinimized;
        private bool _showNotifications;
        private bool _playSounds;
        private bool _enableFileWatcher;
        private bool _showFirstRunWizard;
        private string _soundFile;
        private double _windowWidth;
        private double _windowHeight;
        private double _windowLeft;
        private double _windowTop;

        /// <summary>
        /// Enable dark mode theme
        /// </summary>
        [JsonProperty("darkMode")]
        public bool DarkMode
        {
            get => _darkMode;
            set { _darkMode = value; OnPropertyChanged(nameof(DarkMode)); }
        }

        /// <summary>
        /// Minimize to system tray instead of taskbar
        /// </summary>
        [JsonProperty("minimizeToTray")]
        public bool MinimizeToTray
        {
            get => _minimizeToTray;
            set { _minimizeToTray = value; OnPropertyChanged(nameof(MinimizeToTray)); }
        }

        /// <summary>
        /// Start application minimized to tray
        /// </summary>
        [JsonProperty("startMinimized")]
        public bool StartMinimized
        {
            get => _startMinimized;
            set { _startMinimized = value; OnPropertyChanged(nameof(StartMinimized)); }
        }

        /// <summary>
        /// Show Windows toast notifications
        /// </summary>
        [JsonProperty("showNotifications")]
        public bool ShowNotifications
        {
            get => _showNotifications;
            set { _showNotifications = value; OnPropertyChanged(nameof(ShowNotifications)); }
        }

        /// <summary>
        /// Play sound effects when organizing files
        /// </summary>
        [JsonProperty("playSounds")]
        public bool PlaySounds
        {
            get => _playSounds;
            set { _playSounds = value; OnPropertyChanged(nameof(PlaySounds)); }
        }

        /// <summary>
        /// Enable real-time file watching
        /// </summary>
        [JsonProperty("enableFileWatcher")]
        public bool EnableFileWatcher
        {
            get => _enableFileWatcher;
            set { _enableFileWatcher = value; OnPropertyChanged(nameof(EnableFileWatcher)); }
        }

        /// <summary>
        /// Show first run wizard on next start
        /// </summary>
        [JsonProperty("showFirstRunWizard")]
        public bool ShowFirstRunWizard
        {
            get => _showFirstRunWizard;
            set { _showFirstRunWizard = value; OnPropertyChanged(nameof(ShowFirstRunWizard)); }
        }

        /// <summary>
        /// Custom sound file path (null for default)
        /// </summary>
        [JsonProperty("soundFile")]
        public string SoundFile
        {
            get => _soundFile;
            set { _soundFile = value; OnPropertyChanged(nameof(SoundFile)); }
        }

        /// <summary>
        /// Remember window width
        /// </summary>
        [JsonProperty("windowWidth")]
        public double WindowWidth
        {
            get => _windowWidth;
            set { _windowWidth = value; OnPropertyChanged(nameof(WindowWidth)); }
        }

        /// <summary>
        /// Remember window height
        /// </summary>
        [JsonProperty("windowHeight")]
        public double WindowHeight
        {
            get => _windowHeight;
            set { _windowHeight = value; OnPropertyChanged(nameof(WindowHeight)); }
        }

        /// <summary>
        /// Remember window left position
        /// </summary>
        [JsonProperty("windowLeft")]
        public double WindowLeft
        {
            get => _windowLeft;
            set { _windowLeft = value; OnPropertyChanged(nameof(WindowLeft)); }
        }

        /// <summary>
        /// Remember window top position
        /// </summary>
        [JsonProperty("windowTop")]
        public double WindowTop
        {
            get => _windowTop;
            set { _windowTop = value; OnPropertyChanged(nameof(WindowTop)); }
        }

        /// <summary>
        /// Create default preferences for first run
        /// </summary>
        public static UserPreferences CreateDefault()
        {
            return new UserPreferences
            {
                DarkMode = false,
                MinimizeToTray = true,
                StartMinimized = false,
                ShowNotifications = true,
                PlaySounds = true,
                EnableFileWatcher = false,
                ShowFirstRunWizard = true,
                SoundFile = null,
                WindowWidth = 950,
                WindowHeight = 700,
                WindowLeft = -1,
                WindowTop = -1
            };
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
