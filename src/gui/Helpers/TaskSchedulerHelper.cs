using System;
using System.Diagnostics;
using System.IO;
using TidyPackRat.Models;

namespace TidyPackRat.Helpers
{
    /// <summary>
    /// Helper class for managing Windows Task Scheduler integration
    /// </summary>
    public static class TaskSchedulerHelper
    {
        private const string TaskName = "TidyPackRat-AutoOrganize";
        private const string TaskDescription = "TidyPackRat - Automated file organization";

        /// <summary>
        /// Creates or updates a scheduled task based on the configuration
        /// </summary>
        /// <param name="config">Configuration containing schedule settings</param>
        /// <param name="workerScriptPath">Path to the PowerShell worker script</param>
        /// <returns>True if successful, false otherwise</returns>
        public static bool CreateOrUpdateScheduledTask(AppConfiguration config, string workerScriptPath)
        {
            try
            {
                if (!config.Schedule.Enabled)
                {
                    // If scheduling is disabled, remove any existing task
                    RemoveScheduledTask();
                    return true;
                }

                // Build the PowerShell command
                string configPath = ConfigurationManager.DefaultConfigPath;
                string psCommand = $"-ExecutionPolicy Bypass -File \"{workerScriptPath}\" -ConfigPath \"{configPath}\"";

                // Determine trigger based on frequency
                string trigger = GetTriggerXml(config.Schedule);

                // Create task using schtasks.exe
                string taskXml = BuildTaskXml(psCommand, trigger, config.Schedule.Time);

                // Save XML to temp file
                string tempXmlPath = Path.Combine(Path.GetTempPath(), "tidypackrat_task.xml");
                File.WriteAllText(tempXmlPath, taskXml);

                try
                {
                    // Delete existing task if it exists
                    RemoveScheduledTask();

                    // Create new task from XML
                    var psi = new ProcessStartInfo
                    {
                        FileName = "schtasks.exe",
                        Arguments = $"/Create /TN \"{TaskName}\" /XML \"{tempXmlPath}\"",
                        UseShellExecute = false,
                        CreateNoWindow = true,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true
                    };

                    using (var process = Process.Start(psi))
                    {
                        process.WaitForExit();
                        return process.ExitCode == 0;
                    }
                }
                finally
                {
                    // Clean up temp file
                    if (File.Exists(tempXmlPath))
                    {
                        File.Delete(tempXmlPath);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Failed to create scheduled task: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Removes the TidyPackRat scheduled task if it exists
        /// </summary>
        /// <returns>True if successful or task doesn't exist, false on error</returns>
        public static bool RemoveScheduledTask()
        {
            try
            {
                var psi = new ProcessStartInfo
                {
                    FileName = "schtasks.exe",
                    Arguments = $"/Delete /TN \"{TaskName}\" /F",
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true
                };

                using (var process = Process.Start(psi))
                {
                    process.WaitForExit();
                    // Exit code 0 = success, 1 = task not found (also OK)
                    return process.ExitCode == 0 || process.ExitCode == 1;
                }
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Checks if the TidyPackRat scheduled task exists
        /// </summary>
        /// <returns>True if task exists, false otherwise</returns>
        public static bool TaskExists()
        {
            try
            {
                var psi = new ProcessStartInfo
                {
                    FileName = "schtasks.exe",
                    Arguments = $"/Query /TN \"{TaskName}\"",
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true
                };

                using (var process = Process.Start(psi))
                {
                    process.WaitForExit();
                    return process.ExitCode == 0;
                }
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Runs the scheduled task immediately
        /// </summary>
        /// <returns>True if successfully triggered, false otherwise</returns>
        public static bool RunTask()
        {
            try
            {
                var psi = new ProcessStartInfo
                {
                    FileName = "schtasks.exe",
                    Arguments = $"/Run /TN \"{TaskName}\"",
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true
                };

                using (var process = Process.Start(psi))
                {
                    process.WaitForExit();
                    return process.ExitCode == 0;
                }
            }
            catch
            {
                return false;
            }
        }

        private static string GetTriggerXml(ScheduleSettings schedule)
        {
            string[] timeParts = schedule.Time.Split(':');
            string hour = timeParts.Length > 0 ? timeParts[0].PadLeft(2, '0') : "02";
            string minute = timeParts.Length > 1 ? timeParts[1].PadLeft(2, '0') : "00";

            switch (schedule.Frequency.ToLower())
            {
                case "daily":
                    return $@"
      <CalendarTrigger>
        <StartBoundary>{DateTime.Now:yyyy-MM-dd}T{hour}:{minute}:00</StartBoundary>
        <Enabled>true</Enabled>
        <ScheduleByDay>
          <DaysInterval>1</DaysInterval>
        </ScheduleByDay>
      </CalendarTrigger>";

                case "weekly":
                    return $@"
      <CalendarTrigger>
        <StartBoundary>{DateTime.Now:yyyy-MM-dd}T{hour}:{minute}:00</StartBoundary>
        <Enabled>true</Enabled>
        <ScheduleByWeek>
          <DaysOfWeek>
            <Monday />
          </DaysOfWeek>
          <WeeksInterval>1</WeeksInterval>
        </ScheduleByWeek>
      </CalendarTrigger>";

                case "monthly":
                    return $@"
      <CalendarTrigger>
        <StartBoundary>{DateTime.Now:yyyy-MM-dd}T{hour}:{minute}:00</StartBoundary>
        <Enabled>true</Enabled>
        <ScheduleByMonth>
          <DaysOfMonth>
            <Day>1</Day>
          </DaysOfMonth>
          <Months>
            <January /><February /><March /><April /><May /><June />
            <July /><August /><September /><October /><November /><December />
          </Months>
        </ScheduleByMonth>
      </CalendarTrigger>";

                default:
                    goto case "daily";
            }
        }

        private static string BuildTaskXml(string psCommand, string trigger, string time)
        {
            return $@"<?xml version=""1.0"" encoding=""UTF-16""?>
<Task version=""1.2"" xmlns=""http://schemas.microsoft.com/windows/2004/02/mit/task"">
  <RegistrationInfo>
    <Description>{TaskDescription}</Description>
  </RegistrationInfo>
  <Triggers>
    {trigger}
  </Triggers>
  <Principals>
    <Principal id=""Author"">
      <LogonType>InteractiveToken</LogonType>
      <RunLevel>HighestAvailable</RunLevel>
    </Principal>
  </Principals>
  <Settings>
    <MultipleInstancesPolicy>IgnoreNew</MultipleInstancesPolicy>
    <DisallowStartIfOnBatteries>false</DisallowStartIfOnBatteries>
    <StopIfGoingOnBatteries>false</StopIfGoingOnBatteries>
    <AllowHardTerminate>true</AllowHardTerminate>
    <StartWhenAvailable>true</StartWhenAvailable>
    <RunOnlyIfNetworkAvailable>false</RunOnlyIfNetworkAvailable>
    <IdleSettings>
      <StopOnIdleEnd>false</StopOnIdleEnd>
      <RestartOnIdle>false</RestartOnIdle>
    </IdleSettings>
    <AllowStartOnDemand>true</AllowStartOnDemand>
    <Enabled>true</Enabled>
    <Hidden>false</Hidden>
    <RunOnlyIfIdle>false</RunOnlyIfIdle>
    <WakeToRun>false</WakeToRun>
    <ExecutionTimeLimit>PT1H</ExecutionTimeLimit>
    <Priority>7</Priority>
  </Settings>
  <Actions Context=""Author"">
    <Exec>
      <Command>powershell.exe</Command>
      <Arguments>{psCommand}</Arguments>
    </Exec>
  </Actions>
</Task>";
        }
    }
}
