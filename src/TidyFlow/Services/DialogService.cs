using System.Windows;
using Microsoft.Win32;
using TidyFlow.Core.Storage;

namespace TidyFlow.Services;

/// <summary>Standard Windows dialogs, kept out of the view models so they stay testable.</summary>
public interface IDialogService
{
    string? PickFolder(string title, string? initialFolder);

    string? PickSettingsFileToOpen();

    string? PickSettingsFileToSave();

    bool Confirm(string title, string message);

    void ShowError(string title, string message);
}

public sealed class DialogService : IDialogService
{
    private const string SettingsFilter = "TidyFlow settings (*.tfconfig)|*.tfconfig";

    public string? PickFolder(string title, string? initialFolder)
    {
        var dialog = new OpenFolderDialog { Title = title };
        string expanded = PathHelper.Expand(initialFolder);
        if (Directory.Exists(expanded))
            dialog.InitialDirectory = expanded;

        return dialog.ShowDialog(Owner) == true ? dialog.FolderName : null;
    }

    public string? PickSettingsFileToOpen()
    {
        var dialog = new OpenFileDialog { Filter = SettingsFilter, Title = "Import TidyFlow settings" };
        return dialog.ShowDialog(Owner) == true ? dialog.FileName : null;
    }

    public string? PickSettingsFileToSave()
    {
        var dialog = new SaveFileDialog
        {
            Filter = SettingsFilter,
            DefaultExt = SettingsBundle.FileExtension,
            FileName = "TidyFlow-Settings",
            Title = "Export TidyFlow settings",
        };
        return dialog.ShowDialog(Owner) == true ? dialog.FileName : null;
    }

    public bool Confirm(string title, string message) =>
        MessageBox.Show(Owner, message, title, MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes;

    public void ShowError(string title, string message) =>
        MessageBox.Show(Owner, message, title, MessageBoxButton.OK, MessageBoxImage.Warning);

    /// <summary>The main window, unless it's hidden in the notification area (a hidden owner would hide the dialog too).</summary>
    private static Window? Owner => Application.Current.MainWindow is { IsVisible: true } window ? window : null;
}
