namespace TidyFlow.Core.Storage;

public static class PathHelper
{
    /// <summary>Expands environment variables and normalizes the path; returns empty for blank input.</summary>
    public static string Expand(string? path)
    {
        if (string.IsNullOrWhiteSpace(path))
            return string.Empty;

        string expanded = Environment.ExpandEnvironmentVariables(path.Trim());
        try
        {
            return Path.TrimEndingDirectorySeparator(Path.GetFullPath(expanded));
        }
        catch (Exception ex) when (ex is ArgumentException or NotSupportedException or PathTooLongException)
        {
            return expanded;
        }
    }

    public static bool AreSameFolder(string? a, string? b) =>
        string.Equals(Expand(a), Expand(b), StringComparison.OrdinalIgnoreCase);

    /// <summary>
    /// Folders TidyFlow must never organize from: drive roots, Windows, Program Files and the user profile root.
    /// </summary>
    public static bool IsProtectedFolder(string? path)
    {
        string full = Expand(path);
        if (full.Length == 0)
            return true;

        string? root = Path.GetPathRoot(full);
        if (root is not null && string.Equals(Path.TrimEndingDirectorySeparator(root), full, StringComparison.OrdinalIgnoreCase))
            return true;

        Environment.SpecialFolder[] protectedFolders =
        [
            Environment.SpecialFolder.Windows,
            Environment.SpecialFolder.System,
            Environment.SpecialFolder.SystemX86,
            Environment.SpecialFolder.ProgramFiles,
            Environment.SpecialFolder.ProgramFilesX86,
            Environment.SpecialFolder.UserProfile,
        ];

        return protectedFolders
            .Select(Environment.GetFolderPath)
            .Where(p => !string.IsNullOrEmpty(p))
            .Any(p => AreSameFolder(p, full));
    }
}
