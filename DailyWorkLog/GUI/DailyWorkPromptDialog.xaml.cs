using System.Windows;

namespace DailyWorkLog.GUI;

public partial class DailyWorkPromptDialog : Window
{
    public string WorkText => WorkTextBox.Text;

    public DailyWorkPromptDialog()
    {
        InitializeComponent();
        WorkTextBox.Focus();
    }

    private void OnOkClick(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(WorkTextBox.Text))
        {
            System.Windows.MessageBox.Show(
                "Please enter what you worked on today.",
                "Daily Work Log",
                MessageBoxButton.OK,
                MessageBoxImage.Warning);
            return;
        }

        DialogResult = true;
    }

    private void OnCancelClick(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
    }
}
