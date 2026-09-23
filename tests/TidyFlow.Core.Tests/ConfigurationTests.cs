using TidyFlow.Core.Models;
using TidyFlow.Core.Storage;

namespace TidyFlow.Core.Tests;

public sealed class ConfigurationTests : IDisposable
{
    private readonly TempWorkspace _ws = new();

    public void Dispose() => _ws.Dispose();

    /// <summary>A config.json exactly as 1.2.7 wrote it, including values 2.0 has to migrate.</summary>
    private const string LegacyConfigJson = """
        {
          "appName": "TidyFlow",
          "version": "1.2.7",
          "sourceFolder": "%USERPROFILE%\\Downloads",
          "fileAgeThreshold": 48,
          "fileSizeThreshold": 500,
          "duplicateHandling": "ask",
          "categories": [
            { "name": "Images", "extensions": ["JPG", ".Png ", "*.gif"], "destination": "%USERPROFILE%\\Pictures", "enabled": true }
          ],
          "schedule": { "enabled": true, "frequency": "WEEKLY", "time": "7:30", "runOnStartup": true },
          "excludePatterns": ["*.tmp", "", "*.TMP"],
          "logging": { "enabled": true, "logPath": "%PROGRAMDATA%\\TidyFlow\\logs", "logLevel": "info", "maxLogFiles": 12 }
        }
        """;

    [Fact]
    public void Default_configuration_is_valid()
    {
        var config = ConfigurationStore.CreateDefault();

        Assert.Empty(ConfigurationStore.Validate(config));
        Assert.Contains(config.Categories, c => c.Name == "Images" && c.Extensions.Contains(".heic"));
        Assert.Equal(AppConfiguration.CurrentSchemaVersion, config.SchemaVersion);
    }

    [Fact]
    public void Missing_file_loads_defaults()
    {
        var config = new ConfigurationStore(_ws.Paths).Load();

        Assert.Equal(@"%USERPROFILE%\Downloads", config.SourceFolder);
        Assert.NotEmpty(config.Categories);
    }

    [Fact]
    public void Migrates_a_1x_configuration()
    {
        File.WriteAllText(_ws.Paths.ConfigFile, LegacyConfigJson);

        var config = new ConfigurationStore(_ws.Paths).Load();

        Assert.Equal(48, config.FileAgeThresholdHours);
        Assert.Equal(500, config.MinFileSizeKB);
        Assert.Null(config.LegacyFileSizeThreshold);
        Assert.Equal(DuplicateHandling.Rename, config.DuplicateHandling);
        Assert.Equal([".jpg", ".png", ".gif"], config.Categories[0].Extensions);
        Assert.Equal(ScheduleFrequency.Weekly, config.Schedule.Frequency);
        Assert.Equal("07:30", config.Schedule.Time);
        Assert.True(config.Schedule.RunAtSignIn);
        Assert.Equal(["*.tmp"], config.ExcludePatterns);
        Assert.Equal(string.Empty, config.Logging.LogPath);
    }

    [Fact]
    public void Saved_file_uses_the_new_schema_and_round_trips()
    {
        var store = new ConfigurationStore(_ws.Paths);
        var config = _ws.Config(c =>
        {
            c.MinFileSizeKB = 64;
            c.Schedule = new ScheduleSettings { Enabled = true, Frequency = ScheduleFrequency.Monthly, DayOfMonth = 0, Time = "23:15" };
        });

        store.Save(config);
        string json = File.ReadAllText(_ws.Paths.ConfigFile);
        var reloaded = store.Load();

        Assert.Contains("\"minFileSizeKB\": 64", json);
        Assert.Contains("\"frequency\": \"monthly\"", json);
        Assert.DoesNotContain("fileSizeThreshold", json);
        Assert.Equal(64, reloaded.MinFileSizeKB);
        Assert.Equal(0, reloaded.Schedule.DayOfMonth);
        Assert.Equal("23:15", reloaded.Schedule.Time);
    }

