using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TidyFlow.Helpers;
using TidyFlow.Models;

namespace TidyFlow.Services
{
    /// <summary>
    /// Watches source folders for new files and organizes them in real-time
    /// </summary>
    public class FileWatcherService : IDisposable
    {
        private FileSystemWatcher _watcher;
        private AppConfiguration _config;
        private Statistics _statistics;
        private MoveHistory _history;
        private readonly object _lock = new object();
        private readonly Dictionary<string, DateTime> _pendingFiles = new Dictionary<string, DateTime>();
        private Timer _processTimer;
        private bool _isRunning;

        /// <summary>
        /// Event raised when a file is organized
        /// </summary>
        public event EventHandler<FileOrganizedEventArgs> FileOrganized;

        /// <summary>
        /// Event raised when an error occurs
        /// </summary>
        public event EventHandler<string> Error;

        /// <summary>
        /// Whether the watcher is currently running
        /// </summary>
        public bool IsRunning => _isRunning;

        /// <summary>
        /// Start watching the configured source folder
        /// </summary>
        public void Start(AppConfiguration config, Statistics statistics, MoveHistory history)
        {
            if (_isRunning)
                return;

            _config = config;
            _statistics = statistics;
            _history = history;

            string sourceFolder = Environment.ExpandEnvironmentVariables(_config.SourceFolder);

            if (!Directory.Exists(sourceFolder))
            {
                Error?.Invoke(this, $"Source folder does not exist: {sourceFolder}");
                return;
            }

            try
            {
                _watcher = new FileSystemWatcher(sourceFolder)
                {
                    NotifyFilter = NotifyFilters.FileName | NotifyFilters.CreationTime | NotifyFilters.LastWrite,
                    EnableRaisingEvents = true,
                    IncludeSubdirectories = false
                };

                _watcher.Created += OnFileCreated;
                _watcher.Renamed += OnFileRenamed;

                // Timer to process pending files (allows downloads to complete)
                _processTimer = new Timer(ProcessPendingFiles, null, TimeSpan.FromSeconds(5), TimeSpan.FromSeconds(10));

                _isRunning = true;
            }
            catch (Exception ex)
            {
                Error?.Invoke(this, $"Failed to start file watcher: {ex.Message}");
            }
        }

        /// <summary>
        /// Stop watching
        /// </summary>
        public void Stop()
        {
            _isRunning = false;
            _processTimer?.Dispose();
            _processTimer = null;
            _watcher?.Dispose();
            _watcher = null;

            lock (_lock)
            {
                _pendingFiles.Clear();
            }
        }

        private void OnFileCreated(object sender, FileSystemEventArgs e)
        {
            QueueFile(e.FullPath);
        }

        private void OnFileRenamed(object sender, RenamedEventArgs e)
        {
            // Check if it was renamed FROM a temp extension to a real extension
            // This handles browser downloads that rename .crdownload to final name
            string oldExt = Path.GetExtension(e.OldFullPath).ToLowerInvariant();
            if (oldExt == ".crdownload" || oldExt == ".part" || oldExt == ".tmp")
            {
                QueueFile(e.FullPath);
            }
        }

        private void QueueFile(string filePath)
        {
            lock (_lock)
            {
                // Add or update the pending file with current timestamp
                _pendingFiles[filePath] = DateTime.Now;
            }
        }

        private void ProcessPendingFiles(object state)
        {
            if (!_isRunning || _config == null)
                return;

            List<string> filesToProcess;

            lock (_lock)
            {
                // Get files that have been pending for at least the age threshold
                var threshold = DateTime.Now.AddHours(-_config.FileAgeThreshold);
                // For file watcher, use a shorter delay (30 seconds minimum to let downloads complete)
                var minDelay = DateTime.Now.AddSeconds(-30);

                filesToProcess = _pendingFiles
                    .Where(kvp => kvp.Value < minDelay)
                    .Select(kvp => kvp.Key)
                    .ToList();

                foreach (var file in filesToProcess)
                {
                    _pendingFiles.Remove(file);
                }
            }

            foreach (var filePath in filesToProcess)
            {
                try
                {
                    ProcessFile(filePath);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error processing {filePath}: {ex.Message}");
                }
            }
        }

        private void ProcessFile(string filePath)
        {
            if (!File.Exists(filePath))
                return;

            string fileName = Path.GetFileName(filePath);
            string extension = Path.GetExtension(filePath).ToLowerInvariant();

            // Check exclude patterns
            if (_config.ExcludePatterns != null)
            {
                foreach (var pattern in _config.ExcludePatterns)
                {
                    if (MatchesPattern(fileName, pattern))
                        return;
                }
            }

            // Check file size threshold
            var fileInfo = new FileInfo(filePath);
            if (_config.FileSizeThreshold > 0 && fileInfo.Length < _config.FileSizeThreshold * 1024)
                return;

            // Find matching category
            FileCategory matchingCategory = null;
            foreach (var category in _config.Categories)
            {
                if (!category.Enabled)
                    continue;

                if (category.Extensions != null && category.Extensions.Contains(extension))
                {
                    matchingCategory = category;
                    break;
                }
            }

            if (matchingCategory == null)
                return;

            // Move the file
            string destFolder = Environment.ExpandEnvironmentVariables(matchingCategory.Destination);
            if (!Directory.Exists(destFolder))
            {
                Directory.CreateDirectory(destFolder);
            }

            string destPath = Path.Combine(destFolder, fileName);

            // Handle duplicates
            if (File.Exists(destPath))
            {
                if (_config.DuplicateHandling == "skip")
                    return;

                // Rename with number
                int counter = 1;
                string nameWithoutExt = Path.GetFileNameWithoutExtension(fileName);
                while (File.Exists(destPath))
                {
                    destPath = Path.Combine(destFolder, $"{nameWithoutExt}_{counter}{extension}");
                    counter++;
                }
            }

            // Perform the move
            File.Move(filePath, destPath);

            // Record in history
            var batch = MoveBatch.CreateNew();
            batch.Moves.Add(new MoveRecord
            {
                SourcePath = filePath,
                DestinationPath = destPath,
                FileName = fileName,
                FileSize = fileInfo.Length,
                Category = matchingCategory.Name,
                MovedAt = DateTime.Now
            });
            _history.AddBatch(batch);
            PreferencesManager.SaveHistory(_history);

            // Update statistics
            _statistics.RecordFileMove(fileInfo.Length);
            PreferencesManager.SaveStatistics(_statistics);

            // Raise event
            FileOrganized?.Invoke(this, new FileOrganizedEventArgs
            {
                FileName = fileName,
                Category = matchingCategory.Name,
                FileSize = fileInfo.Length,
                DestinationPath = destPath
            });
        }

        private bool MatchesPattern(string fileName, string pattern)
        {
            // Simple wildcard matching
            if (pattern.StartsWith("*"))
            {
                return fileName.EndsWith(pattern.Substring(1), StringComparison.OrdinalIgnoreCase);
            }
            if (pattern.EndsWith("*"))
            {
                return fileName.StartsWith(pattern.Substring(0, pattern.Length - 1), StringComparison.OrdinalIgnoreCase);
            }
            return string.Equals(fileName, pattern, StringComparison.OrdinalIgnoreCase);
        }

        public void Dispose()
        {
            Stop();
        }
    }

    /// <summary>
    /// Event args for file organized event
    /// </summary>
    public class FileOrganizedEventArgs : EventArgs
    {
        public string FileName { get; set; }
        public string Category { get; set; }
        public long FileSize { get; set; }
        public string DestinationPath { get; set; }
    }
}
