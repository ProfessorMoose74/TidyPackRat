using TidyFlow.Core.Models;
using TidyFlow.Core.Organizing;

namespace TidyFlow.Core.Tests;

public sealed class OrganizerTests : IDisposable
{
    private readonly TempWorkspace _ws = new();
    private readonly TestClock _clock = new();

    public void Dispose() => _ws.Dispose();

    private Organizer CreateOrganizer(Action<AppConfiguration>? customize = null) => new(_ws.Config(customize), _clock);

    private static FileDecision For(IEnumerable<FileDecision> plan, string name) =>
        plan.Single(d => d.FileName == name);

    [Fact]
    public void Moves_files_into_their_category_folder()
    {
        _ws.CreateFile("report.pdf");
        _ws.CreateFile("photo.JPG");

        var organizer = CreateOrganizer();
        var result = organizer.Execute(organizer.Plan());

        Assert.Equal(2, result.Moved.Count);
        Assert.True(File.Exists(Path.Combine(_ws.Dest("Documents"), "report.pdf")));
        Assert.True(File.Exists(Path.Combine(_ws.Dest("Images"), "photo.JPG")));
        Assert.Empty(Directory.GetFiles(_ws.Source));
    }

    [Fact]
    public void Plan_does_not_move_anything()
    {
        _ws.CreateFile("report.pdf");

        var plan = CreateOrganizer().Plan();

        Assert.True(For(plan, "report.pdf").WillMove);
        Assert.True(File.Exists(Path.Combine(_ws.Source, "report.pdf")));
    }

    [Fact]
    public void Skips_recent_files_unless_the_age_threshold_is_ignored()
    {
        _ws.CreateFile("fresh.pdf", lastWrite: TestClock.Now.AddHours(-2));
        var organizer = CreateOrganizer();

        Assert.Equal(SkipReason.TooRecent, For(organizer.Plan(), "fresh.pdf").SkipReason);
        Assert.True(For(organizer.Plan(ignoreAgeThreshold: true), "fresh.pdf").WillMove);
    }

    [Fact]
    public void Zero_age_threshold_moves_everything()
    {
        _ws.CreateFile("fresh.pdf", lastWrite: TestClock.Now);

        var plan = CreateOrganizer(c => c.FileAgeThresholdHours = 0).Plan();

        Assert.True(For(plan, "fresh.pdf").WillMove);
    }

    [Fact]
    public void Minimum_size_is_measured_in_kilobytes()
    {
        // 1.x bug: the scheduled worker compared this value against bytes while the GUI said KB.
        _ws.CreateFile("small.pdf", sizeBytes: 1500);
        _ws.CreateFile("large.pdf", sizeBytes: 3000);

        var plan = CreateOrganizer(c => c.MinFileSizeKB = 2).Plan();

        Assert.Equal(SkipReason.TooSmall, For(plan, "small.pdf").SkipReason);
        Assert.True(For(plan, "large.pdf").WillMove);
    }

    [Theory]
    [InlineData("*.tmp", "download.tmp")]
    [InlineData("~*", "~lockfile.pdf")]
    [InlineData("*invoice*", "March-INVOICE-final.pdf")]
    [InlineData("scan-????.pdf", "scan-0042.pdf")]
    public void Exclude_patterns_support_full_wildcards(string pattern, string fileName)
    {
        // 1.x bug: the file watcher only understood a leading or trailing '*'.
        _ws.CreateFile(fileName);

        var plan = CreateOrganizer(c => c.ExcludePatterns = [pattern]).Plan();

        Assert.Equal(SkipReason.Excluded, For(plan, fileName).SkipReason);
    }

    [Fact]
    public void Skips_hidden_and_system_files_by_default()
    {
        _ws.CreateFile("hidden.pdf", attributes: FileAttributes.Hidden);
        _ws.CreateFile("system.txt", attributes: FileAttributes.System);

        var plan = CreateOrganizer().Plan();
        Assert.Equal(SkipReason.Hidden, For(plan, "hidden.pdf").SkipReason);
        Assert.Equal(SkipReason.Hidden, For(plan, "system.txt").SkipReason);

        var planIncludingHidden = CreateOrganizer(c => c.SkipHiddenFiles = false).Plan();
        Assert.True(For(planIncludingHidden, "hidden.pdf").WillMove);
    }

    [Fact]
    public void Ignores_disabled_categories_and_unknown_extensions()
    {
        _ws.CreateFile("backup.zip");
        _ws.CreateFile("mystery.xyz");

        var plan = CreateOrganizer().Plan();

        Assert.Equal(SkipReason.NoCategory, For(plan, "backup.zip").SkipReason);
        Assert.Equal(SkipReason.NoCategory, For(plan, "mystery.xyz").SkipReason);
    }

    [Fact]
    public void Never_touches_subfolders()
    {
        Directory.CreateDirectory(Path.Combine(_ws.Source, "Project"));
        File.WriteAllText(Path.Combine(_ws.Source, "Project", "notes.txt"), "x");

        var organizer = CreateOrganizer();
        organizer.Execute(organizer.Plan());

        Assert.True(File.Exists(Path.Combine(_ws.Source, "Project", "notes.txt")));
    }

