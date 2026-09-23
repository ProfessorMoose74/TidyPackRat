using System.Text;
using System.Text.Json;

namespace TidyFlow.Core.Storage;

/// <summary>
/// JSON conventions shared by every TidyFlow data file, plus crash-safe writes.
/// </summary>
public static class JsonFile
{
    public static readonly JsonSerializerOptions Options = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        ReadCommentHandling = JsonCommentHandling.Skip,
        AllowTrailingCommas = true,
        WriteIndented = true,
        NumberHandling = System.Text.Json.Serialization.JsonNumberHandling.AllowNamedFloatingPointLiterals,
    };

    /// <summary>Reads and deserializes a file, returning null when it does not exist.</summary>
    public static T? Read<T>(string path) where T : class
    {
        if (!File.Exists(path))
            return null;

        string json = File.ReadAllText(path, Encoding.UTF8);
        return string.IsNullOrWhiteSpace(json) ? null : JsonSerializer.Deserialize<T>(json, Options);
    }

    /// <summary>
    /// Serializes to a temporary file and swaps it into place, so a crash or power cut never
    /// leaves a half-written file behind.
    /// </summary>
    public static void Write<T>(string path, T value)
    {
        string directory = Path.GetDirectoryName(Path.GetFullPath(path))!;
        Directory.CreateDirectory(directory);

        string tempPath = Path.Combine(directory, $".{Path.GetFileName(path)}.{Guid.NewGuid():N}.tmp");
        try
        {
            File.WriteAllText(tempPath, JsonSerializer.Serialize(value, Options), new UTF8Encoding(false));
            File.Move(tempPath, path, overwrite: true);
        }
        finally
        {
            if (File.Exists(tempPath))
            {
                try { File.Delete(tempPath); } catch (IOException) { }
            }
        }
    }
}
