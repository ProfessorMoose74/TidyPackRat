using System;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using TidyFlow.Models;

namespace TidyFlow.Helpers
{
    /// <summary>
    /// Helper class for managing Windows Task Scheduler integration.
    ///
    /// This class handles creating, updating, and repairing scheduled tasks for TidyFlow.
    /// It uses the stable deployed worker script path to ensure tasks survive app updates.
    ///
    /// Self-healing: On startup, call ValidateAndRepairTask() to detect and fix
    /// any tasks that may have been broken by app updates or path changes.
    /// </summary>
    public static class TaskSchedulerHelper
    {
        private const string TaskName = "TidyFlow-AutoOrganize";
        private const string TaskDescription = "TidyFlow - Automated file organization";

        /// <summary>
        /// Result of a task scheduler operation
        /// </summary>
        public class TaskOperationResult
        {
            public bool Success { get; set; }
            public string Message { get; set; }
            public bool WasRepaired { get; set; }
        }

        /// <summary>
        /// Creates or updates a scheduled task based on the configuration.
        /// Always uses the stable deployed worker script path to survive app updates.
        /// </summary>
        /// <param name="config">Configuration containing schedule settings</param>
        /// <returns>Result of the operation</returns>
        public static TaskOperationResult CreateOrUpdateScheduledTask(AppConfiguration config)
        {
            var result = new TaskOperationResult();

            try
            {
                if (!config.Schedule.Enabled)
                {
                    // If scheduling is disabled, remove any existing task
                    RemoveScheduledTask();
                    result.Success = true;
                    result.Message = "Scheduled task removed (scheduling disabled)";
                    return result;
                }

                // Always use the stable deployed path for scheduling
                string workerScriptPath = WorkerScriptDeployer.GetSchedulingPath();

                // Verify the deployed script exists
                if (!File.Exists(workerScriptPath))
                {
                    // Try to deploy it first
                    var deployResult = WorkerScriptDeployer.DeployWorkerScript();
                    if (!deployResult.Success)
                    {
                        result.Success = false;
                        result.Message = $"Cannot create scheduled task: {deployResult.ErrorMessage}";
                        return result;
                    }
                }

                // Build the PowerShell command using stable paths
                string configPath = ConfigurationManager.DefaultConfigPath;
                string psCommand = $"-ExecutionPolicy Bypass -NoProfile -NonInteractive -File \"{workerScriptPath}\" -ConfigPath \"{configPath}\"";

                // Determine trigger based on frequency
                string trigger = GetTriggerXml(config.Schedule);

                // Create task using schtasks.exe
                string taskXml = BuildTaskXml(psCommand, trigger);

                // Save XML to temp file with unique name to prevent race conditions
                string tempXmlPath = Path.Combine(Path.GetTempPath(), $"tidyflow_task_{Guid.NewGuid():N}.xml");
                File.WriteAllText(tempXmlPath, taskXml);

                try
                {
                    // Delete existing task if it exists
                    RemoveScheduledTask();

                    // Create new task from XML
                    var createResult = RunSchtasks($"/Create /TN \"{TaskName}\" /XML \"{tempXmlPath}\"");

                    if (createResult.ExitCode == 0)
                    {
                        result.Success = true;
                        result.Message = $"Scheduled task created successfully ({config.Schedule.Frequency} at {config.Schedule.Time})";
                        Debug.WriteLine($"[TaskSchedulerHelper] Created task with worker path: {workerScriptPath}");
                    }
                    else
                    {
                        result.Success = false;
                        result.Message = $"Failed to create scheduled task: {createResult.Error}";
                    }
                }
                finally
                {
                    // Clean up temp file
                    if (File.Exists(tempXmlPath))
                    {
                        try { File.Delete(tempXmlPath); } catch { }
                    }
                }
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.Message = $"Error creating scheduled task: {ex.Message}";
                Debug.WriteLine($"[TaskSchedulerHelper] Exception: {ex}");
            }

            return result;
        }