    [Fact]
    public void Renames_when_the_destination_already_has_the_name()
    {
        Directory.CreateDirectory(_ws.Dest("Documents"));
        File.WriteAllText(Path.Combine(_ws.Dest("Documents"), "report.pdf"), "existing");
        _ws.CreateFile("report.pdf");

        var organizer = CreateOrganizer();
        var result = organizer.Execute(organizer.Plan());

        Assert.Equal(Path.Combine(_ws.Dest("Documents"), "report_1.pdf"), Assert.Single(result.Moved).DestinationPath);
        Assert.Equal("existing", File.ReadAllText(Path.Combine(_ws.Dest("Documents"), "report.pdf")));
    }

    [Fact]
    public void Planned_renames_never_collide_with_each_other()
    {
        Directory.CreateDirectory(_ws.Dest("Documents"));
        File.WriteAllText(Path.Combine(_ws.Dest("Documents"), "a.pdf"), "existing");
        _ws.CreateFile("a.pdf");
        _ws.CreateFile("a_1.pdf");

        var organizer = CreateOrganizer();
        var result = organizer.Execute(organizer.Plan());

        Assert.Equal(2, result.Moved.Count);
        Assert.Empty(result.Failed);
        Assert.Equal(3, Directory.GetFiles(_ws.Dest("Documents")).Length);
    }

    [Fact]
    public void Skip_strategy_leaves_duplicates_in_the_source_folder()
    {
        Directory.CreateDirectory(_ws.Dest("Documents"));
        File.WriteAllText(Path.Combine(_ws.Dest("Documents"), "report.pdf"), "existing");
        _ws.CreateFile("report.pdf");

        var organizer = CreateOrganizer(c => c.DuplicateHandling = DuplicateHandling.Skip);
        var result = organizer.Execute(organizer.Plan());

        Assert.Empty(result.Moved);
        Assert.Equal(SkipReason.DuplicateExists, Assert.Single(result.Skipped).SkipReason);
        Assert.True(File.Exists(Path.Combine(_ws.Source, "report.pdf")));
    }

    [Fact]
    public void Handles_a_file_disappearing_between_plan_and_execute()
    {
        string path = _ws.CreateFile("report.pdf");
        var organizer = CreateOrganizer();
        var plan = organizer.Plan();
        File.Delete(path);

        var result = organizer.Execute(plan);

        Assert.Empty(result.Moved);
        Assert.Empty(result.Failed);
        Assert.Equal(SkipReason.NotFound, Assert.Single(result.Skipped).SkipReason);
    }

    [Theory]
    [InlineData("report.pdf", true)]
    [InlineData("report.pdf.crdownload", false)]
    [InlineData("~lock.pdf", false)]
    [InlineData("backup.zip", false)]
    public void Knows_which_names_can_be_organized(string fileName, bool expected)
    {
        // The watcher only reacts to renames that turn a file into one of these (a finished download).
        Assert.Equal(expected, CreateOrganizer(c => c.ExcludePatterns = ["*.crdownload", "~*"]).IsCandidateName(fileName));
    }

    [Fact]
    public void Reports_a_missing_source_folder()
    {
        var organizer = CreateOrganizer(c => c.SourceFolder = Path.Combine(_ws.Root, "missing"));

        Assert.Throws<DirectoryNotFoundException>(() => organizer.Plan());
    }

    [Fact]
    public void Expands_environment_variables_in_paths()
    {
        Environment.SetEnvironmentVariable("TIDYFLOW_TEST_ROOT", _ws.Root);
        _ws.CreateFile("report.pdf");

        var organizer = CreateOrganizer(c =>
        {
            c.SourceFolder = @"%TIDYFLOW_TEST_ROOT%\Source";
            c.Categories[0].Destination = @"%TIDYFLOW_TEST_ROOT%\Dest\Documents";
        });
        organizer.Execute(organizer.Plan());

        Assert.True(File.Exists(Path.Combine(_ws.Dest("Documents"), "report.pdf")));
    }

    [Fact]
    public void Undo_restores_files_without_overwriting_newcomers()
    {
        _ws.CreateFile("report.pdf");
        _ws.CreateFile("photo.jpg");
        var organizer = CreateOrganizer();
        var batch = new MoveBatch();
        batch.Moves.AddRange(organizer.Execute(organizer.Plan()).Moved);

        // A new report.pdf lands in the source folder before the user hits Undo.
        File.WriteAllText(Path.Combine(_ws.Source, "report.pdf"), "newer download");

        var undo = Organizer.Undo(batch);

        Assert.Empty(undo.Failed.Select(f => f.Error));
        Assert.Empty(undo.Missing);
        Assert.Equal(2, undo.Restored.Count);
        Assert.True(batch.WasUndone);
        Assert.False(batch.CanUndo);
        Assert.Equal("newer download", File.ReadAllText(Path.Combine(_ws.Source, "report.pdf")));
        Assert.True(File.Exists(Path.Combine(_ws.Source, "report_1.pdf")));
        Assert.True(File.Exists(Path.Combine(_ws.Source, "photo.jpg")));
    }

    [Fact]
    public void Undo_reports_files_the_user_already_removed()
    {
        _ws.CreateFile("report.pdf");
        var organizer = CreateOrganizer();
        var batch = new MoveBatch();
        batch.Moves.AddRange(organizer.Execute(organizer.Plan()).Moved);
        File.Delete(batch.Moves[0].DestinationPath);

        var undo = Organizer.Undo(batch);

        Assert.Single(undo.Missing);
        Assert.Empty(undo.Restored);
        Assert.True(batch.WasUndone);
    }
}
