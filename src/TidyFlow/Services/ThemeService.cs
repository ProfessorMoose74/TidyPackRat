using System.Windows;
using TidyFlow.Core.Models;

namespace TidyFlow.Services;

/// <summary>Applies WPF's built-in Fluent theme in light, dark, or follow-Windows mode.</summary>
public static class ThemeService
{
    public static void Apply(AppTheme theme)
    {
        Application.Current.ThemeMode = theme switch
        {
            AppTheme.Light => ThemeMode.Light,
            AppTheme.Dark => ThemeMode.Dark,
            _ => ThemeMode.System,
        };
    }
}
