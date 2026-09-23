using System.IO.Enumeration;
using TidyFlow.Core.Models;
using TidyFlow.Core.Storage;

namespace TidyFlow.Core.Organizing;

/// <summary>
/// The organizing engine. Decides where each file in the source folder belongs and moves it there.
/// Used by manual runs, scheduled runs and the file watcher, so all three follow exactly the same rules.
/// </summary>
public sealed class Organizer
{
    /// <summary>Give up looking for a free "name_N" after this many attempts.</summary>
    private const int MaxRenameAttempts = 10_000;

    private readonly AppConfiguration _config;
    private readonly TimeProvider _time;
    private readonly Dictionary<string, FileCategory> _categoriesByExtension;

    public Organizer(AppConfiguration config, TimeProvider? time = null)
    {
        _config = config;
        _time = time ?? TimeProvider.System;
        SourceFolder = PathHelper.Expand(config.SourceFolder);

        // First enabled category wins when two claim the same extension, matching the order shown in the GUI.
        _categoriesByExtension = new Dictionary<string, FileCategory>(StringComparer.OrdinalIgnoreCase);
        foreach (var category in config.Categories.Where(c => c.Enabled))
        {
            foreach (string extension in category.Extensions)
                _categoriesByExtension.TryAdd(extension, category);
        }
    }

    /// <summary>The expanded source folder.</summary>
    public string SourceFolder { get; }

    /// <summary>
    /// Decides what to do with every file directly inside the source folder (subfolders are never touched).
    /// Nothing is moved.
    /// </summary>
    /// <param name="ignoreAgeThreshold">True for the file watcher, which organizes new files straight away.</param>
    public IReadOnlyList<FileDecision> Plan(bool ignoreAgeThreshold = false)
    {
        if (!Directory.Exists(SourceFolder))
            throw new DirectoryNotFoundException($"The source folder doesn't exist: {SourceFolder}");

        var reserved = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        return new DirectoryInfo(SourceFolder)
            .EnumerateFiles("*", new EnumerationOptions { IgnoreInaccessible = true, AttributesToSkip = 0 })
            .OrderBy(f => f.Name, StringComparer.OrdinalIgnoreCase)
            .Select(file => Decide(file, ignoreAgeThreshold, reserved))
            .ToList();
    }

    /// <summary>Decides what to do with a single file (used by the file watcher).</summary>
    public FileDecision Evaluate(string filePath, bool ignoreAgeThreshold = false) =>
        Decide(new FileInfo(filePath), ignoreAgeThreshold, reserved: null);

    /// <summary>
    /// Moves every file the decisions say to move. Destination names are re-checked at move time,
    /// because the folder may have changed since the plan was made.
    /// </summary>
    public OrganizeResult Execute(IEnumerable<FileDecision> decisions, CancellationToken cancellationToken = default)
    {
        var result = new OrganizeResult();

        foreach (var decision in decisions)
        {
            if (!decision.WillMove)
            {
                result.Skipped.Add(decision);
                continue;
            }

            if (cancellationToken.IsCancellationRequested)
            {
                result.Cancelled = true;
                break;
            }

            MoveOne(decision, result);
        }

        return result;
    }

