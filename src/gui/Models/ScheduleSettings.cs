using System.ComponentModel;
using Newtonsoft.Json;

namespace TidyFlow.Models
{
    /// <summary>
    /// Represents scheduling configuration for automated file organization
    /// </summary>
    public class ScheduleSettings : INotifyPropertyChanged
    {
        private bool _enabled;
        private string _frequency;
        private string _time;
        private bool _runOnStartup;

        /// <summary>
        /// Gets or sets whether scheduled execution is enabled
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
        /// Gets or sets the frequency (daily, weekly, monthly)
        /// </summary>
        [JsonProperty("frequency")]
        public string Frequency
        {
            get => _frequency;
            set
            {
                if (_frequency != value)
                {
                    _frequency = value;
                    OnPropertyChanged(nameof(Frequency));
                }
            }
        }

        /// <summary>
        /// Gets or sets the time of day to run (HH:mm format)
        /// </summary>
        [JsonProperty("time")]
        public string Time
        {
            get => _time;
            set
            {
                if (_time != value)
                {
                    _time = value;
                    OnPropertyChanged(nameof(Time));
                }
            }
        }

        /// <summary>
        /// Gets or sets whether to run on system startup
        /// </summary>
        [JsonProperty("runOnStartup")]
        public bool RunOnStartup
        {
            get => _runOnStartup;
            set
            {
                if (_runOnStartup != value)
                {
                    _runOnStartup = value;
                    OnPropertyChanged(nameof(RunOnStartup));
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