        /// <summary>
        /// Validates the existing scheduled task and repairs it if necessary.
        /// Call this on app startup to self-heal broken tasks after updates.
        /// </summary>
        /// <param name="config">Current configuration</param>
        /// <returns>Result of the validation/repair operation</returns>
        public static TaskOperationResult ValidateAndRepairTask(AppConfiguration config)
        {
            var result = new TaskOperationResult { Success = true };

            try
            {
                // If scheduling is disabled, nothing to validate
                if (config?.Schedule?.Enabled != true)
                {
                    result.Message = "Scheduling disabled, no validation needed";
                    return result;
                }

                // Check if task exists
                if (!TaskExists())
                {
                    // Task should exist but doesn't - recreate it
                    Debug.WriteLine("[TaskSchedulerHelper] Scheduled task missing, recreating...");
                    var createResult = CreateOrUpdateScheduledTask(config);
                    createResult.WasRepaired = createResult.Success;
                    return createResult;
                }

                // Get current task configuration
                var taskInfo = GetTaskInfo();
                if (taskInfo == null)
                {
                    result.Message = "Could not query task info";
                    return result;
                }

                // Check if the task references the correct (stable) worker path
                string expectedPath = WorkerScriptDeployer.GetSchedulingPath();

                if (!taskInfo.Arguments.Contains(expectedPath))
                {
                    Debug.WriteLine($"[TaskSchedulerHelper] Task references wrong path, repairing...");
                    Debug.WriteLine($"[TaskSchedulerHelper] Expected: {expectedPath}");
                    Debug.WriteLine($"[TaskSchedulerHelper] Found: {taskInfo.Arguments}");

                    // Recreate the task with correct path
                    var repairResult = CreateOrUpdateScheduledTask(config);
                    repairResult.WasRepaired = repairResult.Success;
                    repairResult.Message = repairResult.Success
                        ? "Scheduled task repaired (updated path after app update)"
                        : repairResult.Message;
                    return repairResult;
                }

                // Verify the referenced worker script exists
                if (!File.Exists(expectedPath))
                {
                    Debug.WriteLine("[TaskSchedulerHelper] Worker script missing, deploying and repairing task...");

                    var deployResult = WorkerScriptDeployer.DeployWorkerScript();
                    if (!deployResult.Success)
                    {
                        result.Success = false;
                        result.Message = $"Cannot repair: {deployResult.ErrorMessage}";
                        return result;
                    }

                    // Recreate task to ensure it's properly configured
                    var repairResult = CreateOrUpdateScheduledTask(config);
                    repairResult.WasRepaired = repairResult.Success;
                    return repairResult;
                }

                result.Message = "Scheduled task is valid";
                Debug.WriteLine("[TaskSchedulerHelper] Task validation passed");
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.Message = $"Validation error: {ex.Message}";
                Debug.WriteLine($"[TaskSchedulerHelper] Validation exception: {ex}");
            }

            return result;
        }

