using System.Collections.Generic;
using System.ComponentModel;
using Newtonsoft.Json;

namespace TidyFlow.Models
{
    /// <summary>
    /// Represents a file category with associated extensions and destination folder
    /// </summary>
    public class FileCategory : INotifyPropertyChanged
    {
        private string _name;
        private List<string> _extensions;
        private string _destination;
        private bool _enabled;

        /// <summary>
        /// Gets or sets the category name (e.g., "Images", "Documents")
        /// </summary>
        [JsonProperty("name")]
        public string Name
        {
            get => _name;
            set
            {
                if (_name != value)
                {
                    _name = value;
                    OnPropertyChanged(nameof(Name));
                }
            }
        }

        /// <summary>
        /// Gets or sets the list of file extensions for this category (e.g., ".jpg", ".png")
        /// </summary>
        [JsonProperty("extensions")]
        public List<string> Extensions
        {
            get => _extensions;
            set
            {
                if (_extensions != value)
                {
                    _extensions = value;
                    OnPropertyChanged(nameof(Extensions));
                    OnPropertyChanged(nameof(ExtensionsDisplay));
                }
            }
        }

        /// <summary>
        /// Gets or sets the destination folder path for this category
        /// </summary>
        [JsonProperty("destination")]
        public string Destination
        {
            get => _destination;
            set
            {
                if (_destination != value)
                {
                    _destination = value;
                    OnPropertyChanged(nameof(Destination));
                }
            }
        }

        /// <summary>
        /// Gets or sets whether this category is enabled
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
        /// Gets a comma-separated display string of extensions
        /// </summary>
        [JsonIgnore]
        public string ExtensionsDisplay => Extensions != null ? string.Join(", ", Extensions) : string.Empty;

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
