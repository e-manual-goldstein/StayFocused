using System.Windows;

namespace DailyWorkLog.GUI;

public partial class WorkItemIdDialog : Window
{
    public int WorkItemId { get; private set; }

    public WorkItemIdDialog(int? defaultId = null)
    {
        InitializeComponent();
        if (defaultId.HasValue && defaultId.Value > 0)
            IdTextBox.Text = defaultId.Value.ToString();
        IdTextBox.Focus();
    }

    private void OnGetClick(object sender, RoutedEventArgs e)
    {
        if (!int.TryParse(IdTextBox.Text, out var id) || id <= 0)
        {
            System.Windows.MessageBox.Show(
                "Enter a valid work item ID.",
                "Get work item",
                MessageBoxButton.OK,
                MessageBoxImage.Warning);
            return;
        }

        WorkItemId = id;
        DialogResult = true;
    }

    private void OnCancelClick(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
    }
}
