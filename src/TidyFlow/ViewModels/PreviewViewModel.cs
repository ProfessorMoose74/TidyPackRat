using System.ComponentModel;
using System.Windows.Data;
using CommunityToolkit.Mvvm.ComponentModel;
using TidyFlow.Core.Organizing;

namespace TidyFlow.ViewModels;

/// <summary>The "Preview changes" window: what a run would do, before it does it.</summary>
public sealed partial class PreviewViewModel : ObservableObject
{
    public PreviewViewModel(string sourceFolder, IReadOnlyList<FileDecision> decisions)
    {
        SourceFolder = sourceFolder;
        Decisions = decisions;
        Rows = decisions.Select(d => new PreviewRow(d)).ToList();
        MoveCount = decisions.Count(d => d.WillMove);
        SkipCount = decisions.Count - MoveCount;

        View = CollectionViewSource.GetDefaultView(Rows);
        View.Filter = row => ShowSkipped || ((PreviewRow)row).WillMove;
        View.SortDescriptions.Add(new SortDescription(nameof(PreviewRow.SortKey), ListSortDirection.Ascending));
    }

    public string SourceFolder { get; }

    public IReadOnlyList<FileDecision> Decisions { get; }

    public IReadOnlyList<PreviewRow> Rows { get; }

    public ICollectionView View { get; }

    public int MoveCount { get; }

    public int SkipCount { get; }

    public bool HasMoves => MoveCount > 0;

    public string Heading => MoveCount switch
    {
        0 => "Nothing to organize right now",
        1 => "1 file will be moved",
        _ => $"{MoveCount} files will be moved",
    };

    public string SubHeading => SkipCount == 0
        ? $"From {SourceFolder}"
        : $"From {SourceFolder}. {SkipCount} {(SkipCount == 1 ? "file stays" : "files stay")} where {(SkipCount == 1 ? "it is" : "they are")}.";

    public string OrganizeButtonText => MoveCount == 1 ? "Move 1 file" : $"Move {MoveCount} files";

    [ObservableProperty]
    public partial bool ShowSkipped { get; set; }

    partial void OnShowSkippedChanged(bool value) => View.Refresh();
}

public sealed class PreviewRow(FileDecision decision)
{
    public string FileName => decision.FileName;

    public string Action => decision.Describe();

    public string Destination => decision.DestinationPath ?? string.Empty;

    public string Size => Core.Models.Statistics.FormatBytes(decision.Size);

    public bool WillMove => decision.WillMove;

    /// <summary>Moves first, then skips, alphabetically within each.</summary>
    public string SortKey => (WillMove ? "0" : "1") + FileName;
}