    /// <summary>
    /// Moves the files in a batch back to where they came from. If a file with the same name has
    /// since appeared in the source folder, the restored file gets a numeric suffix rather than overwriting it.
    /// </summary>
    public static UndoResult Undo(MoveBatch batch)
    {
        var result = new UndoResult();

        foreach (var move in batch.Moves.Where(m => m.CanUndo))
        {
            try
            {
                if (!File.Exists(move.DestinationPath))
                {
                    result.Missing.Add(move);
                    move.CanUndo = false;
                    continue;
                }

                string folder = Path.GetDirectoryName(move.SourcePath)!;
                Directory.CreateDirectory(folder);
                string name = FindFreeName(folder, Path.GetFileName(move.SourcePath), reserved: null)
                    ?? throw new IOException("No free file name available.");

                File.Move(move.DestinationPath, Path.Combine(folder, name));
                move.CanUndo = false;
                result.Restored.Add(move);
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
            {
                result.Failed.Add((move, ex.Message));
            }
        }

        batch.WasUndone = !batch.Moves.Any(m => m.CanUndo);
        return result;
    }

    private FileDecision Decide(FileInfo file, bool ignoreAgeThreshold, HashSet<string>? reserved)
    {
        file.Refresh();
        if (!file.Exists)
            return new FileDecision { SourcePath = file.FullName, SkipReason = SkipReason.NotFound };

        var decision = new FileDecision
        {
            SourcePath = file.FullName,
            Size = file.Length,
            LastWriteTime = file.LastWriteTime,
        };

        if (_config.SkipHiddenFiles && (file.Attributes & (FileAttributes.Hidden | FileAttributes.System)) != 0)
            return decision with { SkipReason = SkipReason.Hidden };

        if (IsExcluded(file.Name))
            return decision with { SkipReason = SkipReason.Excluded };

        if (!ignoreAgeThreshold && _config.FileAgeThresholdHours > 0
            && file.LastWriteTime > _time.GetLocalNow().DateTime.AddHours(-_config.FileAgeThresholdHours))
        {
            return decision with { SkipReason = SkipReason.TooRecent };
        }

        if (_config.MinFileSizeKB > 0 && file.Length < _config.MinFileSizeKB * 1024)
            return decision with { SkipReason = SkipReason.TooSmall };

        if (!_categoriesByExtension.TryGetValue(file.Extension, out var category))
            return decision with { SkipReason = SkipReason.NoCategory };

        string destinationFolder = PathHelper.Expand(category.Destination);
        decision = decision with { CategoryName = category.Name, DestinationFolder = destinationFolder };

        if (destinationFolder.Length == 0 || PathHelper.AreSameFolder(destinationFolder, SourceFolder))
            return decision with { SkipReason = SkipReason.NoCategory };

        string? destinationName = ResolveDestinationName(destinationFolder, file.Name, reserved);
        if (destinationName is null)
            return decision with { SkipReason = SkipReason.DuplicateExists };

        string destinationPath = Path.Combine(destinationFolder, destinationName);
        reserved?.Add(destinationPath);
        return decision with { DestinationPath = destinationPath };
    }

    /// <summary>
    /// True when a file with this name could be organized (a category claims its extension and no exclude pattern
    /// matches). The file watcher uses this to notice a download being renamed from its temporary name.
    /// </summary>
    public bool IsCandidateName(string fileName) =>
        !IsExcluded(fileName) && _categoriesByExtension.ContainsKey(Path.GetExtension(fileName));

    private bool IsExcluded(string fileName) =>
        _config.ExcludePatterns.Any(pattern => FileSystemName.MatchesSimpleExpression(pattern, fileName, ignoreCase: true));

    /// <summary>The name to use in the destination folder, or null when the file should be skipped as a duplicate.</summary>
    private string? ResolveDestinationName(string folder, string fileName, HashSet<string>? reserved)
    {
        bool taken = File.Exists(Path.Combine(folder, fileName)) || (reserved?.Contains(Path.Combine(folder, fileName)) ?? false);
        if (!taken)
            return fileName;

        return _config.DuplicateHandling == DuplicateHandling.Skip ? null : FindFreeName(folder, fileName, reserved);
    }

    /// <summary>Returns fileName, or the first free "name_N.ext" in the folder.</summary>
    private static string? FindFreeName(string folder, string fileName, HashSet<string>? reserved)
    {
        bool IsFree(string name)
        {
            string path = Path.Combine(folder, name);
            return !File.Exists(path) && !Directory.Exists(path) && !(reserved?.Contains(path) ?? false);
        }

        if (IsFree(fileName))
            return fileName;

        string baseName = Path.GetFileNameWithoutExtension(fileName);
        string extension = Path.GetExtension(fileName);
        for (int i = 1; i <= MaxRenameAttempts; i++)
        {
            string candidate = $"{baseName}_{i}{extension}";
            if (IsFree(candidate))
                return candidate;
        }

        return null;
    }

    private void MoveOne(FileDecision decision, OrganizeResult result)
    {
        try
        {
            if (!File.Exists(decision.SourcePath))
            {
                result.Skipped.Add(decision with { SkipReason = SkipReason.NotFound });
                return;
            }

            string folder = decision.DestinationFolder!;
            Directory.CreateDirectory(folder);

            // Something may have arrived in the destination since the plan was made.
            string? name = Path.GetFileName(decision.DestinationPath!);
            if (File.Exists(Path.Combine(folder, name)))
            {
                name = _config.DuplicateHandling == DuplicateHandling.Skip
                    ? null
                    : FindFreeName(folder, decision.FileName, reserved: null);
            }

            if (name is null)
            {
                result.Skipped.Add(decision with { SkipReason = SkipReason.DuplicateExists });
                return;
            }

            string destination = Path.Combine(folder, name);
            try
            {
                File.Move(decision.SourcePath, destination, overwrite: false);
            }
            catch (FileNotFoundException)
            {
                // Moved or deleted by someone else (the file watcher, or the user) since the check above.
                result.Skipped.Add(decision with { SkipReason = SkipReason.NotFound });
                return;
            }

            result.Moved.Add(new MoveRecord
            {
                SourcePath = decision.SourcePath,
                DestinationPath = destination,
                FileName = decision.FileName,
                FileSize = decision.Size,
                Category = decision.CategoryName ?? string.Empty,
                MovedAt = _time.GetLocalNow().DateTime,
            });
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            result.Failed.Add(new MoveFailure(decision, ex.Message));
        }
    }
}

public sealed class OrganizeResult
{
    public List<MoveRecord> Moved { get; } = [];

    public List<FileDecision> Skipped { get; } = [];

    public List<MoveFailure> Failed { get; } = [];

    public bool Cancelled { get; set; }

    public long BytesMoved => Moved.Sum(m => m.FileSize);
}

public sealed class UndoResult
{
    public List<MoveRecord> Restored { get; } = [];

    /// <summary>Moves whose file is no longer at the destination (deleted or moved by the user).</summary>
    public List<MoveRecord> Missing { get; } = [];

    public List<(MoveRecord Move, string Error)> Failed { get; } = [];
}
