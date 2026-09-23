using System.Globalization;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Xml.Linq;
using TidyFlow.Core.Models;

namespace TidyFlow.Services;

/// <summary>Current state of TidyFlow's scheduled task, for display.</summary>
public sealed record ScheduledTaskStatus(bool Exists, DateTime? NextRun, string? Command)
{
    public static readonly ScheduledTaskStatus None = new(false, null, null);
}

/// <summary>
/// Creates, repairs and removes the "TidyFlow-AutoOrganize" task through the Task Scheduler COM API.
/// The task runs <c>TidyFlow.exe --run</c> (or the MSIX execution alias), with no console window,
/// on the calendar schedule and/or at sign-in.
/// </summary>
public static class TaskSchedulerService
{
    public const string TaskName = "TidyFlow-AutoOrganize";
    public const string RunArguments = "--run";

    private const int TaskCreateOrUpdate = 6;
    private const int TaskLogonInteractiveToken = 3;
    private const int ErrorFileNotFound = unchecked((int)0x80070002);

    private static readonly XNamespace Ns = "http://schemas.microsoft.com/windows/2004/02/mit/task";

    /// <summary>True when the settings call for a task at all.</summary>
    public static bool IsNeeded(ScheduleSettings schedule) => schedule.Enabled || schedule.RunAtSignIn;

    /// <summary>Creates or updates the task to match the settings, or removes it when neither trigger is wanted.</summary>
    public static void Apply(ScheduleSettings schedule)
    {
        if (!IsNeeded(schedule))
        {
            Remove();
            return;
        }

        string xml = BuildTaskXml(schedule, AppInfo.ScheduledTaskCommand, CurrentUser);
        WithRootFolder(folder =>
        {
            folder.RegisterTask(TaskName, xml, TaskCreateOrUpdate, null, null, TaskLogonInteractiveToken, null);
            return true;
        });
    }

    /// <summary>
    /// Run at startup: recreates the task when it is missing, or when it still points at an old location such as
    /// the 1.x PowerShell worker. Returns true when a repair was made.
    /// </summary>
    public static bool Repair(ScheduleSettings schedule)
    {
        var status = GetStatus();

        if (!IsNeeded(schedule))
        {
            if (status.Exists)
                Remove();
            return status.Exists;
        }

        if (status.Exists && string.Equals(status.Command, AppInfo.ScheduledTaskCommand, StringComparison.OrdinalIgnoreCase))
            return false;

        Apply(schedule);
        return true;
    }

    public static void Remove()
    {
        WithRootFolder(folder =>
        {
            try
            {
                folder.DeleteTask(TaskName, 0);
            }
            catch (Exception ex) when (IsTaskNotFound(ex))
            {
            }
            return true;
        });
    }

    public static ScheduledTaskStatus GetStatus()
    {
        try
        {
            return WithRootFolder(folder =>
            {
                dynamic task;
                try
                {
                    task = folder.GetTask(TaskName);
                }
                catch (Exception ex) when (IsTaskNotFound(ex))
                {
                    return ScheduledTaskStatus.None;
                }

                string? command = null;
                foreach (dynamic action in task.Definition.Actions)
                {
                    command = (string)action.Path;
                    break;
                }

                DateTime next = task.NextRunTime;
                return new ScheduledTaskStatus(true, next.Year > 1900 ? next : null, command);
            });
        }
        catch (Exception ex) when (ex is COMException or InvalidOperationException or UnauthorizedAccessException)
        {
            return ScheduledTaskStatus.None;
        }
    }