        /// <summary>
        /// Removes the TidyFlow scheduled task if it exists
        /// </summary>
        /// <returns>True if successful or task doesn't exist, false on error</returns>
        public static bool RemoveScheduledTask()
        {
            try
            {
                var result = RunSchtasks($"/Delete /TN \"{TaskName}\" /F");
                // Exit code 0 = success, 1 = task not found (also OK)
                return result.ExitCode == 0 || result.ExitCode == 1;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[TaskSchedulerHelper] Failed to remove scheduled task: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Checks if the TidyFlow scheduled task exists
        /// </summary>
        /// <returns>True if task exists, false otherwise</returns>
        public static bool TaskExists()
        {
            try
            {
                var result = RunSchtasks($"/Query /TN \"{TaskName}\"");
                return result.ExitCode == 0;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[TaskSchedulerHelper] Failed to query scheduled task: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Gets information about the current scheduled task
        /// </summary>
        private static TaskInfo GetTaskInfo()
        {
            try
            {
                var result = RunSchtasks($"/Query /TN \"{TaskName}\" /XML");
                if (result.ExitCode != 0)
                    return null;

                // Parse the XML to extract the command arguments
                var info = new TaskInfo { RawXml = result.Output };

                // Extract arguments using regex
                var argsMatch = Regex.Match(result.Output, @"<Arguments>([^<]+)</Arguments>");
                if (argsMatch.Success)
                {
                    info.Arguments = argsMatch.Groups[1].Value;
                }

                var commandMatch = Regex.Match(result.Output, @"<Command>([^<]+)</Command>");
                if (commandMatch.Success)
                {
                    info.Command = commandMatch.Groups[1].Value;
                }

                return info;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[TaskSchedulerHelper] Failed to get task info: {ex.Message}");
                return null;
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
                var result = RunSchtasks($"/Run /TN \"{TaskName}\"");
                return result.ExitCode == 0;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[TaskSchedulerHelper] Failed to run scheduled task: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Validates and parses a time string in HH:mm format
        /// </summary>
        /// <param name="timeString">Time string to validate</param>
        /// <param name="hour">Parsed hour (00-23)</param>
        /// <param name="minute">Parsed minute (00-59)</param>
        /// <returns>True if valid, false otherwise</returns>
        public static bool TryParseTime(string timeString, out string hour, out string minute)
        {
            hour = "02";
            minute = "00";

            if (string.IsNullOrWhiteSpace(timeString))
                return false;

            string[] timeParts = timeString.Split(':');
            if (timeParts.Length != 2)
                return false;

            if (!int.TryParse(timeParts[0], out int hourValue) || hourValue < 0 || hourValue > 23)
                return false;

            if (!int.TryParse(timeParts[1], out int minuteValue) || minuteValue < 0 || minuteValue > 59)
                return false;

            hour = hourValue.ToString("D2");
            minute = minuteValue.ToString("D2");
            return true;
        }

        /// <summary>
        /// Runs schtasks.exe with the given arguments
        /// </summary>
        private static SchtasksResult RunSchtasks(string arguments)
        {
            var psi = new ProcessStartInfo
            {
                FileName = "schtasks.exe",
                Arguments = arguments,
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            };

            using (var process = Process.Start(psi))
            {
                string output = process.StandardOutput.ReadToEnd();
                string error = process.StandardError.ReadToEnd();
                process.WaitForExit();

                return new SchtasksResult
                {
                    ExitCode = process.ExitCode,
                    Output = output,
                    Error = error
                };
            }
        }

        private static string GetTriggerXml(ScheduleSettings schedule)
        {
            // Validate and parse time with bounds checking
            if (!TryParseTime(schedule.Time, out string hour, out string minute))
            {
                Debug.WriteLine($"[TaskSchedulerHelper] Invalid time format '{schedule.Time}', using default 02:00");
                hour = "02";
                minute = "00";
            }

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

        private static string BuildTaskXml(string psCommand, string trigger)
        {
            // Escape XML special characters in the command
            string escapedCommand = System.Security.SecurityElement.Escape(psCommand);

            return $@"<?xml version=""1.0"" encoding=""UTF-16""?>
<Task version=""1.2"" xmlns=""http://schemas.microsoft.com/windows/2004/02/mit/task"">
  <RegistrationInfo>
    <Description>{TaskDescription}</Description>
    <Author>TidyFlow</Author>
  </RegistrationInfo>
  <Triggers>
    {trigger}
  </Triggers>
  <Principals>
    <Principal id=""Author"">
      <LogonType>InteractiveToken</LogonType>
      <RunLevel>LeastPrivilege</RunLevel>
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
      <Arguments>{escapedCommand}</Arguments>
    </Exec>
  </Actions>
</Task>";
        }

        /// <summary>
        /// Internal class to hold schtasks execution results
        /// </summary>
        private class SchtasksResult
        {
            public int ExitCode { get; set; }
            public string Output { get; set; }
            public string Error { get; set; }
        }

        /// <summary>
        /// Internal class to hold task information
        /// </summary>
        private class TaskInfo
        {
            public string Command { get; set; }
            public string Arguments { get; set; }
            public string RawXml { get; set; }
        }
    }
}
