using TidyFlow.Core.Models;

namespace TidyFlow.Core.Storage;

/// <summary>
/// The .tfconfig export format: configuration plus preferences in one file.
/// Reads files exported by 1.x as well.
/// </summary>
public sealed class SettingsBundle
{
    public const string FileExtension = ".tfconfig";

    public string ExportVersion { get; set; } = "2.0";

    public DateTime ExportDate { get; set; } = DateTime.Now;

    public AppConfiguration? Configuration { get; set; }

    public UserPreferences? Preferences { get; set; }

    public static void Export(string path, AppConfiguration configuration, UserPreferences preferences) =>
        JsonFile.Write(path, new SettingsBundle { Configuration = configuration, Preferences = preferences });

    /// <summary>Reads a bundle and migrates whatever it contains. Throws if the file has no configuration.</summary>
    public static SettingsBundle Import(string path)
    {
        SettingsBundle? bundle;
        try
        {
            bundle = JsonFile.Read<SettingsBundle>(path);
        }
        catch (System.Text.Json.JsonException ex)
        {
            throw new ConfigurationException("This isn't a valid TidyFlow settings file.", ex);
        }

        if (bundle?.Configuration is null)
            throw new ConfigurationException("This file doesn't contain a TidyFlow configuration.");

        ConfigurationStore.Normalize(bundle.Configuration);
        bundle.Preferences?.Migrate();
        return bundle;
    }
}
