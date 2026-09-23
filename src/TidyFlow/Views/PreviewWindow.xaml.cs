using System.Windows;
using TidyFlow.ViewModels;

namespace TidyFlow.Views;

public partial class PreviewWindow : Window
{
    public PreviewWindow(PreviewViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }

    private void Organize_Click(object sender, RoutedEventArgs e) => DialogResult = true;
}
