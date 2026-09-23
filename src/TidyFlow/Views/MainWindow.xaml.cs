using System.ComponentModel;
using System.Windows;
using TidyFlow.ViewModels;

namespace TidyFlow.Views;

/// <summary>
/// The main window. Behavior lives in <see cref="MainViewModel"/>; this only handles window chrome:
/// placement, hiding to the notification area, and refreshing when brought back.
/// </summary>
public partial class MainWindow : Window
{
    private readonly MainViewModel _viewModel;

    public MainWindow(MainViewModel viewModel)
    {
        InitializeComponent();

        _viewModel = viewModel;
        DataContext = viewModel;
        viewModel.ShowPreview = ShowPreview;

        RestorePlacement();

        Activated += (_, _) =>
        {
            // A scheduled run in another process may have changed things while we were hidden.
            _viewModel.RefreshActivity();
            _viewModel.RefreshScheduleStatus();
        };
        IsVisibleChanged += (_, _) => _viewModel.IsWindowVisible = IsVisible;
        StateChanged += (_, _) =>
        {
            if (WindowState == WindowState.Minimized && _viewModel.MinimizeToTray)
                Hide();
        };
    }

    /// <summary>Set when the user chooses Exit, so closing really closes.</summary>
    public bool IsExiting { get; set; }

    public void ShowAndActivate()
    {
        Show();
        if (WindowState == WindowState.Minimized)
            WindowState = WindowState.Normal;
        Activate();
        Topmost = true;  // Bring to front even when another app has focus.
        Topmost = false;
        Focus();
    }

    protected override void OnClosing(CancelEventArgs e)
    {
        SavePlacement();

        if (!IsExiting && _viewModel.MinimizeToTray)
        {
            e.Cancel = true;
            Hide();
            return;
        }

        if (_viewModel.IsDirty
            && MessageBox.Show(this, "Save your changes before closing?", "TidyFlow", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
        {
            _viewModel.SaveCommand.Execute(null);
        }

        base.OnClosing(e);
    }

    private bool ShowPreview(PreviewViewModel preview)
    {
        var window = new PreviewWindow(preview);
        if (IsVisible)
            window.Owner = this;
        else
            window.WindowStartupLocation = WindowStartupLocation.CenterScreen;
        return window.ShowDialog() == true;
    }

    private void RestorePlacement()
    {
        var prefs = _viewModel.Preferences;
        Width = prefs.WindowWidth;
        Height = prefs.WindowHeight;

        bool hasPosition = !double.IsNaN(prefs.WindowLeft) && !double.IsNaN(prefs.WindowTop);
        bool onScreen = hasPosition
            && prefs.WindowLeft >= SystemParameters.VirtualScreenLeft
            && prefs.WindowTop >= SystemParameters.VirtualScreenTop
            && prefs.WindowLeft + 100 <= SystemParameters.VirtualScreenLeft + SystemParameters.VirtualScreenWidth
            && prefs.WindowTop + 100 <= SystemParameters.VirtualScreenTop + SystemParameters.VirtualScreenHeight;

        if (onScreen)
        {
            WindowStartupLocation = WindowStartupLocation.Manual;
            Left = prefs.WindowLeft;
            Top = prefs.WindowTop;
        }
        else
        {
            WindowStartupLocation = WindowStartupLocation.CenterScreen;
        }
    }

    private void SavePlacement()
    {
        if (WindowState != WindowState.Normal || !IsLoaded)
            return;

        var prefs = _viewModel.Preferences;
        prefs.WindowWidth = Width;
        prefs.WindowHeight = Height;
        prefs.WindowLeft = Left;
        prefs.WindowTop = Top;
        _viewModel.SavePreferences();
    }
}