    /// <summary>Builds the Task Scheduler XML. Internal so it can be inspected in tests and diagnostics.</summary>
    internal static string BuildTaskXml(ScheduleSettings schedule, string command, string userId)
    {
        var triggers = new XElement(Ns + "Triggers");

        if (schedule.Enabled)
            triggers.Add(BuildCalendarTrigger(schedule));

        if (schedule.RunAtSignIn)
        {
            // A logon trigger without a user means "any user", which needs admin rights to register.
            triggers.Add(new XElement(Ns + "LogonTrigger",
                new XElement(Ns + "Enabled", "true"),
                new XElement(Ns + "UserId", userId),
                new XElement(Ns + "Delay", "PT1M")));
        }

        var doc = new XDocument(
            new XElement(Ns + "Task", new XAttribute("version", "1.2"),
                new XElement(Ns + "RegistrationInfo",
                    new XElement(Ns + "Description", "Organizes your files with TidyFlow."),
                    new XElement(Ns + "Author", "TidyFlow")),
                triggers,
                new XElement(Ns + "Principals",
                    new XElement(Ns + "Principal", new XAttribute("id", "Author"),
                        new XElement(Ns + "UserId", userId),
                        new XElement(Ns + "LogonType", "InteractiveToken"),
                        new XElement(Ns + "RunLevel", "LeastPrivilege"))),
                new XElement(Ns + "Settings",
                    new XElement(Ns + "MultipleInstancesPolicy", "IgnoreNew"),
                    new XElement(Ns + "DisallowStartIfOnBatteries", "false"),
                    new XElement(Ns + "StopIfGoingOnBatteries", "false"),
                    new XElement(Ns + "AllowHardTerminate", "true"),
                    // Catch up on a run missed while the PC was off or asleep.
                    new XElement(Ns + "StartWhenAvailable", "true"),
                    new XElement(Ns + "RunOnlyIfNetworkAvailable", "false"),
                    new XElement(Ns + "IdleSettings",
                        new XElement(Ns + "StopOnIdleEnd", "false"),
                        new XElement(Ns + "RestartOnIdle", "false")),
                    new XElement(Ns + "AllowStartOnDemand", "true"),
                    new XElement(Ns + "Enabled", "true"),
                    new XElement(Ns + "Hidden", "false"),
                    new XElement(Ns + "RunOnlyIfIdle", "false"),
                    new XElement(Ns + "WakeToRun", "false"),
                    new XElement(Ns + "ExecutionTimeLimit", "PT1H"),
                    new XElement(Ns + "Priority", "7")),
                new XElement(Ns + "Actions", new XAttribute("Context", "Author"),
                    new XElement(Ns + "Exec",
                        new XElement(Ns + "Command", command),
                        new XElement(Ns + "Arguments", RunArguments)))));

        return doc.ToString();
    }

    private static XElement BuildCalendarTrigger(ScheduleSettings schedule)
    {
        if (!ScheduleSettings.TryParseTime(schedule.Time, out var time))
            time = new TimeOnly(2, 0);
        string start = DateTime.Today.Add(time.ToTimeSpan()).ToString("yyyy-MM-dd'T'HH:mm:ss", CultureInfo.InvariantCulture);

        XElement scheduleElement = schedule.Frequency switch
        {
            ScheduleFrequency.Weekly => new XElement(Ns + "ScheduleByWeek",
                new XElement(Ns + "DaysOfWeek", new XElement(Ns + schedule.DayOfWeek.ToString())),
                new XElement(Ns + "WeeksInterval", "1")),

            ScheduleFrequency.Monthly => new XElement(Ns + "ScheduleByMonth",
                new XElement(Ns + "DaysOfMonth",
                    new XElement(Ns + "Day", schedule.DayOfMonth == 0 ? "Last" : schedule.DayOfMonth.ToString(CultureInfo.InvariantCulture))),
                new XElement(Ns + "Months",
                    CultureInfo.InvariantCulture.DateTimeFormat.MonthNames
                        .Where(m => m.Length > 0)
                        .Select(m => new XElement(Ns + m)))),

            _ => new XElement(Ns + "ScheduleByDay", new XElement(Ns + "DaysInterval", "1")),
        };

        return new XElement(Ns + "CalendarTrigger",
            new XElement(Ns + "StartBoundary", start),
            new XElement(Ns + "Enabled", "true"),
            scheduleElement);
    }

    private static string CurrentUser => WindowsIdentity.GetCurrent().Name;

    /// <summary>Late-bound COM calls surface "no such task" as FileNotFoundException rather than COMException.</summary>
    private static bool IsTaskNotFound(Exception ex) =>
        ex is FileNotFoundException || (ex is COMException com && com.HResult == ErrorFileNotFound);

    private static T WithRootFolder<T>(Func<dynamic, T> action)
    {
        var type = Type.GetTypeFromProgID("Schedule.Service")
            ?? throw new InvalidOperationException("The Windows Task Scheduler service isn't available.");

        dynamic service = Activator.CreateInstance(type)!;
        try
        {
            service.Connect();
            dynamic folder = service.GetFolder(@"\");
            return action(folder);
        }
        finally
        {
            Marshal.FinalReleaseComObject(service);
        }
    }
}
