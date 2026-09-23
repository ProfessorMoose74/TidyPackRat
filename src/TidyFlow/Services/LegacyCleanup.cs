using TidyFlow.Core.Storage;

namespace TidyFlow.Services;

/// <summary>Removes what 1.x left behind. Safe to run on every start.</summary>
public static class LegacyCleanup
{
    public static void Run(AppPaths paths)
    {
        foreach (string file in paths.LegacyFiles)
        {
            try
            {
                if (File.Exists(file))
                    File.Delete(file);
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
            {
            }
        }
    }
}
