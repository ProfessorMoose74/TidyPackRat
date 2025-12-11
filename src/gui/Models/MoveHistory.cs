using System;
using System.Collections.Generic;
using System.ComponentModel;
using Newtonsoft.Json;

namespace TidyFlow.Models
{
    /// <summary>
    /// Records a single file move operation for undo capability
    /// </summary>
    public class MoveRecord
    {
        [JsonProperty("sourcePath")]
        public string SourcePath { get; set; }

        [JsonProperty("destinationPath")]
        public string DestinationPath { get; set; }

        [JsonProperty("fileName")]
        public string FileName { get; set; }

        [JsonProperty("fileSize")]
        public long FileSize { get; set; }

        [JsonProperty("category")]
        public string Category { get; set; }

        [JsonProperty("movedAt")]
        public DateTime MovedAt { get; set; }

        [JsonProperty("canUndo")]
        public bool CanUndo { get; set; } = true;
    }

    /// <summary>
    /// Records a batch of file moves from a single organization run
    /// </summary>
    public class MoveBatch
    {
        [JsonProperty("batchId")]
        public string BatchId { get; set; }

        [JsonProperty("startTime")]
        public DateTime StartTime { get; set; }

        [JsonProperty("endTime")]
        public DateTime EndTime { get; set; }

        [JsonProperty("moves")]
        public List<MoveRecord> Moves { get; set; } = new List<MoveRecord>();

        [JsonProperty("wasUndone")]
        public bool WasUndone { get; set; }

        /// <summary>
        /// Total files moved in this batch
        /// </summary>
        [JsonIgnore]
        public int FileCount => Moves?.Count ?? 0;

        /// <summary>
        /// Total bytes moved in this batch
        /// </summary>
        [JsonIgnore]
        public long TotalBytes
        {
            get
            {
                long total = 0;
                if (Moves != null)
                {
                    foreach (var move in Moves)
                    {
                        total += move.FileSize;
                    }
                }
                return total;
            }
        }

        /// <summary>
        /// Formatted total size
        /// </summary>
        [JsonIgnore]
        public string TotalBytesFormatted => Statistics.FormatBytes(TotalBytes);

        /// <summary>
        /// Create a new batch with a unique ID
        /// </summary>
        public static MoveBatch CreateNew()
        {
            return new MoveBatch
            {
                BatchId = Guid.NewGuid().ToString("N").Substring(0, 8),
                StartTime = DateTime.Now,
                Moves = new List<MoveRecord>()
            };
        }
    }

    /// <summary>
    /// Manages history of file moves for undo functionality
    /// </summary>
    public class MoveHistory : INotifyPropertyChanged
    {
        private List<MoveBatch> _batches;
        private int _maxBatches;

        /// <summary>
        /// List of move batches (most recent first)
        /// </summary>
        [JsonProperty("batches")]
        public List<MoveBatch> Batches
        {
            get => _batches ?? (_batches = new List<MoveBatch>());
            set { _batches = value; OnPropertyChanged(nameof(Batches)); }
        }

        /// <summary>
        /// Maximum number of batches to keep in history
        /// </summary>
        [JsonProperty("maxBatches")]
        public int MaxBatches
        {
            get => _maxBatches > 0 ? _maxBatches : 50;
            set { _maxBatches = value; OnPropertyChanged(nameof(MaxBatches)); }
        }

        /// <summary>
        /// Add a completed batch to history
        /// </summary>
        public void AddBatch(MoveBatch batch)
        {
            if (batch == null || batch.Moves == null || batch.Moves.Count == 0)
                return;

            batch.EndTime = DateTime.Now;
            Batches.Insert(0, batch);

            // Trim old batches
            while (Batches.Count > MaxBatches)
            {
                Batches.RemoveAt(Batches.Count - 1);
            }

            OnPropertyChanged(nameof(Batches));
        }

        /// <summary>
        /// Get the most recent batch that can be undone
        /// </summary>
        public MoveBatch GetLastUndoableBatch()
        {
            foreach (var batch in Batches)
            {
                if (!batch.WasUndone && batch.Moves != null)
                {
                    foreach (var move in batch.Moves)
                    {
                        if (move.CanUndo)
                            return batch;
                    }
                }
            }
            return null;
        }

        /// <summary>
        /// Mark a batch as undone
        /// </summary>
        public void MarkBatchUndone(string batchId)
        {
            foreach (var batch in Batches)
            {
                if (batch.BatchId == batchId)
                {
                    batch.WasUndone = true;
                    OnPropertyChanged(nameof(Batches));
                    return;
                }
            }
        }

        /// <summary>
        /// Clear all history
        /// </summary>
        public void Clear()
        {
            Batches.Clear();
            OnPropertyChanged(nameof(Batches));
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