    [Fact]
    public void Falls_back_to_the_backup_when_the_file_is_corrupt()
    {
        var store = new ConfigurationStore(_ws.Paths);
        store.Save(_ws.Config(c => c.FileAgeThresholdHours = 7));
        store.Save(_ws.Config(c => c.FileAgeThresholdHours = 8));
        File.WriteAllText(_ws.Paths.ConfigFile, "{ not json");

        Assert.Equal(7, store.Load().FileAgeThresholdHours);
    }

    [Fact]
    public void Throws_when_neither_file_is_readable()
    {
        File.WriteAllText(_ws.Paths.ConfigFile, "{ not json");

        Assert.Throws<ConfigurationException>(() => new ConfigurationStore(_ws.Paths).Load());
    }

    [Fact]
    public void Refuses_to_save_an_invalid_configuration()
    {
        var config = _ws.Config(c => c.Categories[0].Destination = c.SourceFolder);

        var ex = Assert.Throws<ConfigurationException>(() => new ConfigurationStore(_ws.Paths).Save(config));

        Assert.Contains("source folder itself", ex.Message);
        Assert.False(File.Exists(_ws.Paths.ConfigFile));
    }

    [Theory]
    [InlineData(@"C:\")]
    [InlineData(@"%WINDIR%")]
    [InlineData(@"%USERPROFILE%")]
    [InlineData("")]
    public void Rejects_protected_source_folders(string folder)
    {
        var errors = ConfigurationStore.Validate(_ws.Config(c => c.SourceFolder = folder));

        Assert.NotEmpty(errors);
    }

    [Theory]
    [InlineData("02:00", true)]
    [InlineData("7:05", true)]
    [InlineData("23:59", true)]
    [InlineData("24:00", false)]
    [InlineData("2 AM", false)]
    [InlineData("", false)]
    public void Parses_schedule_times(string value, bool valid)
    {
        Assert.Equal(valid, ScheduleSettings.TryParseTime(value, out _));
    }

    [Fact]
    public void Parses_extension_lists_the_way_people_type_them()
    {
        Assert.Equal([".epub", ".mobi", ".azw3"], FileCategory.ParseExtensions("epub, .MOBI; *.azw3  epub"));
    }

    [Fact]
    public void Imports_a_1x_settings_export()
    {
        string path = Path.Combine(_ws.Root, "old.tfconfig");
        File.WriteAllText(path, $$"""
            {
              "exportDate": "2025-11-02T10:00:00",
              "exportVersion": "1.0",
              "configuration": {{LegacyConfigJson}},
              "preferences": { "darkMode": true, "minimizeToTray": false, "windowLeft": -1 }
            }
            """);

        var bundle = SettingsBundle.Import(path);

        Assert.Equal(500, bundle.Configuration!.MinFileSizeKB);
        Assert.Equal(AppTheme.Dark, bundle.Preferences!.Theme);
        Assert.False(bundle.Preferences.MinimizeToTray);
        Assert.True(double.IsNaN(bundle.Preferences.WindowLeft));
    }

    [Fact]
    public void Export_then_import_round_trips()
    {
        string path = Path.Combine(_ws.Root, "settings.tfconfig");
        SettingsBundle.Export(path, _ws.Config(c => c.MinFileSizeKB = 3), new UserPreferences { Theme = AppTheme.Light });

        var bundle = SettingsBundle.Import(path);

        Assert.Equal(3, bundle.Configuration!.MinFileSizeKB);
        Assert.Equal(AppTheme.Light, bundle.Preferences!.Theme);
    }

    [Fact]
    public void Rejects_files_that_are_not_settings_exports()
    {
        string path = Path.Combine(_ws.Root, "random.tfconfig");
        File.WriteAllText(path, """{ "hello": "world" }""");

        Assert.Throws<ConfigurationException>(() => SettingsBundle.Import(path));
    }

    [Fact]
    public void Preferences_fall_back_to_defaults_when_corrupt()
    {
        File.WriteAllText(_ws.Paths.PreferencesFile, "garbage");

        var prefs = new PreferencesStore(_ws.Paths).Load();

        Assert.Equal(AppTheme.System, prefs.Theme);
    }
}
