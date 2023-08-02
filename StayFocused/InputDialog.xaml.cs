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
   public partial class InputDialog : Window
    {
        public string InputValue { get; private set; }
        
        public InputDialog(string promptMessage)
        {
            InitializeComponent();
            PromptTextBlock.Text = promptMessage;
        }

        private void OnOkClick(object sender, RoutedEventArgs e)
        {
            InputValue = inputTextBox.Text;
            DialogResult = true;
        }

        private void OnCancelClick(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}
