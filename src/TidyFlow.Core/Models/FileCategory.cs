namespace TidyFlow.Core.Models;

/// <summary>
/// A named group of file extensions and the folder files of that type are moved to.
/// </summary>
public sealed class FileCategory
{
    public string Name { get; set; } = string.Empty;

    /// <summary>Extensions including the leading dot, lower-case (".jpg").</summary>
    public List<string> Extensions { get; set; } = [];

    /// <summary>Destination folder. Environment variables are expanded.</summary>
    public string Destination { get; set; } = string.Empty;

    public bool Enabled { get; set; } = true;

    /// <summary>
    /// Normalizes user-entered extensions: trims, lower-cases, adds the leading dot and removes duplicates.
    /// Accepts comma, semicolon or whitespace separated input.
    /// </summary>
    public static List<string> ParseExtensions(string? input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return [];

        return input
            .Split([',', ';', ' ', '\t', '\r', '\n'], StringSplitOptions.RemoveEmptyEntries)
            .Select(NormalizeExtension)
            .Where(e => e.Length > 1)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    internal static string NormalizeExtension(string extension)
    {
        string trimmed = extension.Trim().TrimStart('*').ToLowerInvariant();
        return trimmed.StartsWith('.') ? trimmed : "." + trimmed;
    }
}
