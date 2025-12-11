using System.Windows;

namespace TidyFlow
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // Check for command line arguments
            // Could be used for /install, /config, etc.
            if (e.Args.Length > 0)
            {
                // Handle command line arguments if needed
            }
        }
    }
}
