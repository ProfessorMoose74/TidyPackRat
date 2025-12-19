using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Security.Cryptography;

namespace TidyFlow.Helpers
{
    /// <summary>
    /// Manages deployment of the worker script to a stable location that survives MSIX updates.
    ///
    /// Problem: MSIX packages install to version-specific paths like:
    ///   C:\Program Files\WindowsApps\ElementalGeniusLLC.TidyFlow_1.2.0.0_x64_...\
    /// When the app updates, this path changes, breaking any scheduled tasks that reference it.
    ///
    /// Solution: Deploy the worker script to a stable location (ProgramData) that never changes,
    /// and have scheduled tasks reference that stable path instead.
    /// </summary>
    public static class WorkerScriptDeployer
    {
        private const string WorkerScriptName = "TidyFlow-Worker.ps1";
        private const string DeploymentMetadataName = "worker-deployment.json";

        /// <summary>
        /// The stable path where the worker script is deployed for scheduling.
        /// This path never changes across app updates.
        /// </summary>
        public static readonly string StableWorkerPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
            "TidyFlow",
            WorkerScriptName);

        /// <summary>
        /// Directory containing deployed files
        /// </summary>
        public static readonly string DeploymentDirectory = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
            "TidyFlow");

        /// <summary>
        /// Path to deployment metadata file
        /// </summary>
        private static readonly string MetadataPath = Path.Combine(DeploymentDirectory, DeploymentMetadataName);

        /// <summary>
        /// Finds the worker script in the application package/directory.
        /// Searches multiple locations to support MSIX, MSI, and development scenarios.
        /// </summary>
        /// <returns>Path to the source worker script, or null if not found</returns>
        public static string FindPackagedWorkerScript()
        {
            var searchPaths = new[]
            {
                // MSIX/Store: Script is in app package root directory
                Path.Combine(AppDomain.CurrentDomain.BaseDirectory, WorkerScriptName),
                // Development: Script in worker subdirectory
                Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "worker", WorkerScriptName),
                // MSI installation: Program Files location
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles),
                    "TidyFlow", WorkerScriptName),
                // Development relative paths
                Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "worker", WorkerScriptName),
                Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "..", "worker", WorkerScriptName),
                Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "src", "worker", WorkerScriptName)
            };

            foreach (var path in searchPaths)
            {
                try
                {
                    string fullPath = Path.GetFullPath(path);
                    if (File.Exists(fullPath))
                    {
                        Debug.WriteLine($"[WorkerScriptDeployer] Found packaged worker script at: {fullPath}");
                        return fullPath;
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"[WorkerScriptDeployer] Error checking path {path}: {ex.Message}");
                }
            }

            Debug.WriteLine("[WorkerScriptDeployer] Could not find packaged worker script in any search location");
            return null;
        }

        /// <summary>
        /// Deploys the worker script from the app package to the stable location.
        /// Should be called on every app startup to ensure the deployed script is current.
        /// </summary>
        /// <returns>Deployment result with status and paths</returns>
        public static DeploymentResult DeployWorkerScript()
        {
            var result = new DeploymentResult();

            try
            {
                // Find the source script in the app package
                string sourceScript = FindPackagedWorkerScript();
                if (string.IsNullOrEmpty(sourceScript))
                {
                    result.Success = false;
                    result.ErrorMessage = "Worker script not found in application package";
                    result.SourcePath = null;
                    result.DeployedPath = StableWorkerPath;
                    return result;
                }

                result.SourcePath = sourceScript;
                result.DeployedPath = StableWorkerPath;

                // Ensure deployment directory exists
                if (!Directory.Exists(DeploymentDirectory))
                {
                    Directory.CreateDirectory(DeploymentDirectory);
                    Debug.WriteLine($"[WorkerScriptDeployer] Created deployment directory: {DeploymentDirectory}");
                }

                // Check if deployment is needed
                bool needsDeploy = NeedsDeployment(sourceScript);

                if (needsDeploy)
                {
                    // Copy the script to stable location
                    File.Copy(sourceScript, StableWorkerPath, overwrite: true);
                    Debug.WriteLine($"[WorkerScriptDeployer] Deployed worker script to: {StableWorkerPath}");

                    // Update deployment metadata
                    SaveDeploymentMetadata(sourceScript);
                    result.WasUpdated = true;
                }
                else
                {
                    Debug.WriteLine("[WorkerScriptDeployer] Deployed script is current, no update needed");
                    result.WasUpdated = false;
                }

                result.Success = true;
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.ErrorMessage = $"Failed to deploy worker script: {ex.Message}";
                Debug.WriteLine($"[WorkerScriptDeployer] Deployment failed: {ex}");
            }

            return result;
        }

        /// <summary>
        /// Checks if the worker script needs to be deployed or updated.
        /// </summary>
        private static bool NeedsDeployment(string sourceScript)
        {
            // If deployed script doesn't exist, definitely need to deploy
            if (!File.Exists(StableWorkerPath))
            {
                Debug.WriteLine("[WorkerScriptDeployer] Deployed script doesn't exist, deployment needed");
                return true;
            }

            // Compare file hashes to detect changes
            try
            {
                string sourceHash = ComputeFileHash(sourceScript);
                string deployedHash = ComputeFileHash(StableWorkerPath);

                if (sourceHash != deployedHash)
                {
                    Debug.WriteLine("[WorkerScriptDeployer] Script hashes differ, deployment needed");
                    return true;
                }

                // Also check app version from metadata
                var metadata = LoadDeploymentMetadata();
                string currentVersion = GetAppVersion();

                if (metadata == null || metadata.AppVersion != currentVersion)
                {
                    Debug.WriteLine($"[WorkerScriptDeployer] App version changed ({metadata?.AppVersion} -> {currentVersion}), deployment needed");
                    return true;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[WorkerScriptDeployer] Error checking deployment status: {ex.Message}, forcing deployment");
                return true;
            }

            return false;
        }

        /// <summary>
        /// Computes SHA256 hash of a file for comparison
        /// </summary>
        private static string ComputeFileHash(string filePath)
        {
            using (var sha256 = SHA256.Create())
            using (var stream = File.OpenRead(filePath))
            {
                byte[] hash = sha256.ComputeHash(stream);
                return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
            }
        }

        /// <summary>
        /// Gets the current application version
        /// </summary>
        private static string GetAppVersion()
        {
            try
            {
                var assembly = Assembly.GetExecutingAssembly();
                var version = assembly.GetName().Version;
                return version?.ToString() ?? "1.0.0.0";
            }
            catch
            {
                return "1.0.0.0";
            }
        }

        /// <summary>
        /// Saves deployment metadata to track what was deployed
        /// </summary>
        private static void SaveDeploymentMetadata(string sourceScript)
        {
            try
            {
                var metadata = new DeploymentMetadata
                {
                    AppVersion = GetAppVersion(),
                    DeployedAt = DateTime.UtcNow,
                    SourcePath = sourceScript,
                    ScriptHash = ComputeFileHash(sourceScript)
                };

                string json = Newtonsoft.Json.JsonConvert.SerializeObject(metadata, Newtonsoft.Json.Formatting.Indented);
                File.WriteAllText(MetadataPath, json);
                Debug.WriteLine($"[WorkerScriptDeployer] Saved deployment metadata");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[WorkerScriptDeployer] Failed to save deployment metadata: {ex.Message}");
            }
        }

        /// <summary>
        /// Loads deployment metadata from previous deployment
        /// </summary>
        private static DeploymentMetadata LoadDeploymentMetadata()
        {
            try
            {
                if (!File.Exists(MetadataPath))
                    return null;

                string json = File.ReadAllText(MetadataPath);
                return Newtonsoft.Json.JsonConvert.DeserializeObject<DeploymentMetadata>(json);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[WorkerScriptDeployer] Failed to load deployment metadata: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Validates that the deployed worker script exists and is accessible
        /// </summary>
        /// <returns>True if the deployed script is ready to use</returns>
        public static bool IsDeploymentValid()
        {
            if (!File.Exists(StableWorkerPath))
                return false;

            try
            {
                // Try to read the file to verify access
                using (var stream = File.OpenRead(StableWorkerPath))
                {
                    return stream.Length > 0;
                }
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Gets the best available worker script path for execution.
        /// Prefers the stable deployed path, falls back to packaged path.
        /// </summary>
        public static string GetExecutionPath()
        {
            // Prefer the stable deployed path
            if (File.Exists(StableWorkerPath))
                return StableWorkerPath;

            // Fall back to packaged path
            return FindPackagedWorkerScript();
        }

        /// <summary>
        /// Gets the stable path that should be used for scheduled tasks.
        /// This path is guaranteed to be stable across app updates.
        /// </summary>
        public static string GetSchedulingPath()
        {
            return StableWorkerPath;
        }
    }

    /// <summary>
    /// Result of a worker script deployment operation
    /// </summary>
    public class DeploymentResult
    {
        public bool Success { get; set; }
        public bool WasUpdated { get; set; }
        public string SourcePath { get; set; }
        public string DeployedPath { get; set; }
        public string ErrorMessage { get; set; }
    }

    /// <summary>
    /// Metadata about the deployed worker script
    /// </summary>
    public class DeploymentMetadata
    {
        public string AppVersion { get; set; }
        public DateTime DeployedAt { get; set; }
        public string SourcePath { get; set; }
        public string ScriptHash { get; set; }
    }
}
