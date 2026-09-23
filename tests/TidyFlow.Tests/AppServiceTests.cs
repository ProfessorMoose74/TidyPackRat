using System.Xml.Linq;
using TidyFlow.Core.Models;
using TidyFlow.Services;

namespace TidyFlow.Tests;

public sealed class TaskXmlTests
{
    private static readonly XNamespace Ns = "http://schemas.microsoft.com/windows/2004/02/mit/task";

    private static XDocument Build(ScheduleSettings schedule) =>
        XDocument.Parse(TaskSchedulerService.BuildTaskXml(schedule, @"C:\Apps\TidyFlow.exe", @"PC\alex"));

    [Fact]
    public void Runs_the_app_headless_instead_of_powershell()
    {
        var exec = Build(new ScheduleSettings { Enabled = true }).Descendants(Ns + "Exec").Single();

        Assert.Equal(@"C:\Apps\TidyFlow.exe", exec.Element(Ns + "Command")!.Value);
        Assert.Equal("--run", exec.Element(Ns + "Arguments")!.Value);
    }

    [Fact]
    public void Weekly_schedule_uses_the_chosen_day_and_time()
    {
        // 1.x always scheduled weekly runs for Monday.
        var doc = Build(new ScheduleSettings { Enabled = true, Frequency = ScheduleFrequency.Weekly, DayOfWeek = DayOfWeek.Thursday, Time = "18:45" });

        var days = doc.Descendants(Ns + "DaysOfWeek").Single().Elements().Select(e => e.Name.LocalName);
        Assert.Equal(["Thursday"], days);
        Assert.EndsWith("T18:45:00", doc.Descendants(Ns + "StartBoundary").Single().Value);
    }

    [Theory]
    [InlineData(15, "15")]
    [InlineData(0, "Last")]
    public void Monthly_schedule_uses_the_chosen_day(int dayOfMonth, string expected)
    {
        // 1.x always scheduled monthly runs for the 1st.
        var doc = Build(new ScheduleSettings { Enabled = true, Frequency = ScheduleFrequency.Monthly, DayOfMonth = dayOfMonth });

        Assert.Equal(expected, doc.Descendants(Ns + "Day").Single().Value);
        Assert.Equal(12, doc.Descendants(Ns + "Months").Single().Elements().Count());
    }

    [Fact]
    public void Sign_in_trigger_is_scoped_to_the_current_user()
    {
        // A logon trigger without a user needs admin rights to register.
        var doc = Build(new ScheduleSettings { Enabled = false, RunAtSignIn = true });

        var logon = doc.Descendants(Ns + "LogonTrigger").Single();
        Assert.Equal(@"PC\alex", logon.Element(Ns + "UserId")!.Value);
        Assert.Empty(doc.Descendants(Ns + "CalendarTrigger"));
    }

    [Fact]
    public void Task_is_needed_only_when_a_trigger_is_on()
    {
        Assert.False(TaskSchedulerService.IsNeeded(new ScheduleSettings()));
        Assert.True(TaskSchedulerService.IsNeeded(new ScheduleSettings { Enabled = true }));
        Assert.True(TaskSchedulerService.IsNeeded(new ScheduleSettings { RunAtSignIn = true }));
    }
}

public sealed class CommandLineTests
{
    [Fact]
    public void Recognizes_run_and_minimized_in_any_style()
    {
        Assert.True(CommandLine.Parse(["--run"]).Run);
        Assert.True(CommandLine.Parse(["/RUN"]).Run);
        Assert.True(CommandLine.Parse(["-minimized"]).Minimized);
        Assert.True(CommandLine.Parse(["--uninstall"]).Uninstall);
        Assert.False(CommandLine.Parse([]).Run);
    }

    [Fact]
    public void Picks_up_an_existing_settings_file_to_import()
    {
        string path = Path.Combine(Path.GetTempPath(), $"tidyflow-{Guid.NewGuid():N}.tfconfig");
        File.WriteAllText(path, "{}");
        try
        {
            Assert.Equal(path, CommandLine.Parse([path]).ImportFile);
            Assert.Null(CommandLine.Parse([path + ".missing.tfconfig"]).ImportFile);
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Fact]
    public void Ignores_notification_activation_arguments()
    {
        var parsed = CommandLine.Parse(["-ToastActivated"]);

        Assert.False(parsed.Run);
        Assert.Null(parsed.ImportFile);
    }
}
