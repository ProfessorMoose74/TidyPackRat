using System.Text.Json.Serialization;

namespace TidyFlow.Core.Models;

/// <summary>One file moved by TidyFlow.</summary>
public sealed class MoveRecord
{
    public string SourcePath { get; set; } = string.Empty;

    public string DestinationPath { get; set; } = string.Empty;

    public string FileName { get; set; } = string.Empty;

    public long FileSize { get; set; }

    public string Category { get; set; } = string.Empty;

    public DateTime MovedAt { get; set; }

    /// <summary>False once the move has been undone (or could not be undone).</summary>
    public bool CanUndo { get; set; } = true;
}

/// <summary>Where a batch of moves came from.</summary>
[JsonConverter(typeof(TolerantEnumConverter<RunTrigger>))]
public enum RunTrigger
{
    Manual,
    Scheduled,
    Watcher,
}

/// <summary>The moves made by one run, undone together.</summary>
public sealed class MoveBatch
{
    public string BatchId { get; set; } = Guid.NewGuid().ToString("N")[..8];

    public RunTrigger Trigger { get; set; } = RunTrigger.Manual;

    public DateTime StartTime { get; set; }

    public DateTime EndTime { get; set; }

    public List<MoveRecord> Moves { get; set; } = [];

    public bool WasUndone { get; set; }

    [JsonIgnore]
    public int FileCount => Moves.Count;

    [JsonIgnore]
    public long TotalBytes => Moves.Sum(m => m.FileSize);

    [JsonIgnore]
    public bool CanUndo => !WasUndone && Moves.Any(m => m.CanUndo);
}

/// <summary>
/// Recent batches, most recent first. Persisted as history.json.
/// </summary>
public sealed class MoveHistory
{
    public const int DefaultMaxBatches = 50;

    public List<MoveBatch> Batches { get; set; } = [];

    public int MaxBatches { get; set; } = DefaultMaxBatches;

    public void AddBatch(MoveBatch batch)
    {
        if (batch.Moves.Count == 0)
            return;

        Batches.Insert(0, batch);

        int max = MaxBatches > 0 ? MaxBatches : DefaultMaxBatches;
        if (Batches.Count > max)
            Batches.RemoveRange(max, Batches.Count - max);
    }

    public MoveBatch? GetLastUndoableBatch() => Batches.FirstOrDefault(b => b.CanUndo);

    public MoveBatch? Find(string batchId) => Batches.FirstOrDefault(b => b.BatchId == batchId);
}
