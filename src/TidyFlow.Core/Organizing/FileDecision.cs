namespace TidyFlow.Core.Organizing;

/// <summary>Why a file was left in the source folder.</summary>
public enum SkipReason
{
    TooRecent,
    TooSmall,
    Excluded,
    Hidden,
    NoCategory,
    DuplicateExists,
    NotFound,
}

/// <summary>
/// What TidyFlow intends to do with one file: move it to a category folder, or leave it and say why.
/// </summary>
public sealed record FileDecision
{
    public required string SourcePath { get; init; }

    public string FileName => Path.GetFileName(SourcePath);

    public long Size { get; init; }

    public DateTime LastWriteTime { get; init; }

    /// <summary>Null when the file will be moved.</summary>
    public SkipReason? SkipReason { get; init; }

    public string? CategoryName { get; init; }

    /// <summary>Expanded destination folder, when a category matched.</summary>
    public string? DestinationFolder { get; init; }

    /// <summary>Full destination path, including any rename needed to avoid a name clash.</summary>
    public string? DestinationPath { get; init; }

    public bool WillMove => SkipReason is null;

    public bool IsRenamed =>
        DestinationPath is not null && !string.Equals(Path.GetFileName(DestinationPath), FileName, StringComparison.OrdinalIgnoreCase);

    /// <summary>"Move to Documents", or "Skip: modified too recently".</summary>
    public string Describe() => SkipReason is null
        ? IsRenamed ? $"Move to {CategoryName} as {Path.GetFileName(DestinationPath)}" : $"Move to {CategoryName}"
        : $"Skip: {SkipReasonText}";

    /// <summary>Why the file is being left alone, in plain words; empty when it will move.</summary>
    public string SkipReasonText => SkipReason switch
    {
        null => string.Empty,
        Organizing.SkipReason.TooRecent => "modified too recently",
        Organizing.SkipReason.TooSmall => "smaller than the minimum size",
        Organizing.SkipReason.Excluded => "matches an exclude pattern",
        Organizing.SkipReason.Hidden => "hidden or system file",
        Organizing.SkipReason.NoCategory => "no category for this file type",
        Organizing.SkipReason.DuplicateExists => $"already exists in {CategoryName}",
        Organizing.SkipReason.NotFound => "file no longer exists",
        _ => "skipped",
    };
}

/// <summary>A move that was attempted and failed.</summary>
public sealed record MoveFailure(FileDecision Decision, string Error);
