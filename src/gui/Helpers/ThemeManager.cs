using System;
using System.Windows;

namespace TidyFlow.Helpers
{
    /// <summary>
    /// Manages application themes (Light/Dark mode)
    /// </summary>
    public static class ThemeManager
    {
        private static bool _isDarkMode;

        /// <summary>
        /// Gets whether dark mode is currently active
        /// </summary>
        public static bool IsDarkMode => _isDarkMode;

        /// <summary>
        /// Apply the specified theme
        /// </summary>
        public static void ApplyTheme(bool darkMode)
        {
            _isDarkMode = darkMode;

            var app = Application.Current;
            if (app == null) return;

            // Remove existing theme dictionaries
            ResourceDictionary themeToRemove = null;
            foreach (var dict in app.Resources.MergedDictionaries)
            {
                if (dict.Source != null &&
                    (dict.Source.OriginalString.Contains("LightTheme") ||
                     dict.Source.OriginalString.Contains("DarkTheme")))
                {
                    themeToRemove = dict;
                    break;
                }
            }

            if (themeToRemove != null)
            {
                app.Resources.MergedDictionaries.Remove(themeToRemove);
            }

            // Add new theme
            var themePath = darkMode
                ? "Themes/DarkTheme.xaml"
                : "Themes/LightTheme.xaml";

            var themeDict = new ResourceDictionary
            {
                Source = new Uri(themePath, UriKind.Relative)
            };

            app.Resources.MergedDictionaries.Insert(0, themeDict);
        }

        /// <summary>
        /// Toggle between light and dark mode
        /// </summary>
        public static void ToggleTheme()
        {
            ApplyTheme(!_isDarkMode);
        }

        /// <summary>
        /// Detect Windows system theme preference
        /// </summary>
        public static bool GetSystemThemePreference()
        {
            try
            {
                using (var key = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(
                    @"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize"))
                {
                    if (key != null)
                    {
                        var value = key.GetValue("AppsUseLightTheme");
                        if (value != null)
                        {
                            return (int)value == 0; // 0 = dark mode
                        }
                    }
                }
            }
            catch
            {
                // Ignore registry access errors
            }

            return false; // Default to light mode
        }
    }
}
