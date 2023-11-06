using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace StayFocused
{
    /// <summary>
    /// Interaction logic for InputDialog.xaml
    /// </summary>
    public partial class AddFilterDialog : Window
    {
        public string ColumnName { get; }
        FilterType _filterType;
        string _filterText;
        Func<string, bool> _filterFunc = _ => true;

        public AddFilterDialog(GridViewColumn gridViewColumn, Type type)
        {
            InitializeComponent();
            ColumnName = (gridViewColumn.Header as GridViewColumnHeader).Content.ToString();
        }

        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            SetFilterType();
            _filterText = FilterTextBox.Text;
        }

        private void SetFilterType()
        {
            if (StartsWithRadioButton.IsChecked == true)
            {
                _filterType = FilterType.StartsWith;
                _filterFunc = x => x.StartsWith(_filterText, StringComparison.CurrentCultureIgnoreCase);
            }
            else if (ContainsRadioButton.IsChecked == true)
            {
                _filterType = FilterType.Contains;
                _filterFunc = x => x.Contains(_filterText, StringComparison.CurrentCultureIgnoreCase);
            }
            else if (EndsWithRadioButton.IsChecked == true)
            {
                _filterType = FilterType.EndsWith;
                _filterFunc = x => x.EndsWith(_filterText, StringComparison.CurrentCultureIgnoreCase);
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        internal Func<ActivitySummary, bool> GetFilter()
        {
            switch (ColumnName)
            {
                case "Process Name":
                    return x => _filterFunc(x.ProcessName);
                case "Window Title":
                    return x => _filterFunc(x.WindowTitle);
                default:
                    break;
            }
            return _ => true;
        }


        enum FilterType
        {
            StartsWith,
            Contains,
            EndsWith
        }
    }
}
