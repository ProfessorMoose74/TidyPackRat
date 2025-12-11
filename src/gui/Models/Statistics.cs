using System;
using System.ComponentModel;
using Newtonsoft.Json;

namespace TidyFlow.Models
{
    /// <summary>
    /// Tracks file organization statistics over time
    /// </summary>
    public class Statistics : INotifyPropertyChanged
    {
        private long _totalFilesMoved;
        private long _totalBytesMoved;
        private int _totalRunCount;
        private DateTime? _firstRunDate;
        private DateTime? _lastRunDate;
        private int _filesMovedToday;
        private long _bytesMovedToday;
        private DateTime? _todayDate;

        /// <summary>
        /// Total number of files moved since installation
        /// </summary>
        [JsonProperty("totalFilesMoved")]
        public long TotalFilesMoved
        {
            get => _totalFilesMoved;
            set { _totalFilesMoved = value; OnPropertyChanged(nameof(TotalFilesMoved)); }
        }

        /// <summary>
        /// Total bytes moved since installation
        /// </summary>
        [JsonProperty("totalBytesMoved")]
        public long TotalBytesMoved
        {
            get => _totalBytesMoved;
            set { _totalBytesMoved = value; OnPropertyChanged(nameof(TotalBytesMoved)); }
        }

        /// <summary>
        /// Total number of organization runs
        /// </summary>
        [JsonProperty("totalRunCount")]
        public int TotalRunCount
        {
            get => _totalRunCount;
            set { _totalRunCount = value; OnPropertyChanged(nameof(TotalRunCount)); }
        }

        /// <summary>
        /// Date of first organization run
        /// </summary>
        [JsonProperty("firstRunDate")]
        public DateTime? FirstRunDate
        {
            get => _firstRunDate;
            set { _firstRunDate = value; OnPropertyChanged(nameof(FirstRunDate)); }
        }

        /// <summary>
        /// Date of most recent organization run
        /// </summary>
        [JsonProperty("lastRunDate")]
        public DateTime? LastRunDate
        {
            get => _lastRunDate;
            set { _lastRunDate = value; OnPropertyChanged(nameof(LastRunDate)); }
        }

        /// <summary>
        /// Files moved today
        /// </summary>
        [JsonProperty("filesMovedToday")]
        public int FilesMovedToday
        {
            get => _filesMovedToday;
            set { _filesMovedToday = value; OnPropertyChanged(nameof(FilesMovedToday)); }
        }

        /// <summary>
        /// Bytes moved today
        /// </summary>
        [JsonProperty("bytesMovedToday")]
        public long BytesMovedToday
        {
            get => _bytesMovedToday;
            set { _bytesMovedToday = value; OnPropertyChanged(nameof(BytesMovedToday)); }
        }

        /// <summary>
        /// Date for today's stats (to reset daily counters)
        /// </summary>
        [JsonProperty("todayDate")]
        public DateTime? TodayDate
        {
            get => _todayDate;
            set { _todayDate = value; OnPropertyChanged(nameof(TodayDate)); }
        }

        /// <summary>
        /// Formatted string for total bytes moved
        /// </summary>
        [JsonIgnore]
        public string TotalBytesMovedFormatted => FormatBytes(TotalBytesMoved);

        /// <summary>
        /// Formatted string for bytes moved today
        /// </summary>
        [JsonIgnore]
        public string BytesMovedTodayFormatted => FormatBytes(BytesMovedToday);

        /// <summary>
        /// Days since first use
        /// </summary>
        [JsonIgnore]
        public int DaysSinceFirstUse => FirstRunDate.HasValue
            ? (int)(DateTime.Now - FirstRunDate.Value).TotalDays
            : 0;

        /// <summary>
        /// Reset daily counters if it's a new day
        /// </summary>
        public void CheckAndResetDailyCounters()
        {
            var today = DateTime.Today;
            if (!TodayDate.HasValue || TodayDate.Value.Date != today)
            {
                FilesMovedToday = 0;
                BytesMovedToday = 0;
                TodayDate = today;
            }
        }

        /// <summary>
        /// Record a file move operation
        /// </summary>
        public void RecordFileMove(long fileSize)
        {
            CheckAndResetDailyCounters();

            TotalFilesMoved++;
            TotalBytesMoved += fileSize;
            FilesMovedToday++;
            BytesMovedToday += fileSize;
            LastRunDate = DateTime.Now;

            if (!FirstRunDate.HasValue)
            {
                FirstRunDate = DateTime.Now;
            }
        }

        /// <summary>
        /// Record completion of an organization run
        /// </summary>
        public void RecordRunComplete()
        {
            TotalRunCount++;
            LastRunDate = DateTime.Now;
        }

        /// <summary>
        /// Format bytes into human-readable string
        /// </summary>
        public static string FormatBytes(long bytes)
        {
            string[] sizes = { "B", "KB", "MB", "GB", "TB" };
            int order = 0;
            double size = bytes;

            while (size >= 1024 && order < sizes.Length - 1)
            {
                order++;
                size /= 1024;
            }

            return $"{size:0.##} {sizes[order]}";
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
