using System.Windows;
using Forms = System.Windows.Forms;

namespace TidyPackRat
{
    public partial class AddCategoryDialog : Window
    {
        public string CategoryName { get; private set; }
        public string Extensions { get; private set; }
        public string Destination { get; private set; }

        public AddCategoryDialog()
        {
            InitializeComponent();
            Owner = Application.Current.MainWindow;
        }

        private void BrowseDestination_Click(object sender, RoutedEventArgs e)
        {
            using (var dialog = new Forms.FolderBrowserDialog())
            {
                dialog.Description = "Select destination folder for this category";

                if (dialog.ShowDialog() == Forms.DialogResult.OK)
                {
                    txtDestination.Text = dialog.SelectedPath;
                }
            }
        }

        private void Add_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtName.Text))
            {
                MessageBox.Show("Please enter a category name.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(txtExtensions.Text))
            {
                MessageBox.Show("Please enter at least one file extension.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(txtDestination.Text))
            {
                MessageBox.Show("Please select a destination folder.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            CategoryName = txtName.Text.Trim();
            Extensions = txtExtensions.Text.Trim();
            Destination = txtDestination.Text.Trim();

            DialogResult = true;
            Close();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
