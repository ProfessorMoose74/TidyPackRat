using System.Text.Json;
using TidyFlow.Core.Models;

namespace TidyFlow.Core.Storage;

/// <summary>
/// Loads and saves preferences.json. Preferences are never worth crashing over, so read errors fall back to defaults.
/// </summary>
public sealed class PreferencesStore(AppPaths paths)
{
    public UserPreferences Load()
    {
        try
        {
            var prefs = JsonFile.Read<UserPreferences>(paths.PreferencesFile) ?? new UserPreferences();
            prefs.Migrate();
            return prefs;
        }
        catch (Exception ex) when (ex is JsonException or IOException or UnauthorizedAccessException)
        {
            return new UserPreferences();
        }
    }

    public void Save(UserPreferences preferences) => JsonFile.Write(paths.PreferencesFile, preferences);
}
